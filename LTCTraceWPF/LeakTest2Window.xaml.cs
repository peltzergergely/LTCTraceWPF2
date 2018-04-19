using Npgsql;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for LeakTest2Window.xaml
    /// </summary>
    public partial class LeakTest2Window : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public double Number { get; set; } = 0;

        public LeakTest2Window()
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

            if (e.Key == Key.Enter && housingDmTxbx.Text.Length > 0)
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

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void DmValidator()
        {
            if (RegexValidation(housingDmTxbx.Text, "HousingDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            housingDmTxbx.Text = "";
            leakTestTxbx.Text = "";
            housingDmTxbx.Focus();
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
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", housingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("leak_test_result", Number));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("timestamp", UploadMoment));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                Resultlbl.Text = "Adatok elmentve! " + DateTime.Now;
                ResetForm();
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
                DbInsert("housing_leak_test_two");
            }
        }

        private void housingDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (housingDmTxbx.Text.Length > 0)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("final_assy_two", "housing_dm", housingDmTxbx.Text) == 0)
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
                StartedOn = DateTime.Now;
            }
        }
    }
}

