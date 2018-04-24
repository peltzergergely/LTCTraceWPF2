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
    /// Interaction logic for FirewallWindow.xaml
    /// </summary>
    public partial class FirewallWindow : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsCameraLaunched { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public string[] FilePathStr; // = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");

        public FirewallWindow()
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
                if (Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg").Length > 2)
                {
                    AllFieldsValidated = true;
                }
                else errorMsg += "Legalább 3 képnek kell készülnie";
            }
            if (IsDmValidated == false)
            {
                errorMsg += "DataMátrix nem megfelelő! ";
            }
            if (IsCameraLaunched == false)
            {
                errorMsg += "Kamera nem volt elindítva! ";
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
            if (RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx") == true)
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
            HousingDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void DbInsert(string table)
        {
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
                        var cmd = new NpgsqlCommand("insert into " + table + " (housing_dm, pc_name, started_on, saved_on, label_one, label_two, pic1, pic2, pic3, pic4, pic5, pic6, pic7, pic8, pic9) " +
                        "values(:housing_dm, :pc_name, :started_on, :saved_on, :label_one, :label_two, :pic1, :pic2, :pic3, :pic4, :pic5, :pic6, :pic7, :pic8, :pic9)", conn);
                        cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                        cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                        cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                        cmd.Parameters.Add(new NpgsqlParameter("label_one", Label1Txbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("label_two", Label2Txbx.Text));
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
                            System.IO.Directory.CreateDirectory("C:\\TraceImagesArchive\\" + "HOUSINGDM_" + HousingDmTxbx.Text);
                            for (int i = 0; i < FilePathStr.Length; i++)
                            {
                                File.Move(FilePathStr[i], "C:\\TraceImagesArchive\\" + "HOUSINGDM_" + HousingDmTxbx.Text + "\\" + Path.GetFileName(FilePathStr[i]));
                            }
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
        }

        private void HousingDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HousingDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("eol", "housing_dm", HousingDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        ErrorLog.Create("eol", "housing_dm", HousingDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a termék!", this.GetType().Name.ToString());
                    }
                }
                StartedOn = DateTime.Now;
            }
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
                DbInsert("firewall");
            }
        }
    }
}
