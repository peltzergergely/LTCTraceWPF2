using Npgsql;
using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ReworkWindow.xaml
    /// </summary>
    public partial class ReworkWindow : Window
    {

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsDmValidated { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        public bool IsPreChkPassed { get; set; } = false;

        public ReworkWindow()
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
                FormValidator();
                SaveBtn_Click(sender, e);
            }
            DmValidator();
        }

        private void FormValidator()
        {
            if (IsDmValidated == true)
            {
                PreChk("mb_hs_assy", "mb_dm", MbDmTxbx.Text);
                if (IsPreChkPassed)
                {
                        AllFieldsValidated = true;
                }
                else
                    CallMessageForm("Mainboard Heatsink Szerelés folyamaton nem szerepelt a Mainboard!");
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
            if (
                //RegexValidation(HousingDmTxbx.Text, "MbDmRegEx") && 
                (HousingDmTxbx.Text.Length>0) &&
                (MbDmTxbx.Text.Length > 0) && 
                (DSP11DmTxbx.Text.Length > 0) &&
                (DSP12DmTxbx.Text.Length > 0) &&
                (DSP13DmTxbx.Text.Length > 0) &&
                (DSP21DmTxbx.Text.Length > 0) &&
                (DSP22DmTxbx.Text.Length > 0) &&
                (DSP23DmTxbx.Text.Length > 0)

                )
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            HousingDmTxbx.Text = "";
            MbDmTxbx.Text = "";
            DSP11DmTxbx.Text = "";
            DSP12DmTxbx.Text = "";
            DSP13DmTxbx.Text = "";
            DSP21DmTxbx.Text = "";
            DSP22DmTxbx.Text = "";
            DSP23DmTxbx.Text = "";
            IsPreChkPassed = false;
            HousingDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
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

        //UPDATE final_assy_one SET mb_dm = :mb_dm WHERE housing_dm = :housing_dm
        private void DbUpdateFinalAssy(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("UPDATE " + table + " SET mb_dm = :mb_dm WHERE housing_dm = :housing_dm", conn);
                cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                ResetForm();
            }
        }

        //UPDATE mb_dsp_assy SET dsp11 = :dsp11, dsp12 = :dsp12, dsp13= :dsp13, dsp21 = :dsp21, dsp22 = :dsp22, dsp23 = :dsp23 WHERE mb_dm = :mb_dm 
        private void DbUpdateMbDspAssy(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                DateTime UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("UPDATE " + table + " dsp11 = :dsp11, dsp12 = :dsp12, dsp13= :dsp13, dsp21 = :dsp21, dsp22 = :dsp22, dsp23 = :dsp23 WHERE mb_dm = :mb_dm", conn);
                cmd.Parameters.Add(new NpgsqlParameter("dsp11", DSP11DmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("dsp12", DSP12DmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("dsp13", DSP13DmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("dsp21", DSP21DmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("dsp22", DSP22DmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("dsp23", DSP23DmTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDmTxbx.Text));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                ResetForm();
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AllFieldsValidated)
            {
                DbUpdateFinalAssy("final_assy_one");
                DbUpdateMbDspAssy("mb_dsp_assy");
            }
        }
    }
}
