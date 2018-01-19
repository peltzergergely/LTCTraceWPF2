using Npgsql;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for Hipot1Window.xaml
    /// </summary>
    public partial class Hipot1Window : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool IsPreChkPassed { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public Hipot1Window()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        //Handles focus movement and checks validator
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
                PreChk("housing_leak_test_one");
                FormValidator();
                SaveBtn_Click(sender, e);
            }

            DmValidator();
        }

        private void FormValidator()
        {
            if (IsDmValidated == true && IsPreChkPassed == true)
            {
                AllFieldsValidated = true;
            }
            else if (IsDmValidated == false)
            {
                CallMessageForm("DataMátrix nem megfelelő!");
            }
            else if (IsPreChkPassed == false)
            {
                CallMessageForm("Előző munkafolyamatot nem találom!");
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void DmValidator()
        {
            if (RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx"))
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
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

        //precheck if previous table have current datamatrix
        //housing_fb_assy housing_dm fb_dm
        private void PreChk(string previousTable)
        {
            string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            // Making connection
            var conn = new NpgsqlConnection(connstring);
            conn.Open();
            // building query
            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM " + previousTable + " WHERE housing_dm = :housing_dm", conn);
            cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
            Int32 countProd = Convert.ToInt32(cmd.ExecuteScalar());
            conn.Close();
            if (countProd == 1)
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
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (housing_dm, test_result, pc_name, started_on, saved_on) " +
                    "VALUES(:housing_dm, :test_result, :pc_name, :started_on, :saved_on)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("test_result", PFGenChkbx.IsChecked));
                cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

            if (AllFieldsValidated)
            {
                DbInsert("hipot_test_one");
            }
        }

        private void FbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            StartedOn = DateTime.Now;
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
