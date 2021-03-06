﻿using ErrorLogging;
using Microsoft.Win32;
using Npgsql;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for CalibWindow.xaml
    /// </summary>
    public partial class CalibWindow : Window
    {
        public bool AllFieldsValidated { get; set; } = false;

        public bool IsDmValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public bool IsPreChkPassed { get; set; } = false;

        OpenFileDialog openFileDialog = new OpenFileDialog();

        public CalibWindow()
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
            if (IsDmValidated == true)
            {
                AllFieldsValidated = true;
            }
            else
                CallMessageForm("Hibás kitöltés");
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void DmValidator()
        {
            if (RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            HousingDmTxbx.Text = "";
            TestResultChkbx.IsChecked = false;
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
                if (openFileDialog.FileName == "")
                    LaunchFiledialog();

                FileStream file1 = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                var fileToByteArr = new byte[file1.Length];
                file1.Read(fileToByteArr, 0, Convert.ToInt32(file1.Length));
                file1.Close();

                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, test_result, internal_id, pc_name, started_on, saved_on, filename, file, filename1, file1) " +
                    "VALUES(:housing_dm, :test_result, :internal_id, :pc_name, :started_on, :saved_on, :filename, :file, :filename1, :file1)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("test_result", TestResultChkbx.IsChecked));
                cmd.Parameters.Add(new NpgsqlParameter("internal_id", InternalDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                cmd.Parameters.Add(new NpgsqlParameter("filename", openFileDialog.SafeFileName));
                cmd.Parameters.Add(new NpgsqlParameter("file", fileToByteArr));
                LaunchFiledialog();
                FileStream file2 = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                var fileToByteArr2 = new byte[file2.Length];
                file2.Read(fileToByteArr, 0, Convert.ToInt32(file2.Length));
                file2.Close();
                cmd.Parameters.Add(new NpgsqlParameter("filename1", openFileDialog.SafeFileName));
                cmd.Parameters.Add(new NpgsqlParameter("file1", fileToByteArr2));

                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                CallMessageForm("Adatok feltöltve!");
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            FormValidator();
            if (AllFieldsValidated)
            {
                DbInsert("calibration");
            }
        }

        private void FbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HousingDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("hipot_test_one", "housing_dm", HousingDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        ErrorLog.Create("hipot_test_one", "housing_dm", HousingDmTxbx.Text, MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a termék!", this.GetType().Name.ToString());
                    }
                }
                StartedOn = DateTime.Now;
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LaunchFiledialog()
        {
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LaunchFiledialog();
        }
    }
}
