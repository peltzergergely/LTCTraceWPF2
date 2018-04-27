using ErrorLogging;
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
    /// Interaction logic for PottingWindow.xaml
    /// </summary>
    public partial class HousingFbAssy : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public bool IsPreChkPassed { get; set; } = false;

        public string[] FilePathStr { get; set; }//Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");

        public HousingFbAssy()
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
            string errorMsg = "";
            if (IsDmValidated == true)
            {
                if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                {
                    PreChk("housing_leak_test_one", "housing_dm", HousingDmTxbx.Text);

                    if (IsPreChkPassed)
                    {
                        PreChk("fb_emc_assy", "fb_dm", FbDmTxbx.Text);
                        if (IsPreChkPassed)
                        {
                            AllFieldsValidated = true;
                        }
                        else errorMsg += "Előző munkafolyamaton nem szerepelt a Filterboard! ";
                    }
                    else
                        errorMsg += " Előző munkafolyamaton nem szerepelt a Ház! ";
                }
                else
                {
                    PreChk("housing_leak_test_one", "housing_dm", HousingDmTxbx.Text);

                    if(!IsPreChkPassed)
                    {
                        ErrorLog.Create("housing_leak_test_one", "housing_dm", HousingDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a Ház!", this.GetType().Name.ToString());
                        IsPreChkPassed = true;
                    }

                    if (IsPreChkPassed)
                    {
                        PreChk("fb_emc_assy", "fb_dm", FbDmTxbx.Text);

                        if (!IsPreChkPassed)
                        {
                            ErrorLog.Create("fb_emc_assy", "fb_dm", FbDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a Filterboard!", this.GetType().Name.ToString());
                            IsPreChkPassed = true;
                        }

                        AllFieldsValidated = true;  
                    }
                }
            }

            if (IsDmValidated == false)
            {
                errorMsg += " DataMátrix nem megfelelő! ";
            }

            if (errorMsg != "")
            {
                CallMessageForm(errorMsg);
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void DmValidator()
        {
            if (RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx") == true
                && RegexValidation(FbDmTxbx.Text, "FbDmRegEx") == true)
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            HousingDmTxbx.Text = "";
            FbDmTxbx.Text = "";
            HousingDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
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
            if (countProd > 0)
            {
                IsPreChkPassed = true;
            }
            else
            {
                IsPreChkPassed = false;
            }
        }

        private void DbInsert(string table)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ltctrace.dbconnectionstring"].ConnectionString))
                {
                    conn.Open();
                    var cmd = new NpgsqlCommand("insert into " + table + " (housing_dm, fb_dm, pc_name, started_on, saved_on) " +
                    "values(:housing_dm, :fb_dm, :pc_name, :started_on, :saved_on)", conn);
                    cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                    cmd.Parameters.Add(new NpgsqlParameter("fb_dm", FbDmTxbx.Text));
                    cmd.Parameters.Add(new NpgsqlParameter("pc_name", Environment.MachineName));
                    cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                    cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                    //uploading the pictures

                    int result = cmd.ExecuteNonQuery();
                    if (result == 1)
                    {
                        Resultlbl.Text = "Adatok elmentve! " + DateTime.Now;
                        ResetForm();
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            SaveBtn.Focus();
            var webCam = new camApp();
            webCam.Show();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            FormValidator();

            if (AllFieldsValidated)
            {
                DbInsert("housing_fb_assy");
            }
        }
    }
}
