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
    /// Interaction logic for PottingWindow.xaml
    /// </summary>
    public partial class HousingFbAssy : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsCameraLaunched { get; set; } = false;

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
                PreChk("housing_leak_test_one", "housing_dm", HousingDmTxbx.Text);
                if (IsPreChkPassed)
                {
                    PreChk("fb_emc_assy", "fb_dm", FbDmTxbx.Text);
                    if (IsPreChkPassed)
                    {
                        if (Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg").Length > 3)
                        {
                            AllFieldsValidated = true;
                        }
                    }
                    else errorMsg += "Előző munkafolyamaton nem szerepelt a Filterboard! ";
                }
                else
                    errorMsg += " Előző munkafolyamaton nem szerepelt a Ház! ";
            }
            if (IsDmValidated == false)
            {
                errorMsg += " DataMátrix nem megfelelő! ";
            }
            if (IsCameraLaunched == false)
            {
                errorMsg += " Kamera nem volt elindítva! ";
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
            IsCameraLaunched = false;
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
            System.IO.Directory.CreateDirectory(@"c:\TraceImages\");
            FilePathStr = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");
            int imgArrayLimit = 9;
            if (FilePathStr.Length > imgArrayLimit)
            {
                MessageBox.Show("A készített képek száma meghaladja a maximum 9 képes limitet! Töröld ki a fölösleget a TraceImages mappából!'");
                System.Diagnostics.Process.Start("explorer.exe", "C:\\TraceImages\\");
            }
            else
            {
                imgArrayLimit = FilePathStr.Length;
                try
                {
                    byte[][] imgByteArray = new byte[9][];
                    for (int i = 0; i < imgArrayLimit; i++)
                    {
                        FileStream fs = new FileStream(FilePathStr[i], FileMode.Open, FileAccess.Read);
                        imgByteArray[i] = new byte[fs.Length];
                        fs.Read(imgByteArray[i], 0, Convert.ToInt32(fs.Length));
                        fs.Close();
                    }

                    using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ltctrace.dbconnectionstring"].ConnectionString))
                    {
                        conn.Open();
                        var cmd = new NpgsqlCommand("insert into " + table + " (housing_dm, fb_dm, pc_name, started_on, saved_on, pic1, pic2, pic3, pic4, pic5, pic6, pic7, pic8, pic9) " +
                        "values(:housing_dm, :fb_dm, :pc_name, :started_on, :saved_on, :pic1, :pic2, :pic3, :pic4, :pic5, :pic6, :pic7, :pic8, :pic9)", conn);
                        cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("fb_dm", FbDmTxbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("pc_name", Environment.MachineName));
                        cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                        cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                        //uploading the pictures
                        for (int i = 0; i < 9; i++)
                        {
                            if (i < FilePathStr.Length)
                                cmd.Parameters.Add(new NpgsqlParameter("pic" + (i + 1).ToString(), imgByteArray[i]));
                            else //making them empty
                            {
                                imgByteArray[i] = new byte[0];
                                cmd.Parameters.Add(new NpgsqlParameter("pic" + (i + 1).ToString(), imgByteArray[i]));
                            }
                        }

                        int result = cmd.ExecuteNonQuery();
                        if (result == 1)
                        {
                            FilePathStr = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");
                            Directory.CreateDirectory("C:\\TraceImagesArchive\\" + "HOUSINGDM_" + HousingDmTxbx.Text);
                            for (int i = 0; i < FilePathStr.Length; i++)
                            {
                                File.Move(FilePathStr[i], "C:\\TraceImagesArchive\\" + "HOUSINGDM_" + HousingDmTxbx.Text + "\\" + Path.GetFileName(FilePathStr[i]));
                            }
                            CallMessageForm("Adatok elmentve!");
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
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
            IsCameraLaunched = true;
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
