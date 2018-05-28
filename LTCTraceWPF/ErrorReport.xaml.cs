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

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ErrorReport.xaml
    /// </summary>
    public partial class ErrorReport : Window
    {
        public ErrorReport()
        {
            InitializeComponent();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DbInsert(string table) //DB insert
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                // Making connection with Npgsql provider
                var conn = new NpgsqlConnection(connstring);
                var UploadMoment = DateTime.Now;
                conn.Open();
                // building SQL query
                var cmd = new NpgsqlCommand("INSERT INTO " + table + " (comments, created_on) " +
                    "VALUES(:comments, :created_on)", conn);
                cmd.Parameters.Add(new NpgsqlParameter("comments", reportTxbx.Text));
                cmd.Parameters.Add(new NpgsqlParameter("created_on", UploadMoment));
                cmd.ExecuteNonQuery();
                //closing connection ASAP
                conn.Close();
                CallMessageForm("Adatok feltöltve!");
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
        }

        private void CallMessageForm(string msgToShow)
        {
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            DbInsert("errorreport");
        }

        private void reportTxbx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (reportTxbx.Text == "Ide írhatsz.")
            {
                reportTxbx.Text = "";
            }
        }
    }
}
