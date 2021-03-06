﻿using ErrorLogging;
using Npgsql;
using System;
using System.Configuration;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for FinalAssy1Window.xaml
    /// </summary>
    public partial class FinalAssy1Window : Window
    {
        public bool AllFieldsValidated { get; set; } = false;

        public bool IsDmValidated { get; set; } = false;

        public bool IsHousingDmValidated { get; set; } = false;
        public bool IsGwDmValidated { get; set; } = false;
        public DateTime? StartedOn { get; set; } = null;

        public bool IsPreChkPassed { get; set; } = false;

        public FinalAssy1Window()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        private void OnKeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter && HousingDmTxbx.Text.Length > 0)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;

            }

            if (Keyboard.FocusedElement == SaveBtn)
            {
                SaveBtn_Click(sender, e);
            }
            DmValidator();
        }

        private void FormValidator()
        {
            var preChecker = new DatabaseHelper();
            if (IsDmValidated == true && IsHousingDmValidated == true && IsGwDmValidated == true)
            {
                AllFieldsValidated = true;
            }
            else
            {
                CallMessageForm("Hibás kitöltés");
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void DmValidator()
        {
            if (RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx"))
                IsHousingDmValidated = true;
            else
                IsHousingDmValidated = false;

            if (RegexValidation(MbDmTxbx.Text, "MbDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;

            if (RegexValidation(GwDmTxbx.Text, "GwDmRegEx"))
                IsGwDmValidated = true;
            else
                IsGwDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            IsGwDmValidated = false;
            AllFieldsValidated = false;
            HousingDmTxbx.Text = "";
            MbDmTxbx.Text = "";
            GwDmTxbx.Text = "";
            IsPreChkPassed = false;
            HousingDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void DbInsert(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, mb_dm, pc_name, started_on, saved_on) " +
                    "VALUES(:housing_dm, :mb_dm, :pc_name, :started_on, :saved_on)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                //Resultlbl.Text = "Adatok elmentve! " + DateTime.Now;
                //ResetForm();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                ResetForm();
            }
        }

        private void DbInsertGw(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, gw_dm, pc_name, started_on, saved_on) " +
                    "VALUES(:housing_dm, :gw_dm, :pc_name, :started_on, :saved_on)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("gw_dm", GwDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                Resultlbl.Text = "Adatok elmentve! " + DateTime.Now;
                ResetForm();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                ResetForm();
            }
        }

        private void HousingDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            string table = "";

            if (ConfigurationManager.AppSettings["HousingConnectorAssy"] == "true")
                table = "housing_connector_assy";
            else
                table = "potting";

            if (HousingDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB(table, "housing_dm", HousingDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        ErrorLog.Create(table, "housing_dm", HousingDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a termék!", this.GetType().Name.ToString());
                    }
                }
                StartedOn = DateTime.Now;
            }
        }

        private void MbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (MbDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("mb_dsp_assy", "mb_dm", MbDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a Mainboard!");
                    }
                    else
                    {
                        ErrorLog.Create("mb_dsp_assy", "mb_dm", MbDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a Mainboard!", this.GetType().Name.ToString());
                    }
                }
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            FormValidator();

            if (AllFieldsValidated)
            {
                DbInsert("final_assy_one");
                DbInsertGw("final_assy_two");

            }
        }

    }
}
