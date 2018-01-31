﻿using Npgsql;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for FinalAssy2Window.xaml
    /// </summary>
    public partial class FinalAssy2Window : Window
    {
        public bool AllFieldsValidated { get; set; } = false;

        public bool IsDmValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public bool IsPreChkPassed { get; set; } = false;

        public FinalAssy2Window()
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
                FormValidator();
                SaveBtn_Click(sender, e);
            }

            DmValidator();
        }

        private void FormValidator()
        {
            if (IsDmValidated == true && screwChkbx.IsChecked == true)
            {
                PreChk("calibration", "housing_dm", HousingDmTxbx.Text);
                if (IsPreChkPassed)
                {
                    AllFieldsValidated = true;
                }
                else
                    CallMessageForm("Előző munkafolyamaton nem szerepelt a Ház!");
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
            if (RegexValidation(HousingDmTxbx.Text, "MbDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            HousingDmTxbx.Text = "";
            GwDmTxbx.Text = "";
            screwChkbx.IsChecked = false;
            IsPreChkPassed = false;
            HousingDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void PreChk(string previousTable, string columnToSearch, string dataToFind)
        {
            string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            var conn = new NpgsqlConnection(connstring);
            conn.Open();
            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM " + previousTable + " WHERE " + columnToSearch + " = :dataToFind", conn);
            cmd.Parameters.Add(new NpgsqlParameter("dataToFind", dataToFind));
            Int32 countProd = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            if (countProd == 1)
            {
                IsPreChkPassed = true;
            }
            else
            {
                IsPreChkPassed = false;
            }
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
                CallMessageForm("Adatok feltöltve!");
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                ResetForm();
            }
        }

        private void HousingDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            StartedOn = DateTime.Now;
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
                DbInsert("final_assy_two");
            }
            ResetForm();
        }
    }
}
