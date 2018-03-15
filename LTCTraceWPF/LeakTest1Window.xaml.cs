using Npgsql;
using System;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;
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

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void ResetForm()
        {
            AllFieldsValidated = false;
            leakTestTxbx.Text = "";
            leakTestTxbx.Focus();
        }

        private void FormValidator()
        {
            if (IsDmValidated == true && float.Parse(leakTestTxbx.Text, CultureInfo.InvariantCulture.NumberFormat) < 5 && float.Parse(leakTestTxbx.Text, CultureInfo.InvariantCulture.NumberFormat) > 0)
            {
                AllFieldsValidated = true;
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
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, leak_test_result, pc_name, created_on) " +
                    "VALUES(:housing_dm, :leak_test_result, :pc_name, :timestamp)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDM));
                cmd.Parameters.Add(new NpgsqlParameter("leak_test_result", float.Parse(leakTestTxbx.Text, CultureInfo.InvariantCulture.NumberFormat)));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("timestamp", UploadMoment));
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

        private void PrintDMC(int number)
        {
            string runningtext = "LTC";
            if (number > 0)
            {
                HousingDM = runningtext + number.ToString();
                CounterTxbx.Text = HousingDM;
                //string s = @"^XA^MMT^PW406^LL0280^LS0^FT67,240^A0N,28,28^FH\^" + HousingDM + @"^FS^BY154,154^FT123,209^BXN,7,200,0,0,1,~^FH\^FDLTC1E0002187ADB2.5\0D\0A" + HousingDM + "^FS^PQ1,0,1,Y^XZ";
                string s = @"^XA^MMT^PW406^LL0280^LS0^BY192,192^FT107,205^BXN,16,200,0,0,1,~^FH\^FD" + HousingDM + @"^FS^FT69,239^A0N,28,28^FH\^FDLTC 1E0002187 AD B2.5^FS^FT69,273^A0N,28,28^FH\^FD" + HousingDM + "^FS^PQ1,0,1,Y^XZ";
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    RawPrinterHelper.SendStringToPrinter(pd.PrintQueue.FullName, s);
                }
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
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                var UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO counter (created_at) " +
                                            "VALUES(:timestamp)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("timestamp", UploadMoment));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
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
