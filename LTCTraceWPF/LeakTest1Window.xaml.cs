using Npgsql;
using System;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


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


        public LeakTest1Win()
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
            leakTestTxbx.Focus();
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
            int counter;
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT id FROM counter order by id desc limit 1", conn);
                counter = Convert.ToInt16(cmd.ExecuteScalar());
                conn.Close();
                return counter;
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                return counter = 0;
            }
        }

        private void IncreaseCounterDB()
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                var UploadMoment = DateTime.Now;
                conn.Open();
                var cmd = new NpgsqlCommand("INSERT INTO counter (created_at) " +
                                            "VALUES(:timestamp)", conn);
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
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PrintDMC(SerialNumber);
        }
    }
}
