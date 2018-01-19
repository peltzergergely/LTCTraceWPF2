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

            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }

            if (Keyboard.FocusedElement == FbDmTxbx && FbDmTxbx.Text.Length > 0) //calls the validator for the field in focus
            {
                FbDmValidator();
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void FbDmValidator()
        {
            if (RegexValidation(FbDmTxbx.Text, "FbDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            IsCameraLaunched = false;
            FbDmTxbx.Text = "";
            FbDmTxbx.Focus();
        }

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            SaveBtn.Focus();
            IsCameraLaunched = true;
            var webCam = new camApp();
            webCam.Show();
        }

        private void ValidateAll()
        {
            if (IsDmValidated == true || IsCameraLaunched == true)
                AllFieldsValidated = true;
            else
                CallMessageForm("Hibás kitöltés");
        }

        private void DbInsert(string table) //DB insert
        {
            try
            {
                //ez itt tartja fogva a fileneveket amik nekem kellenek
                string[] filePaths = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");
                int i = 0;


                //ez itten ez olvassa be a filenevet
                FileStream fs = new FileStream(filePaths[0], FileMode.Open, FileAccess.Read);
                //ez itten hozza létre a megfelelő nagyságú byte tömböt
                byte[] imgByteArr = new byte[fs.Length];
                //ez meg bélerakja
                fs.Read(imgByteArr, 0, Convert.ToInt32(fs.Length));
                //oszt becsukluk
                fs.Close();

                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (fb_dm, pc_name, started_on, saved_on, pic1, pic2, pic3, pic4, pic5, pic6) " +
                    "VALUES(:fb_dm, :pc_name, :started_on, :saved_on, :pic0, :pic1, :pic2, :pic3, :pic4)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("fb_dm", FbDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                //eztet kell itten ciklusba tenni e.
                cmd.Parameters.Add(new NpgsqlParameter("pic" + i.ToString(), imgByteArr));
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

        //        if (FilePathStr != "")
        //        {
        //            //Initialize a file stream to read the image file
        //            FileStream fs = new FileStream(FilePathStr, FileMode.Open, FileAccess.Read);

        //            //Initialize a byte array with size of stream
        //            byte[] imgByteArr = new byte[fs.Length];

        //            //Read data from the file stream and put into the byte array
        //            fs.Read(imgByteArr, 0, Convert.ToInt32(fs.Length));

        //            //Close a file stream
        //            fs.Close();

        //            using (NpgsqlConnection conn = new NpgsqlConnection(constr))
        //            {
        //                conn.Open();
        //                string sql = "insert into picturetable(photo) values(@img)";
        //                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
        //                {
        //                    //Pass byte array into database
        //                    cmd.Parameters.Add(new NpgsqlParameter("img", imgByteArr));
        //                    int result = cmd.ExecuteNonQuery();
        //                    if (result == 1)
        //                    {
        //                        MessageBox.Show("Kép elmentve");
        //                    }
        //                }
        //                conn.Close();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllFieldsValidated)
            {
            }
            //UploadToDb();
        }

        private void FbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            StartedOn = DateTime.Now;
        }
    }
}
