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

        public bool IsPreChkPassed { get; set; } = false;

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
            if (IsDmValidated == true && float.Parse(leakTestTxbx.Text) < 5 && float.Parse(leakTestTxbx.Text) > 0)
            {
                PreChk("final_assy_two", "housing_dm", housingDmTxbx.Text);
                if (IsPreChkPassed)
                {
                    AllFieldsValidated = true;
                }
            }
            else
            {
                CallMessageForm("Hibás kitöltés");
            }
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
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", housingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("leak_test_result", float.Parse(leakTestTxbx.Text)));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
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
    }
}

