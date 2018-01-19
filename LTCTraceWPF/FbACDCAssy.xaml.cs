using Npgsql;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for FbACDCAssy.xaml
    /// </summary>
    public partial class FbACDCAssy : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;
        
        public FbACDCAssy()
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

            if (Keyboard.FocusedElement == SaveBtn)
            {
                FormValidator();
                SaveBtn_Click(sender, e);
            }

            DmValidator();
        }

        private void FormValidator()
        {
            if (IsDmValidated == true && screwChkbx.IsChecked == true)
            {
                AllFieldsValidated = true;
            }
            else
            {
                CallMessageForm("Hibás kitöltés");
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
            FbDmTxbx.Text = "";
            screwChkbx.IsChecked = false;
            FbDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
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
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (fb_dm, pc_name, started_on, saved_on) " +
                    "VALUES(:fb_dm, :pc_name, :started_on, :saved_on)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("fb_dm", FbDmTxbx.Text));
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
                DbInsert("fb_acdc_assy");
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
