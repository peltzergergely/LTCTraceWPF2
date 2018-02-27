using Microsoft.Win32;
using Npgsql;
using System;
using System.Configuration;
using System.IO;
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
                FormValidator();
                SaveBtn_Click(sender, e);
            }
            DmValidator();
        }

        private void FormValidator()
        {
            if (IsDmValidated == true)
            {
                PreChk("final_assy_one");
                //IsPreChkPassed = true;
                if (IsPreChkPassed)
                {
                    AllFieldsValidated = true;
                }
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
            screwChkbx.IsChecked = false;
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

        private void PreChk(string previousTable)
        {
            string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            var conn = new NpgsqlConnection(connstring);
            conn.Open();
            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM " + previousTable + " WHERE housing_dm = :housing_dm", conn);
            cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
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
                if (openFileDialog.FileName == "")
                    LaunchFiledialog();

                FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                var fileToByteArr = new byte[fs.Length];
                fs.Read(fileToByteArr, 0, Convert.ToInt32(fs.Length));
                fs.Close();


                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, pc_name, started_on, saved_on, filename, file, filename1, file1) " +
                    "VALUES(:housing_dm, :pc_name, :started_on, :saved_on, :filename, :file, :filename1, :file1)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                cmd.Parameters.Add(new NpgsqlParameter("filename", openFileDialog.SafeFileName));
                cmd.Parameters.Add(new NpgsqlParameter("file", fileToByteArr));
                LaunchFiledialog();
                cmd.Parameters.Add(new NpgsqlParameter("filename1", openFileDialog.SafeFileName));
                cmd.Parameters.Add(new NpgsqlParameter("file1", fileToByteArr));

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
            if (AllFieldsValidated)
            {
                DbInsert("calibration");
            }
        }

        private void FbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            StartedOn = DateTime.Now;
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        private void LaunchFiledialog()
        {
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.InitialDirectory = @"D:\";
            openFileDialog.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LaunchFiledialog();
        }
    }
}
