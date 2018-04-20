using ErrorLogging;
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
    /// Interaction logic for EOLTest.xaml
    /// </summary>
    public partial class EOLTest : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public string tableName { get; set; } = "eol";

        OpenFileDialog openFileDialog = new OpenFileDialog();


        public EOLTest()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        //Handles focus movement and checks validator
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
                AllFieldsValidated = true;
            }
            else if (IsDmValidated == false)
            {
                CallMessageForm("HIBA: DataMátrix nem megfelelő!");
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
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            HousingDmTxbx.Text = "";
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
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, test_result, internal_id, pc_name, started_on, saved_on, filename, file, filename1, file1) " +
                    "VALUES(:housing_dm, :test_result, :internal_id, :pc_name, :started_on, :saved_on, :filename, :file, :filename1, :file1)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("test_result", TestResultCkhbx.IsChecked));
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

        private void LaunchFiledialog()
        {
            openFileDialog.Filter = "All files (*.*)|*.*";
            openFileDialog.InitialDirectory = @"C:\";
            openFileDialog.ShowDialog();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllFieldsValidated)
            {
                DbInsert(tableName);
            }
        }

        private void FbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HousingDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("hipot_test_two", "housing_dm", HousingDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        //MessageBoxResult messageBoxResult = MessageBox.Show("Előző munkafolyamaton nem szerepelt a termék! Folytatáshoz nyomd meg a SPACE billentyűt!", "Interlock hiba!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        //if (messageBoxResult == MessageBoxResult.No)
                        //{
                        //    CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                        //}
                        ErrorLog.Create("hipot_test_two", "housing_dm", HousingDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a termék!");
                    }
                }
                StartedOn = DateTime.Now;
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
