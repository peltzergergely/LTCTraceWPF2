using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        public Report()
        {
            InitializeComponent();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private DataSet dataSet = new DataSet();
        private DataTable dataTable = new DataTable();
        private string HousingDm;
        private string GwDm;
        private string MbDm;
        private string FbDm;

        private StringBuilder sb = new StringBuilder();

        private void QueryDb(string WoName, string query)
        {
            try
            {
                string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();
                string sql = query;
                var dataAdapter = new NpgsqlDataAdapter(sql, conn);
                dataSet.Reset();
                dataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
                conn.Close();
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    sb.AppendFormat(WoName + "\n");
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        sb.AppendFormat("   {0, -20}{1}\n", dataTable.Columns[j].ColumnName, dataTable.Rows[i][j]);
                        if (dataTable.Columns[j].ColumnName == "mb_dm")
                        {
                            MbDm = dataTable.Rows[i][j].ToString();
                        }
                        if (dataTable.Columns[j].ColumnName == "fb_dm")
                        {
                            FbDm = dataTable.Rows[i][j].ToString();
                        }
                    }
                }
            }
            catch (Exception msg)
            {
                //MessageBox.Show(msg.ToString());
                sb.AppendLine("Error or Missing: " + WoName);
            }
            sb.AppendLine();
            sb.AppendLine("=============================\n");

        }

        private void reportGenBtn_Click(object sender, RoutedEventArgs e)
        {
            //QueryDb(costumquery.Text);
            HousingDm = searchedDM.Text;
            QueryDb("47 Firewall", "SELECT housing_dm, pc_name, started_on, saved_on FROM firewall WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("46 EOL", "SELECT housing_dm, pc_name, started_on, saved_on FROM eol WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("45 HiPot II.", "SELECT housing_dm, test_result, pc_name, started_on, saved_on FROM hipot_test_two WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("44 Leak Test II.", "SELECT housing_dm, leak_test_result, pc_name, created_on FROM housing_leak_test_two WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("43 Final Assy II.", "SELECT housing_dm, pc_name, started_on, saved_on FROM final_assy_two WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("42 Calibration ", "SELECT housing_dm, pc_name, started_on, saved_on FROM calibration WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("41 Final Assy I. ", "SELECT housing_dm, mb_dm, pc_name, started_on, saved_on FROM final_assy_one WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("34 Housing Connector Assy ", "SELECT housing_dm, pc_name, started_on, saved_on FROM housing_connector_assy WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("33 HiPot Test I. ", "SELECT housing_dm, test_result, pc_name, started_on, saved_on FROM hipot_test_one WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("32 Housing FB Assy ", "SELECT housing_dm, fb_dm, pc_name, started_on, saved_on FROM housing_fb_assy WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("32 Leak Test I. ", "SELECT housing_dm, leak_test_result, pc_name, created_on FROM housing_leak_test_one WHERE housing_dm = '" + HousingDm + "'");
            txtBlck.Text = sb.ToString();
        }
    }
}
