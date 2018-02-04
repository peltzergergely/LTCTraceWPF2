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
using System.ComponentModel;
using System.Collections.ObjectModel;

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
            FillWorkStationList();
            FillProductNames();
            TreeViewMaker();
        }

        public IDictionary<string, string> workSteps = new Dictionary<string, string>();

        public void FillWorkStationList()
        {
            workSteps.Add("11 FB ACDC Szerelés", "fb_acdc_assy");
            workSteps.Add("12 FB EMC Szerelés", "fb_emc_assy");
            workSteps.Add("21 MB HS Szerelés", "mb_hs_assy");
            workSteps.Add("22 MB DSP Szerelés", "mb_dsp_assy");
            workSteps.Add("31 Ház Leak Teszt I.", "housing_leak_test_one");
            workSteps.Add("32 Ház FB Szerelés", "housing_fb_assy");
            workSteps.Add("33 HiPot Teszt I.", "hipot_test_one");
            workSteps.Add("34 Ház Konnektor Szerelés", "housing_connector_assy");
            workSteps.Add("41 Végszerelés I.", "housing_connector_assy");
            workSteps.Add("42 Kalibráció", "calibration");
            workSteps.Add("43 Végszerelés II.", "final_assy_two");
            workSteps.Add("44 Leak Teszt II.", "leak_test_two");
            workSteps.Add("45 Hipot Teszt II.", "hipot_test_two");
            workSteps.Add("46 EOL", "eol");
            workSteps.Add("47 Firewall", "firewall");

            workStationTableName.ItemsSource = workSteps;
            workStationTableName.DisplayMemberPath = "Key";
            workStationTableName.SelectedValuePath = "Value";
            workStationTableName.SelectedIndex = 0;

            //megmutatja a hozzá tartozó értéket - a tábla nevét
            //MessageBox.Show(cbox.SelectedValue.ToString());
        }

        public IDictionary<string, string> columnName = new Dictionary<string, string>();

        public void FillProductNames()
        {
            columnName.Add("", "");
            columnName.Add("Filterboard DM", "fb_dm");
            columnName.Add("Ház DM", "housing_dm");
            columnName.Add("Mainboard DM", "mb_dm");
            columnName.Add("Gateway DM", "gw_dm");

            searchedField.ItemsSource = columnName;
            searchedField.DisplayMemberPath = "Key";
            searchedField.SelectedValuePath = "Value";
            searchedField.SelectedIndex = 0;
        }

        private void QueryGen()
        {
            string query = "";
            if (queryTb.Text == "" || queryTb.Text == "*")
            {
                query = "SELECT * FROM " + workStationTableName.SelectedValue.ToString();
            }
            else
            {
                query = "SELECT * FROM " + workStationTableName.SelectedValue.ToString() + " WHERE " + searchedField.SelectedValue.ToString() + " = '" + queryTb.Text + "'";
            }
            table_select(query);
        }

        private void TreeViewMaker()
        {
            TreeViewItem ceo = new TreeViewItem() { Header = "CEO" };
            TreeViewItem manager1 = new TreeViewItem() { Header = "Manager1" };
            TreeViewItem manager2 = new TreeViewItem() { Header = "Manager2" };
            TreeViewItem person1 = new TreeViewItem() { Header = "person1" };
            TreeViewItem person2 = new TreeViewItem() { Header = "person2" };

            manager1.Items.Add(person1);
            manager2.Items.Add(person2);
            ceo.Items.Add(manager1);
            ceo.Items.Add(manager2);
            tree.Items.Add(ceo);
        }

        private DataSet dataSet = new DataSet();
        private DataTable dataTable = new DataTable();

        private void table_select(string query)
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
                dataGridView1.ItemsSource = dataTable.AsDataView();
                conn.Close();
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.ToString());
            }
            dataGridView1.Columns[0].Width = 70;
        }

        //select * from firewall where housing_dm = ###
        //select * from eol where housing_dm = ###
        //select * from hipot_test_two where housing_dm = ###
        //select * from leak_test_two where housing_dm = ###
        //select * from firewall where housing_dm = ###
        //select * from firewall where housing_dm = ###

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listDbBtn_Click(object sender, RoutedEventArgs e)
        {
            QueryGen();
        }
    }
}
