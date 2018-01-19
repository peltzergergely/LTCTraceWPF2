using Npgsql;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for MbThtWindow.xaml
    /// </summary>
    public partial class MbThtWindow : Window
    {
        public MbThtWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            InitializeComponent();
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

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool DmValidation()
        {
            if (string.IsNullOrWhiteSpace(MbDm.Text))
                return false;
            else if (string.IsNullOrWhiteSpace(ReworkTbx.Text))
                return false;
            else
                return true;
        }

        private void ValidationMsg(bool isValid)
        {
            if (!isValid)
            {
                MessageBox.Show("HIÁNYOS KITÖLTÉS!");
            }
        }

        private bool InterlockCheck(string table)
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.CCDBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring); // Making connection
                conn.Open();
                var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM " + table + " WHERE mb_dm = :mb_dm", conn);
                cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDm.Text));
                Int32 countProd = Convert.ToInt32(cmd.ExecuteScalar());
                conn.Close();
                if (countProd == 1)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
                return false;
            }
        }

        private void InterlockMsg(bool interlockResult)
        {
            if (!interlockResult)
                MessageBox.Show("Interlock! A termék nem szerepelt a korábbi munkaállomáson!");
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
                    " (mb_dm, created_on, username, station)" +
                    " VALUES(:mb_dm, :created_on, :username, :station)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDm.Text));
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

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DmValidation())
            {
                if (InterlockCheck("mbhsassy"))
                {
                    DbInsert("mbtht");
                }else
                    InterlockMsg(InterlockCheck("mbhsassy"));
            }else
                ValidationMsg(DmValidation());
        }
    }
}
