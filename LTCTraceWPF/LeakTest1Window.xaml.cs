using Npgsql;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for HousingLeakTestWindow.xaml
    /// Get leak test result in number then print a unique ID
    /// option to reprint and database to store value
    /// table with running number, always just add + 1?
    /// </summary>
    public partial class LeakTest1Win : Window
    {
        public bool IsDmValidated { get; set; } = true;
        public bool AllFieldsValidated { get; set; } = false;
        public string HousingDM { get; set; }
        public int SerialNumber { get; set; }
        public double Number { get; set; } = 0;
        public bool AutoID { get; set; }


        public LeakTest1Win()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
            GetAutoID();
            RefreshAutoIdSetting();
        }

        private void GetAutoID()
        {
            if (ConfigurationManager.AppSettings["AutoGenerateId"] == "true")
                AutoID = true;
            else
                AutoID = false;
        }

        private void RefreshAutoIdSetting()
        {
            if (AutoID)
            {
                autoIdBtn.Background = new SolidColorBrush(Colors.LightGreen);
                autoIdBtnIcon.Text = "\xE73E";
            }
            else
            {
                autoIdBtn.Background = new SolidColorBrush(Colors.LightGray);
                autoIdBtnIcon.Text = "\xE711";
            }
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

            if (e.Key == Key.Enter && leakTestTxbx.Text.Length > 0)
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
        }

        private void ResetForm()
        {
            AllFieldsValidated = false;
            leakTestTxbx.Text = "";
            if (AutoID)
                leakTestTxbx.Focus();
            else
                StartingIdTxbx.Focus();
        }

        private void FormValidator()
        {
            bool isNum = double.TryParse(leakTestTxbx.Text, out double Num);

            if (isNum)
            {
                if (Num < 5)
                {
                    this.Number = Num;
                    AllFieldsValidated = true;
                }
                else Resultlbl.Text = "Hiba a bevitelben: " + Num.ToString(); 
            }
            else
            {
                CallMessageForm("Hibás kitöltés");
            }
        }

        private void DbInsert(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                var UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, leak_test_result, pc_name, saved_on) " +
                    "VALUES(:housing_dm, :leak_test_result, :pc_name, :timestamp)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDM));
                cmd.Parameters.Add(new NpgsqlParameter("leak_test_result", Number));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("timestamp", UploadMoment));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                //CallMessageForm("Adatok feltöltve!");
                Resultlbl.Text = "Adatok feltöltve! " + UploadMoment;
                ResetForm();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void PrintDMC(int number)
        {
            string runningtext = "LTC";
            if (number > 0)
            {
                HousingDM = runningtext + number.ToString();
                CounterTxbx.Text = HousingDM;
                string s = @"^XA^MMT^PW406^LL0280^LS0^BY252,252^FT16,266^BXN,18,200,0,0,1,~^FH\^FD" + @HousingDM + @"^FS^FT345,274^A0B,25,26^FH\^FDLTC 1E0002187 AD B2.5^FS^FT376,274^A0B,25,26^FH\^FD" + @HousingDM + @"^FS^PQ1,0,1,Y^XZ";
                string printerName = "ZDesigner ZT420-203dpi ZPL";
                RawPrinterHelper.SendStringToPrinter(printerName, s);
            }
            else
            {
                CallMessageForm("Hiba a sorszámkiosztásnál");
            }
        }

        private int GetNextNumber()
        {
            IncreaseCounterDB();
            int ID = int.Parse(StartingIdTxbx.Text);

            if (AutoID)
            {
                StartingIdTxbx.Text = (ID + 1).ToString();
            }

            return ID;
        }

        private void IncreaseCounterDB()
        {
            try
            {
                int ID = int.Parse(StartingIdTxbx.Text);

                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                var UploadMoment = DateTime.Now;
                conn.Open();

                var cmd = new NpgsqlCommand("INSERT INTO counter (id,created_at) " +
                                            "VALUES(:id,:timestamp)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("id", ID));
                cmd.Parameters.Add(new NpgsqlParameter("timestamp", UploadMoment));
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
            Resultlbl.Text = "HIBA!";
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllFieldsValidated)
            {
                SerialNumber = GetNextNumber();
                PrintDMC(SerialNumber);
                DbInsert("housing_leak_test_one");

                if (AutoID)
                    CheckStartingIdValidity(sender, e);
                else
                {
                    StartingIdTxbx.Text = "";
                    StartingIdTxbx.Focus();
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PrintDMC(SerialNumber);
        }

        private void RefreshStartingID(object sender, RoutedEventArgs e)
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT id FROM counter where id < 10000 order by id desc limit 1", conn);
                StartingIdTxbx.Text = (int.Parse(cmd.ExecuteScalar().ToString())+1).ToString();
                conn.Close();
            }catch(Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void CheckStartingIdValidity(object sender, RoutedEventArgs e)
        {
            if (StartingIdTxbx.Text.Length > 0)
            {
                if (Regex.IsMatch(StartingIdTxbx.Text, "^[0-9]+$"))
                {
                    try
                    {
                        string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                        var conn = new NpgsqlConnection(connstring);
                        conn.Open();

                        var countID = new NpgsqlCommand("SELECT count(id) FROM counter where id ='" + StartingIdTxbx.Text + "'", conn);
                        if (Int32.Parse(countID.ExecuteScalar().ToString()) > 0)
                        {
                            CallMessageForm("A "+StartingIdTxbx.Text+" kezdő sorszám már létezik az adatbázisban!");
                            SetStartingIdTextOnError(sender, e);
                        }                            

                        conn.Close();
                    }
                    catch (Exception msg)
                    {
                        MessageBox.Show(msg.ToString());
                    }
                }
                else
                {
                    CallMessageForm("Nem megfelelő a kezdő sorszám formátuma!");
                    SetStartingIdTextOnError(sender, e);
                }
            }
            else
            {
                //simple error handling because of focus jumps
                RefreshStartingID(sender, e);
            }
        }

        private void SetStartingIdTextOnError(object sender, RoutedEventArgs e)
        {
            if (AutoID)
                RefreshStartingID(sender, e);
            else
            {
                StartingIdTxbx.Text = "";
            }
        }

        private void MoveFocus(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && StartingIdTxbx.Text != "")
                leakTestTxbx.Focus();
        }

        private void autoIdBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AutoID)
            {
                AutoID = false;
                RefreshAutoIdSetting();
            }
            else
            {
                AutoID = true;
                RefreshAutoIdSetting();
            }
        }

        private void SaveAppConfig(object sender, EventArgs e)
        {
            ////not working because of the permission on the PC
            //if (AutoID)
            //{
            //    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //    config.AppSettings.Settings["AutoGenerateId"].Value = "true";
            //    config.Save(ConfigurationSaveMode.Modified);
            //}
            //else
            //{
            //    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            //    config.AppSettings.Settings["AutoGenerateId"].Value = "false";
            //    config.Save(ConfigurationSaveMode.Modified);
            //}
        }
    }
}
