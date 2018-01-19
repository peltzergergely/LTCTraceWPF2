using Npgsql;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for HsPreAssyWindowxaml.xaml
    /// Todo:
    ///     validation logic checks
    ///         all fields are filled
    ///         all dms are different
    ///     if valid then connect to database and upload the data
    ///     identify pc by name
    ///     dummy user name
    ///     timestamp
    ///     dummy screwdriver data
    /// </summary>
    public partial class HsPreAssyWindowxaml : Window
    {
        public HsPreAssyWindowxaml()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FocusNext(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);               
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }           
        }

        //check if field has value and is different
        private bool DmValidation()
        {
            if (string.IsNullOrWhiteSpace(HsDm0.Text)) 
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm1.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm2.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(HsDm3.Text))
                return false;
            else if (HsDm0.Text == HsDm1.Text || 
                HsDm0.Text == HsDm2.Text || 
                HsDm0.Text == HsDm3.Text ||
                HsDm1.Text == HsDm2.Text ||
                HsDm1.Text == HsDm3.Text ||
                HsDm2.Text == HsDm3.Text)
                return false;
            else
                return true;
        }

        private void ValidationMsg(bool isValid)
        {
            if (!isValid)
            {
                MessageBox.Show("HIBÁS KITÖLTÉS!");
            }
        }

        //adatbázis kapcsolat és adatok feltöltése az adatábisba
        private void DbInsert(string dbTableName) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.CCDBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + dbTableName + 
                    " (hs_dm_0, hs_dm_1, hs_dm_2, hs_dm_3, created_on, username, station)" +
                    " VALUES(:hs_dm_0, :hs_dm_1, :hs_dm_2, :hs_dm_3, :created_on, :username, :station)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_0", HsDm0.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_1", HsDm1.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_2", HsDm2.Text));
                cmd.Parameters.Add(new NpgsqlParameter("hs_dm_3", HsDm3.Text));
                cmd.Parameters.Add(new NpgsqlParameter("created_on", DateTime.Now));
                cmd.Parameters.Add(new NpgsqlParameter("username", "PG"));
                cmd.Parameters.Add(new NpgsqlParameter("station", Environment.MachineName));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                MessageBox.Show("Adatok feltöltve!");
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void ResetForm()
        {
            HsDm0.Text = "";
            HsDm1.Text = "";
            HsDm2.Text = "";
            HsDm3.Text = "";
            izo1Chkbx.IsChecked = false;
            izo2Chkbx.IsChecked = false;
            pastaChkbx.IsChecked = false;
            Keyboard.Focus(HsDm0);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DmValidation())
            {
                DbInsert("hspreassy");
                ResetForm();
            }
            else
                ValidationMsg(DmValidation());
        }
    }
}
