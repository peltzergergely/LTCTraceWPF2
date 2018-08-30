using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for EditHousingNumberWindow.xaml
    /// </summary>
    public partial class EditHousingNumberWindow : Window
    {
        public EditHousingNumberWindow()
        {
            InitializeComponent();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            //Do nothing atm
        }

        private void Inputs_LostFocus(object sender, RoutedEventArgs e)
        {
            WarningPanel.Height = 0;
            NochangePanel.Height = 0;
            HousingInDbPanel.Height = 0;
            ResultPrintDMCPanel.Height = 0;
            PrintDMCPanel.Height = 0;

            string checkFor = String.Empty;

            if (sender.GetType().ToString().Contains("TextBox"))
                checkFor = (sender as TextBox).Name.ToString();
            else
                checkFor = (sender as PasswordBox).Name.ToString();

            switch (checkFor)
            {
                case "OldHousingDm":

                    if (Regex.IsMatch(OldHousingDm.Text, ConfigurationManager.AppSettings["HousingDmRegEx"]))
                        ChangeToValid(Validator_oldHousingDM);
                    else
                        ChaneToInvalid(Validator_oldHousingDM);
                    break;

                case "NewHousingDm1":

                    if (Regex.IsMatch(NewHousingDm1.Text, ConfigurationManager.AppSettings["HousingDmRegEx"]))
                        ChangeToValid(Validator_newHousingDM1);
                    else
                        ChaneToInvalid(Validator_newHousingDM1);

                    if (NewHousingDm2.Text.Length > 0)
                        goto case "NewHousingDm2";
                    else
                        break;

                case "NewHousingDm2":
                    if (Regex.IsMatch(NewHousingDm2.Text, ConfigurationManager.AppSettings["HousingDmRegEx"]) && NewHousingDm2.Text == NewHousingDm1.Text)
                        ChangeToValid(Validator_newHousingDM2);
                    else
                        ChaneToInvalid(Validator_newHousingDM2);

                    break;
                case "Modifier":

                    if (Modifier.Text.Length > 0)
                        ChangeToValid(Validator_modifier);
                    else
                        ChaneToInvalid(Validator_modifier);

                    break;
                case "pw1":

                    if (pw1.Password == "LTCadmin")
                        ChangeToValid(Validator_pw1);
                    else
                        ChaneToInvalid(Validator_pw1);

                    if (pw2.Password.Length > 0)
                        goto case "pw2";
                    else
                        break;

                case "pw2":

                    if (pw2.Password == "LTCadmin" && pw1.Password == pw2.Password)
                        ChangeToValid(Validator_pw2);
                    else
                        ChaneToInvalid(Validator_pw2);

                    break;
                default:
                    break;
            }
        }

        private void ChaneToInvalid(Label validator)
        {
            validator.Content = "\xE711";
            validator.Foreground = Brushes.Red;
        }

        private void ChangeToValid(Label validator)
        {
            validator.Content = "\xE73E";
            validator.Foreground = Brushes.Green;
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            string newNumber = NewHousingDm1.Text;
            newNumber = newNumber.Substring(newNumber.IndexOf("LTC") + 3, newNumber.Length - (newNumber.IndexOf("LTC") + 3));
            string oldNumber = OldHousingDm.Text;
            oldNumber = oldNumber.Substring(oldNumber.IndexOf("LTC") + 3, oldNumber.Length - (oldNumber.IndexOf("LTC") + 3));
            DatabaseHelper d = new DatabaseHelper();

            if (Validator_oldHousingDM.Content == null ||
                Validator_newHousingDM1.Content == null ||
                Validator_newHousingDM2.Content == null ||
                Validator_modifier.Content == null ||
                Validator_pw1.Content == null ||
                Validator_pw2.Content == null)
                return;

            if (Validator_oldHousingDM.Content.ToString() != "\xE73E" ||
                Validator_newHousingDM1.Content.ToString() != "\xE73E" ||
                Validator_newHousingDM2.Content.ToString() != "\xE73E" ||
                Validator_modifier.Content.ToString() != "\xE73E" ||
                Validator_pw1.Content.ToString() != "\xE73E" ||
                Validator_pw2.Content.ToString() != "\xE73E")
                return;
            
            if (d.CountRowInDB("housing_leak_test_one","housing_dm", NewHousingDm1.Text) > 0)
            {
                HousingInDbPanel.Height = 720;
                return;
            }

            int num = 0;
            try
            {
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString))
                {
                    conn.Open();

                   num = int.Parse(new NpgsqlCommand("SELECT COUNT(*) FROM counter where ID = "+newNumber+"", conn).ExecuteScalar().ToString());
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            if (num > 0 && (newNumber != oldNumber))
            {
                HousingInDbPanel.Height = 720;
                return;
            }

            int sum = 0;
            sum += d.CountRowInDB("calibration", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("cooling_leak_test", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("eol", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("final_assy_one", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("final_assy_two", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("firewall", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("hipot_test_one", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("hipot_test_two", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("housing_connector_assy", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("housing_fb_assy", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("housing_leak_test_one", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("housing_leak_test_two", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("potting", "housing_dm", OldHousingDm.Text);
            sum += d.CountRowInDB("reworked_products", "housing_dm", OldHousingDm.Text);

            if (sum == 0)
            {
                NochangePanel.Height = 720;
                return;
            }

            QuantityText.Text = sum.ToString();
            WarningPanel.Height = 720;
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateTables();
            WarningPanel.Height = 0;
            PrintDMCPanel.Height = 720;
        }

        private void UpdateTables()
        {
            UpdateTable("calibration");
            UpdateTable("cooling_leak_test");
            UpdateTable("eol");
            UpdateTable("final_assy_one");
            UpdateTable("final_assy_two");
            UpdateTable("firewall");
            UpdateTable("hipot_test_one");
            UpdateTable("hipot_test_two");
            UpdateTable("housing_connector_assy");
            UpdateTable("housing_fb_assy");
            UpdateTable("housing_leak_test_one");
            UpdateTable("housing_leak_test_two");
            UpdateTable("potting");
            UpdateTable("reworked_products");

            //Update counter table
            UpdateCounterTable();
        }

        private void UpdateCounterTable()
        {
            string newNumber = NewHousingDm1.Text;
            newNumber = newNumber.Substring(newNumber.IndexOf("LTC")+3,newNumber.Length- (newNumber.IndexOf("LTC") + 3));

            string oldNumber = OldHousingDm.Text;
            oldNumber = oldNumber.Substring(oldNumber.IndexOf("LTC") + 3, oldNumber.Length - (oldNumber.IndexOf("LTC") + 3));

            //MessageBox.Show(newNumber+" "+oldNumber);
            try
            {
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString))
                {
                    conn.Open();

                    new NpgsqlCommand("UPDATE counter SET id = "+newNumber+" WHERE id = "+oldNumber+"", conn).ExecuteNonQuery();
                    //MessageBox.Show("UPDATE counter SET id = " + newNumber + " WHERE id = " + oldNumber + "");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void UpdateTable(string t)
        {
            try
            {
                using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString))
                {
                    int steps = new DatabaseHelper().CountRowInDB(t, "housing_dm", OldHousingDm.Text);
                    conn.Open();

                    for (int i = 0; i < steps; i++)
                    {
                        new NpgsqlCommand("INSERT INTO updated_housing_dms (before_id,before_table,before_dm,after_dm,from_who)VALUES(" +
                            "(SELECT id from " + t + " WHERE housing_dm = '" + OldHousingDm.Text + "' offset "+i+" limit 1)," +
                            "'"+t+"'," +
                            "'" + OldHousingDm.Text + "'," +
                            "'"+NewHousingDm1.Text+"'," +
                            "'"+ Modifier.Text+ "')", conn).ExecuteNonQuery();
                    }
                    new NpgsqlCommand("UPDATE " + t + " SET housing_dm = '" + NewHousingDm1.Text + "' WHERE housing_dm = '" + OldHousingDm.Text + "'", conn).ExecuteNonQuery();
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void nokBtn_Click(object sender, RoutedEventArgs e)
        {
            //Reset form

            //hide panels
            WarningPanel.Height = 0;
            NochangePanel.Height = 0;
            HousingInDbPanel.Height = 0;
            ResultPrintDMCPanel.Height = 0;
            PrintDMCPanel.Height = 0;
            //inputs
            WarningPanel.Height = 0;
            OldHousingDm.Text = String.Empty;
            NewHousingDm1.Text = String.Empty;
            NewHousingDm2.Text = String.Empty;
            Modifier.Text = String.Empty;
            pw1.Password = String.Empty;
            pw2.Password = String.Empty;

            //validators
            Validator_modifier.Content = String.Empty;
            Validator_newHousingDM1.Content = String.Empty;
            Validator_newHousingDM2.Content = String.Empty;
            Validator_oldHousingDM.Content = String.Empty;
            Validator_pw1.Content = String.Empty;
            Validator_pw2.Content = String.Empty;
        }

        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            string barcodeText = String.Empty;

            if (NewHousingDm1.Text.Contains("35LTC"))
            {
                barcodeText = "LTC B3.5 LEVC PN";
            }
            else if (NewHousingDm1.Text.Contains("P514LTC"))
            {
                barcodeText = "LTC P514";
            }
            else
            {
                barcodeText = "LTC 1E0002187AD B2.5";
            }

            string s = @"^XA^MMT^PW406^LL0280^LS0^BY252,252^FT16,266^BXN,18,200,0,0,1,~^FH\^FD" + NewHousingDm1.Text + @"^FS^FT345,274^A0B,25,26^FH\^FD" + barcodeText + @"^FS^FT376,274^A0B,25,26^FH\^FD" + NewHousingDm1.Text + @"^FS^PQ1,0,1,Y^XZ";

            //Set printername
            string printerName = String.Empty;

            if (Environment.MachineName != "STATION-8-TRACE")
            
                printerName = @"\\STATION-8-TRACE\ZDesigner ZT420-203dpi ZPL";
            else
                printerName = "ZDesigner ZT420-203dpi ZPL";

            RawPrinterHelper.SendStringToPrinter(printerName, s);
            PrintDMCPanel.Height = 0;
            ResultPrintDMCPanel.Height = 720;
        }
    }
}
