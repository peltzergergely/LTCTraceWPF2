using System;
using System.Collections.Generic;
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
using Npgsql;
using System.Configuration;
using System.Data;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for DbWindow.xaml
    /// </summary>
    public partial class DbWindow : Window
    {
        public DbWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        private DataSet ds = new DataSet();
        private DataTable dt = new DataTable();

        private void table_select(string query)
        {
            try
            {
                // connstring stored in App.config
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = query;
                // data adapter making request from our connection
                var da = new NpgsqlDataAdapter(sql, conn);
                ds.Reset();
                // filling DataSet with result from NpgsqlDataAdapter
                da.Fill(ds);
                // since it C# DataSet can handle multiple tables, we will select first
                dt = ds.Tables[0];
                // connect grid to DataTable
                dataGridView1.ItemsSource  = dt.AsDataView();
                // since we only showing the result we don't need connection anymore
                conn.Close();
            }
            catch (Exception msg)
            {
                // error handling
                MessageBox.Show(msg.ToString());
                //throw;
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listDbBtn_Click(object sender, RoutedEventArgs e)
        {
            table_select(queryTb.Text);
        }
    }
}
