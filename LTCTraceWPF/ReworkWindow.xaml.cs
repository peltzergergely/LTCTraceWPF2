using ErrorLogging;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
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
        public bool DoMbDspRework { get; set; } = false;
        public bool DoHousingMbRework { get; set; } = false;
        public bool DoHousingGwRework { get; set; } = false;



        public ReworkWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();

            //reset output text
            result.Content = String.Empty;
        }

        private void OnKeyUpEvent(object sender, KeyEventArgs e)
        {
            result.Content = "";

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
                // Mainboard and DSP
                if (MbDmTxbx.Text.Length > 0 && (DSP11DmTxbx.Text.Length > 0 || DSP12DmTxbx.Text.Length > 0 || DSP13DmTxbx.Text.Length > 0 || DSP21DmTxbx.Text.Length > 0 || DSP22DmTxbx.Text.Length > 0 || DSP23DmTxbx.Text.Length > 0))
                {
                    DoMbDspRework = true;
                }
                else DoMbDspRework = false;
                // House and Gateway
                if (HousingDmTxbx.Text.Length > 0 && GwDmTxbx.Text.Length > 0)
                {
                    bool isHouseValid = check("potting", "housing_dm", HousingDmTxbx.Text);

                    if (isHouseValid)
                    {
                        DoHousingGwRework = true;
                    }else
                    {
                        if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                            CallMessageForm("Potting után kaptonozás folyamaton nem szerepelt a Ház!");
                        else
                            ErrorLog.Create("potting", "housing_dm", HousingDmTxbx.Text, MethodBase.GetCurrentMethod().Name.ToString(), "Potting után kaptonozás folyamaton nem szerepelt a Ház!", this.GetType().Name.ToString());

                        DoHousingGwRework = false;
                    }
                }

                // House and Mainboard
                if (HousingDmTxbx.Text.Length > 0 && MbDmTxbx.Text.Length > 0)
                {
                    bool isHouseValid = check("potting", "housing_dm", HousingDmTxbx.Text);
                    bool isMbValid = check("mb_dsp_assy", "mb_dm", MbDmTxbx.Text);

                    if (isHouseValid && isMbValid)
                    {
                        DoHousingMbRework = true;
                    }
                    else
                    {
                        if (!isHouseValid)
                        {
                            if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                                CallMessageForm("Potting után kaptonozás folyamaton nem szerepelt a Ház!");
                            else
                                ErrorLog.Create("potting", "housing_dm", HousingDmTxbx.Text, MethodBase.GetCurrentMethod().Name.ToString(), "Potting után kaptonozás folyamaton nem szerepelt a Ház!", this.GetType().Name.ToString());
                        }

                        if (!isMbValid)
                        {

                            if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                                CallMessageForm("Mainboard DSP szer. folyamaton nem szerepelt a Mainboard!");
                            else
                                ErrorLog.Create("mb_dsp_assy", "mb_dm", MbDmTxbx.Text, MethodBase.GetCurrentMethod().Name.ToString(), "Mainboard DSP szer. folyamaton nem szerepelt a Mainboard!!", this.GetType().Name.ToString());

                        }

                        DoHousingMbRework = false;
                    }
                }

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
                (HousingDmTxbx.Text.Length == 0 || RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx"))  &&
                (MbDmTxbx.Text.Length == 0 || RegexValidation(MbDmTxbx.Text, "MbDmRegEx"))  &&
                (GwDmTxbx.Text.Length == 0 || RegexValidation(GwDmTxbx.Text, "GwDmRegEx")) &&
                (DSP11DmTxbx.Text.Length == 0 || RegexValidation(DSP11DmTxbx.Text, "DspDmRegEx")) &&
                (DSP12DmTxbx.Text.Length == 0 || RegexValidation(DSP12DmTxbx.Text, "DspDmRegEx")) &&
                (DSP13DmTxbx.Text.Length == 0 || RegexValidation(DSP13DmTxbx.Text, "DspDmRegEx")) &&
                (DSP21DmTxbx.Text.Length == 0 || RegexValidation(DSP21DmTxbx.Text, "DspDmRegEx")) &&
                (DSP22DmTxbx.Text.Length == 0 || RegexValidation(DSP22DmTxbx.Text, "DspDmRegEx")) &&
                (DSP23DmTxbx.Text.Length == 0 || RegexValidation(DSP23DmTxbx.Text, "DspDmRegEx"))

                )
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            DoMbDspRework = false;
            DoHousingMbRework = false;
            DoHousingGwRework = false;
            GwDmTxbx.Text = String.Empty;
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
            try
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
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private bool check(string previousTable, string columnToSearch, string dataToFind)
        {
            try
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
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
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
            DmValidator();
            FormValidator();
            if (DoMbDspRework)
            {
                ExecuteMbDspRework();
            }

            if (DoHousingMbRework)
            {
                ExecuteHousingMbRework();
            }

            if (DoHousingGwRework)
            {
                try
                {
                    using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString))
                    {
                        conn.Open();

                        string q1 = "INSERT INTO reworked_products(housing_dm,gw_dm,saved_on)(select housing_dm,gw_dm,current_timestamp from final_assy_two where housing_dm = '" + HousingDmTxbx.Text + "' order by id desc limit 1)";
                        string q2 = "UPDATE final_assy_two SET gw_dm = '" + GwDmTxbx.Text + "' WHERE housing_dm = '" + HousingDmTxbx.Text + "'";

                        new NpgsqlCommand(q1, conn).ExecuteNonQuery();
                        new NpgsqlCommand(q2, conn).ExecuteNonQuery();
                        ResultOutput();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        private void ExecuteHousingMbRework()
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString))
                {
                    conn.Open();

                    new NpgsqlCommand("INSERT INTO reworked_products(housing_dm,mb_dm,saved_on)(select housing_dm,mb_dm,current_timestamp from final_assy_one where housing_dm = '" + HousingDmTxbx.Text + "' order by id limit 1)", conn).ExecuteNonQuery();
                    new NpgsqlCommand("UPDATE final_assy_one SET mb_dm = '" + MbDmTxbx.Text + "' WHERE housing_dm = '" + HousingDmTxbx.Text + "'", conn).ExecuteNonQuery();
                    ResultOutput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ExecuteMbDspRework()
        {
            Dictionary<string, string> dsp = new Dictionary<string, string>();
            string query = "UPDATE mb_dsp_assy SET ";

            if (DSP11DmTxbx.Text.Length > 0)
            {
                dsp.Add("dsp_one_one", DSP11DmTxbx.Text);
                query += "dsp_one_one = '" + DSP11DmTxbx.Text + "',";
            }
            if (DSP12DmTxbx.Text.Length > 0)
            {
                dsp.Add("dsp_one_two", DSP12DmTxbx.Text);
                query += "dsp_one_two = '" + DSP12DmTxbx.Text + "',";
            }
            if (DSP13DmTxbx.Text.Length > 0)
            {
                dsp.Add("dsp_one_three", DSP13DmTxbx.Text);
                query += "dsp_one_three = '" + DSP13DmTxbx.Text + "',";
            }
            if (DSP21DmTxbx.Text.Length > 0)
            {
                dsp.Add("dsp_two_one", DSP21DmTxbx.Text);
                query += "dsp_two_one = '" + DSP21DmTxbx.Text + "',";
            }
            if (DSP22DmTxbx.Text.Length > 0)
            {
                dsp.Add("dsp_two_two", DSP22DmTxbx.Text);
                query += "dsp_two_two = '" + DSP22DmTxbx.Text + "',";
            }
            if (DSP23DmTxbx.Text.Length > 0)
            {
                dsp.Add("dsp_two_three", DSP23DmTxbx.Text);
                query += "dsp_two_three = '" + DSP23DmTxbx.Text + "',";
            }
            query = query.Remove(query.Length - 1);
            query += "WHERE mb_dm = '" + MbDmTxbx.Text + "'";

            //Build up backup query
            string backupQuery = "INSERT INTO reworked_products(mb_dm,";
            foreach (var item in dsp)
            {
                backupQuery += item.Key + ",";
            }
            backupQuery += "saved_on)(select mb_dm,";
            foreach (var item in dsp)
            {
                backupQuery += item.Key + ",";
            }
            backupQuery += "current_timestamp from mb_dsp_assy where mb_dm = '" + MbDmTxbx.Text + "' order by id limit 1)";

            //MessageBox.Show(backupQuery);
            //MessageBox.Show(query);

            string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();

                    new NpgsqlCommand(backupQuery, conn).ExecuteNonQuery();
                    new NpgsqlCommand(query, conn).ExecuteNonQuery();
                    ResultOutput();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void ResultOutput()
        {
            ResetForm();
            result.Content = "Adatok elmentve! " + DateTime.Now;
        }
    }
}
