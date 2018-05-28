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
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    public partial class RegistrationPage : Window
    {
        bool isNameOk = false;
        bool isUsernameOk = false;
        bool isPw1Ok = false;
        bool isPw2Ok = false;

        public RegistrationPage()
        {
            InitializeComponent();

            nameChkOutput.Content = "";
            usernameChkOutput.Content = "";
            pw1ChkOutput.Content = "";
            pw2ChkOutput.Content = "";
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Error(Label output, string errormessage, ref bool flag)
        {
            flag = false;
            output.FontFamily = new FontFamily("Verdana");
            output.Content = errormessage;
            output.Foreground = Brushes.Red;
            output.FontSize = 20;
        }

        private void Valid(Label output, ref bool flag)
        {
            flag = true;
            output.FontFamily = new FontFamily("Segoe MDL2 Assets");
            output.Content = "\xE8FB";
            output.Foreground = Brushes.Green;
            output.FontSize = 30;
        }

        private void CheckValidate(object sender, RoutedEventArgs e)
        {
            if (nameTxb.IsFocused == true)
            {
                if (nameTxb.Text.Length > 2)
                    Valid(nameChkOutput,ref isNameOk);
                else
                    Error(nameChkOutput, "Túl Rövid!", ref isNameOk);
            }

            if (usernameTxb.IsFocused == true)
            {
                if (usernameTxb.Text.Length > 2)
                {   
                    if (Regex.IsMatch(usernameTxb.Text, "^[a-zA-Z0-9]*$"))
                    {
                        try
                        {
                            string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                            var conn = new NpgsqlConnection(connstring);
                            conn.Open();
                            var result = Convert.ToInt32(new NpgsqlCommand("SELECT COUNT(*) FROM users where username = '" + usernameTxb.Text + "'",conn).ExecuteScalar().ToString());
                            conn.Close();

                            if (result == 0)
                                Valid(usernameChkOutput, ref isUsernameOk);
                            else
                                Error(usernameChkOutput, "Már foglalt!", ref isUsernameOk);
                        }catch(Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                            Error(usernameChkOutput, "Adatbázis hiba!", ref isUsernameOk);
                        }
                    }
                    else
                        Error(usernameChkOutput, "Csak Angol ABC és szám lehet!", ref isUsernameOk);
                }
                else
                    Error(usernameChkOutput, "Túl Rövid!", ref isUsernameOk);
            }

            if (passwordOne.IsFocused == true)
            {
                if (passwordOne.Password.Length >= 5)
                    Valid(pw1ChkOutput, ref isPw1Ok);
                else
                    Error(pw1ChkOutput, "Minimum 5 karakter!", ref isPw1Ok);
            }

            if (passwordTwo.IsFocused == true)
            {
                if (passwordTwo.Password.Length >= 5)
                    if (passwordTwo.Password == passwordOne.Password)
                        Valid(pw2ChkOutput, ref isPw2Ok);
                    else
                        Error(pw2ChkOutput, "Nem egyeznek!", ref isPw2Ok);
                else
                    Error(pw2ChkOutput, "Minimum 5 karakter!", ref isPw2Ok);
            }

            if (confirmChb.IsChecked == true && isNameOk && isUsernameOk && isPw1Ok && isPw2Ok)
                okBtn.IsEnabled = true;
            else
                okBtn.IsEnabled = false;
        }

        private void nokBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);

                string trainedFor = "";

                foreach (var item in trained1.Children)
                {
                    if ((item as CheckBox).IsChecked == true)
                    {
                        trainedFor += (item as CheckBox).Content.ToString().Substring(0,2)+",";
                    }
                }

                foreach (var item in trained2.Children)
                {
                    if ((item as CheckBox).IsChecked == true)
                    {
                        trainedFor += (item as CheckBox).Content.ToString().Substring(0, 2) + ",";
                    }
                }

                foreach (var item in trained3.Children)
                {
                    if ((item as CheckBox).IsChecked == true)
                    {
                        trainedFor += (item as CheckBox).Content.ToString().Substring(0, 2) + ",";
                    }
                }

                conn.Open();
                new NpgsqlCommand("INSERT INTO users(\"name\",\"username\", \"password\", \"admin\", \"trained\", \"reg date\") values('"+nameTxb.Text+"', '"+usernameTxb.Text+"', crypt('"+passwordTwo.Password+"', gen_salt('md5')), false, '"+trainedFor+ "', current_timestamp);", conn).ExecuteNonQuery();
                conn.Close();

                this.Close();
                new MessageForm("Sikeres regisztráció!").Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
