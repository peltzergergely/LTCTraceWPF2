using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {
        DispatcherTimer DigitClockTimer = new DispatcherTimer();

        public LoginPage()
        {
            InitializeComponent();

            //Digit clock timer
            DigitClockTimer.Tick += new EventHandler(Timer_Click);
            DigitClockTimer.Interval = new TimeSpan(0, 0, 1);
            DigitClockTimer.Start();

            username.Focus();

            outputLbl.Content = "";
        }

        //Digit clock ticking
        private void Timer_Click(object sender, EventArgs e)
        {
            DateTime d;
            d = DateTime.Now;
            string h = "", m = "", s = "";

            if (d.Hour < 10)
                h = "0" + d.Hour.ToString();
            else
                h = d.Hour.ToString();

            if (d.Minute < 10)
                m = "0" + d.Minute.ToString();
            else
                m = d.Minute.ToString();

            if (d.Second < 10)
                s = "0" + d.Second.ToString();
            else
                s = d.Second.ToString();

            clockLbl.Content = h + ":" + m + ":" + s;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            var DbWindow = new DbWindow();
            DbWindow.Show();
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            string usr = username.Text;
            string pw = password.Password;

            using (new WaitCursor())
            {
                try
                {
                    var connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                    var conn = new NpgsqlConnection(connstring);
                    conn.Open();
                    var query = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = '" + usr + "' AND password = crypt('" + pw + "', password);", conn);

                    if (Convert.ToInt32(query.ExecuteScalar()) == 1)
                    {
                        // sikeres belépés
                        query = new NpgsqlCommand("SELECT admin FROM users WHERE username = '" + usr + "'", conn);
                        bool adminuser = Convert.ToBoolean(query.ExecuteScalar());

                        query = new NpgsqlCommand("SELECT trained FROM users WHERE username = '" + usr + "'", conn);
                        string trained = query.ExecuteScalar().ToString();


                        MainWindow mw = new MainWindow(adminuser, trained);
                        mw.Owner = this;
                        mw.Show();
                        this.ResetForm();
                        this.Hide();
                    }
                    else
                    {
                        outputLbl.Content = "A felhasználó vagy jelszó nem megfelelő!";
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void ResetForm()
        {
            username.Focus();
            username.Text = "";
            password.Clear();
        }

        private void reg_Click(object sender, RoutedEventArgs e)
        {
            RegistrationPage regpage = new RegistrationPage();
            regpage.Owner = this;
            regpage.Show();
        }

        private void JumpFocus(object sender, KeyEventArgs e)
        {
            outputLbl.Content = "";

            if (username.IsFocused && username.Text.Length > 0 && e.Key == Key.Enter)
            {
                password.Focus();
                return;
            }

            if (password.IsFocused && password.Password.Length > 0 && username.Text.Length > 0 && e.Key == Key.Enter)
            {
                login_Click(sender, e);
                return;
            }

            if (login.IsFocused && e.Key == Key.Enter)
            {
                login_Click(sender, e);
                return;
            }
        }
    }
}
