using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ManageUsers.xaml
    /// </summary>
    public partial class ManageUsers : Window
    {
        DataSet dataSet = new DataSet();
        DataTable dataTable = new DataTable();

        public ManageUsers(bool admin)
        {
            InitializeComponent();

            if (admin == true)
                trainer.Visibility = Visibility.Visible;

            mainUserPanel.Visibility = Visibility.Hidden;
            output.Content = "";
            GetUsers();
        }

        private void GetUsers()
        {
            userList.Children.Clear();

            try
            {
                var connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();

                var dataAdapter = new NpgsqlDataAdapter("SELECT username FROM users", conn);
                dataSet.Reset();
                dataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];

                for (int j = 0; j < dataTable.Rows.Count; j++)
                {
                    Button newBtn = new Button();

                    newBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FDFDFD"));
                    newBtn.Foreground = Brushes.DarkSlateGray;
                    newBtn.BorderThickness = new Thickness(0);
                    newBtn.Focusable = false;
                    newBtn.Click += fillUserDatas;
                    newBtn.Content = dataTable.Rows[j][0].ToString();
                    newBtn.Width = 200;
                    newBtn.Margin = new Thickness(1, 1, 1, 0);
                    newBtn.FontSize = 15;

                    userList.Children.Add(newBtn);
                }

                conn.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void fillUserDatas(object sender, RoutedEventArgs e)
        {
            mainUserPanel.Visibility = Visibility.Visible;
            output.Content = "";

            var name = (sender as Button).Content.ToString();
            userNameLbl.Content = name;

            try
            {
                var connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                var trainedString = new NpgsqlCommand("SELECT trained FROM users WHERE username = '"+name+"'", conn).ExecuteScalar().ToString();
                conn.Close();

                foreach (var item in trained1.Children)
                {
                    if (trainedString.Contains((item as CheckBox).Content.ToString().Substring(0,2)))
                    {
                        (item as CheckBox).IsChecked = true;
                    }else
                        (item as CheckBox).IsChecked = false;
                }

                foreach (var item in trained2.Children)
                {
                    if (trainedString.Contains((item as CheckBox).Content.ToString().Substring(0, 2)))
                    {
                        (item as CheckBox).IsChecked = true;
                    }
                    else
                        (item as CheckBox).IsChecked = false;
                }

                foreach (var item in trained3.Children)
                {
                    if (trainedString.Contains((item as CheckBox).Content.ToString().Substring(0, 2)))
                    {
                        (item as CheckBox).IsChecked = true;
                    }
                    else
                        (item as CheckBox).IsChecked = false;
                }

                if (trainedString.Contains("Trainer"))
                    trainer.IsChecked = true;
                else
                    trainer.IsChecked = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.Show();
            this.Close();
        }

        private void saveChanges_Click(object sender, RoutedEventArgs e)
        {
            string trainedFor = "";

            foreach (var item in trained1.Children)
            {
                if ((item as CheckBox).IsChecked == true)
                {
                    trainedFor += (item as CheckBox).Content.ToString().Substring(0, 2) + ",";
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

            if (trainer.IsChecked == true)
            {
                trainedFor += "Trainer,";
            }

            try
            {
                var connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                    new NpgsqlCommand("UPDATE users set trained = '"+ trainedFor + "' WHERE username = '" + userNameLbl.Content + "'", conn).ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                output.Content = "Adatbázis hiba!";
                MessageBox.Show(ex.ToString());
            }

            output.Content = "A változások mentésre kerültek. A felhasználó következő belépésénél lépnek életbe!";
        }
    }
}
