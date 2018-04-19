using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for FbEmcWindow.xaml
    /// </summary>
    public partial class FbEmcWindow : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsCameraLaunched { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public string[] FilePathStr;

        public FbEmcWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

            if (e.Key == Key.Enter && FbDmTxbx.Text.Length > 0)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }

            if (Keyboard.FocusedElement == SaveBtn) //calls the validator for the field in focus
            {
                SaveBtn_Click(sender, e);
            }
        }


        //fb_acdc_assy
        private void FormValidator()
        {
            Directory.CreateDirectory(@"c:\TraceImages\");
            string errorMsg = "";
            if (IsDmValidated == true)
            {
                if (Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg").Length > 2)
                {
                    AllFieldsValidated = true;
                }
                else
                {
                    errorMsg += "Nem készült elég kép! ";
                }
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
            if (RegexValidation(FbDmTxbx.Text, "FbDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }
        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            IsCameraLaunched = false;
            FbDmTxbx.Text = "";
            FbDmTxbx.Focus();
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
                MessageBox.Show("A készített képek száma meghaladja a maximum 9 képes limitet! Töröld ki a fölösleget a C:\\TraceImages mappából!'");
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
                        var cmd = new NpgsqlCommand("insert into " + table + " (fb_dm, pc_name, started_on, saved_on, pic1, pic2, pic3, pic4, pic5, pic6, pic7, pic8, pic9) " +
                        "values(:fb_dm, :pc_name, :started_on, :saved_on, :pic1, :pic2, :pic3, :pic4, :pic5, :pic6, :pic7, :pic8, :pic9)", conn);
                        cmd.Parameters.Add(new NpgsqlParameter("fb_dm", FbDmTxbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
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
                            System.IO.Directory.CreateDirectory("C:\\TraceImagesArchive\\" + "FBDM_" + FbDmTxbx.Text);
                            for (int i = 0; i < FilePathStr.Length; i++)
                            {
                                File.Move(FilePathStr[i], "C:\\TraceImagesArchive\\" + "FBDM_" + FbDmTxbx.Text + "\\" + Path.GetFileName(FilePathStr[i]));
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

        private void FbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            DmValidator();
            if (ConfigurationManager.AppSettings["FbAcdcAssy"] == "true" && FbDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("fb_acdc_assy", "fb_dm", FbDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        MessageBoxResult messageBoxResult = MessageBox.Show("Előző munkafolyamaton nem szerepelt a termék! Folytatáshoz nyomd meg a SPACE billentyűt!", "Interlock hiba!", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (messageBoxResult == MessageBoxResult.No)
                        {
                            CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                        }
                    }
                }
                else
                {
                    WebCamLaunchClick(sender, e);
                }
            }

            StartedOn = DateTime.Now;
        }

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                // very long task
                SaveBtn.Focus();
                var webCam = new camApp();
                webCam.Show();
                IsCameraLaunched = true;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            FormValidator();
            if (AllFieldsValidated)
            {
                DbInsert("fb_emc_assy");
            }
        }
    }
}
