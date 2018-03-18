using Microsoft.Win32;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            if (ConfigurationManager.AppSettings["DBQueryBox"] == "false")
            {
                //costumquery.Visibility = Visibility.Hidden;
            }
            InitializeComponent();
            FillWorkStationList();
            FillProductNames();
        }

        public IDictionary<string, string> workSteps = new Dictionary<string, string>();

        public void FillWorkStationList()
        {
            workSteps.Add("00 Transistor Dátumok", "transdate");
            workSteps.Add("11 MB HS Szerelés", "mb_hs_assy");
            workSteps.Add("12 MB DSP Szerelés", "mb_dsp_assy");
            workSteps.Add("21 FB ACDC Szerelés", "fb_acdc_assy");
            workSteps.Add("22 FB EMC Szerelés", "fb_emc_assy");
            workSteps.Add("31 Ház Leak Teszt I.", "housing_leak_test_one");
            workSteps.Add("32 Ház FB Szerelés", "housing_fb_assy");
            workSteps.Add("33 Potting után Kapton", "potting");
            workSteps.Add("34 Ház Konnektor Szerelés", "housing_connector_assy");
            workSteps.Add("41 Végszerelés I.", "housing_connector_assy");
            workSteps.Add("42 HiPot Teszt I.", "hipot_test_one");
            workSteps.Add("43 Kalibráció", "calibration");
            workSteps.Add("44 Végszerelés II.", "final_assy_two");
            workSteps.Add("45 Leak Teszt II.", "housing_leak_test_two");
            workSteps.Add("46 Hipot Teszt II.", "hipot_test_two");
            workSteps.Add("47 EOL", "eol");
            workSteps.Add("48 Firewall", "firewall");

            workStationCbx.ItemsSource = workSteps;
            workStationCbx.DisplayMemberPath = "Key";
            workStationCbx.SelectedValuePath = "Value";
            workStationCbx.SelectedIndex = 0;

            // set back the short date pattern to dd-MM-yyyy after lacquer load (MM-yyyy)
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            Thread.CurrentThread.CurrentCulture = ci;

            // Starting and Ending date
            startDate.SelectedDate = new DateTime(2017, 10, 01);
            endDate.SelectedDate = DateTime.Today;

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

            prodCbx.ItemsSource = columnName;
            prodCbx.DisplayMemberPath = "Key";
            prodCbx.SelectedValuePath = "Value";
            prodCbx.SelectedIndex = 0;
        }

        private string getSQLcommand()
        {

            string start = "'" + startDate.SelectedDate.Value.Year.ToString() + "-" + startDate.SelectedDate.Value.Month.ToString() + "-" + startDate.SelectedDate.Value.Day.ToString() + "'";
            string end = "'" + endDate.SelectedDate.Value.Year.ToString() + "-" + endDate.SelectedDate.Value.Month.ToString() + "-" + endDate.SelectedDate.Value.Day.ToString() + "'";

            string Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " WHERE date(saved_on) >= " + start + " and date(saved_on) <= " + end;

            if (prodDmTbx.Text.Length > 0 && prodCbx.SelectedIndex != 0)
            {
                Querycmd = Querycmd + " AND " + prodCbx.SelectedValue.ToString() + " = '" + prodDmTbx.Text + "'";
            }


            return Querycmd;
        }

        private void QueryGen()
        {
            //string query = "";
            //if (queryTb.Text == "" || queryTb.Text == "*")
            //{
            //    query = "SELECT * FROM " + workStationTableName.SelectedValue.ToString();
            //}
            //else
            //{
            //    query = "SELECT * FROM " + workStationTableName.SelectedValue.ToString() + " WHERE " + searchedField.SelectedValue.ToString() + " = '" + queryTb.Text + "'";
            //}
            //table_select(query);
        }

        private DataSet dataSet = new DataSet();
        private DataTable dataTable = new DataTable();


        private void ListBtn_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                try
                {
                    string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                    var conn = new NpgsqlConnection(connstring);
                    conn.Open();
                    string sql = getSQLcommand();//query;
                    var dataAdapter = new NpgsqlDataAdapter(sql, conn);
                    dataSet.Reset();
                    dataAdapter.Fill(dataSet);
                    dataTable = dataSet.Tables[0];
                    resultDataGrid.ItemsSource = dataTable.AsDataView();
                    conn.Close();

                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }
                resultDataGrid.Columns[0].Width = 70;

                for (int i = 0; i < resultDataGrid.Columns.Count; i++)
                {
                    if ((resultDataGrid.Columns[i].Header).ToString().Contains("pic"))
                    {
                        resultDataGrid.Columns[i].Visibility = Visibility.Hidden;
                    }
                }
            }
                resultRowCount.Content = resultDataGrid.Items.Count;

        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void listDbBtn_Click(object sender, RoutedEventArgs e)
        {
            QueryGen();
        }

        private void imgBtn_Click(object sender, RoutedEventArgs e)
        {
            var ImgViewer = new ImgViewer();
            ImgViewer.Show();
        }

        private void reportBtn_Click(object sender, RoutedEventArgs e)
        {
            var report = new Report();
            report.Show();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            resultDataGrid.SelectAllCells();
            resultDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, resultDataGrid);
            String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            String result = (string)Clipboard.GetData(DataFormats.Text);
            resultDataGrid.UnselectAllCells();

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel |*.xls";
            if (saveFileDialog.ShowDialog() == true)
            {
                System.IO.StreamWriter file1 = new System.IO.StreamWriter(saveFileDialog.FileName);
                file1.WriteLine(result.Replace(',', ' '));
                file1.Close();
            }
        }

        private void ReportBtn_Click_1(object sender, RoutedEventArgs e)
        {
            var reportWindow = new Report();
            reportWindow.Show();
        }
    }
}
