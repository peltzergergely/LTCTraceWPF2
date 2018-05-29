using Microsoft.Win32;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for DbWindow.xaml
    /// </summary>
    public partial class DbWindow : Window
    {
        public IDictionary<string, string> workSteps = new Dictionary<string, string>();
        public ShowImageWindow w;
        private string dbQueryOffset;

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

            w = new ShowImageWindow();
            dbQueryOffset = "0";
        }

        public void FillWorkStationList()
        {
            workSteps.Add("00 Transistor Dátumok", "transdate");
            workSteps.Add("11 MB HS Szerelés", "mb_hs_assy");
            workSteps.Add("12 MB DSP Szerelés", "mb_dsp_assy");
            workSteps.Add("21 FB ACDC Szerelés", "fb_acdc_assy");
            workSteps.Add("22 FB EMC Szerelés", "fb_emc_assy");
            workSteps.Add("31 Ház Leak Teszt I.", "housing_leak_test_one");
            workSteps.Add("32 Cooling Leak Teszt", "cooling_leak_test");
            workSteps.Add("33 Ház FB Szerelés", "housing_fb_assy");
            workSteps.Add("34 Potting után Kapton", "potting");
            workSteps.Add("35 Ház Konnektor Szerelés", "housing_connector_assy");
            workSteps.Add("41 Végszerelés I.", "final_assy_one");
            workSteps.Add("42 HiPot Teszt I.", "hipot_test_one");
            workSteps.Add("43 Kalibráció", "calibration");
            workSteps.Add("44 Végszerelés II.", "final_assy_two");
            workSteps.Add("45 Leak Teszt II.", "housing_leak_test_two");
            workSteps.Add("46 Hipot Teszt II.", "hipot_test_two");
            workSteps.Add("47 EOL", "eol");
            workSteps.Add("48 Firewall", "firewall");
            workSteps.Add("-- INTERLOCK HIBÁK", "interlock_log");
            workSteps.Add("-- HIBAJELENTÉS", "errorreport");

            workStationCbx.ItemsSource = workSteps;
            workStationCbx.DisplayMemberPath = "Key";
            workStationCbx.SelectedValuePath = "Value";
            workStationCbx.SelectedIndex = 0;

            // set back the short date pattern to dd-MM-yyyy after lacquer load (MM-yyyy)
            CultureInfo ci = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
            ci.DateTimeFormat.ShortDatePattern = "dd-MM-yyyy";
            Thread.CurrentThread.CurrentCulture = ci;

            // Starting and Ending date
            //startDate.SelectedDate = new DateTime(2017, 10, 01);
            startDate.SelectedDate = DateTime.Today;
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

            if (prodDmTbx.Text.Length > 0 && prodCbx.SelectedIndex != 0 && (workStationCbx.SelectedValue.ToString() != "interlock_log" || workStationCbx.SelectedValue.ToString() != "errorreport"))
            {
                Querycmd = Querycmd + " AND " + prodCbx.SelectedValue.ToString() + " = '" + prodDmTbx.Text + "'";
            }

            #region temporary solution while errorreport can be listed
            if (workStationCbx.SelectedValue.ToString() == "errorreport")
            {
                Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " order by id desc";
                //// if there are many then you can filter by date
                //Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " WHERE date(created_on) >= " + start + " and date(created_on) <= " + end +" order by id desc";
            }
            #endregion
            return Querycmd +" offset "+dbQueryOffset+" limit 200";
        }

        private string getSQLcount()
        {

            string start = "'" + startDate.SelectedDate.Value.Year.ToString() + "-" + startDate.SelectedDate.Value.Month.ToString() + "-" + startDate.SelectedDate.Value.Day.ToString() + "'";
            string end = "'" + endDate.SelectedDate.Value.Year.ToString() + "-" + endDate.SelectedDate.Value.Month.ToString() + "-" + endDate.SelectedDate.Value.Day.ToString() + "'";

            string Querycmd = "SELECT COUNT(*) FROM " + workStationCbx.SelectedValue.ToString() + " WHERE date(saved_on) >= " + start + " and date(saved_on) <= " + end;

            if (prodDmTbx.Text.Length > 0 && prodCbx.SelectedIndex != 0 && (workStationCbx.SelectedValue.ToString() != "interlock_log" || workStationCbx.SelectedValue.ToString() != "errorreport"))
            {
                Querycmd = Querycmd + " AND " + prodCbx.SelectedValue.ToString() + " = '" + prodDmTbx.Text + "'";
            }

            #region temporary solution while errorreport can be listed
            if (workStationCbx.SelectedValue.ToString() == "errorreport")
            {
                Querycmd = "SELECT COUNT(*) FROM " + workStationCbx.SelectedValue.ToString();
                //// if there are many then you can filter by date
                //Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " WHERE date(created_on) >= " + start + " and date(created_on) <= " + end +" order by id desc";
            }
            #endregion
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

        private  void ListBtn_Click(object sender, RoutedEventArgs e)
        {
            Int32 Tabcount = 0;
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

                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        if (dataTable.Columns[i].ColumnName.Contains("pic"))
                        {
                            for (int j = 0; j < dataTable.Rows.Count; j++)
                            {
                                byte[] blob = (byte[])dataTable.Rows[j][i];
                                MemoryStream stream = new MemoryStream();
                                if (blob.Length > 10)
                                {
                                    // change the "byte [] array" text to something more readable.
                                }
                                else
                                {
                                    dataTable.Rows[j][i] = null;
                                }
                            }

                        }
                    }

                    // get the query result count
                    var countcmd = new NpgsqlCommand(getSQLcount(), conn);
                    Tabcount = Convert.ToInt32(countcmd.ExecuteScalar());
                    resultRowCount.Content = Tabcount;

                    conn.Close();

                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }
                resultDataGrid.Columns[0].Width = 70;

                if (resultDataGrid.Columns[1].Header.ToString() == "comments")
                {
                    resultDataGrid.Columns[1].Width = 1500;
                }
            }

            Tabs.Children.Clear();
            for (int i = 0; i <= Tabcount/ 200; i++)
            {
                Button newBtn = new Button();

                newBtn.Background = Brushes.White;
                newBtn.Foreground = Brushes.DarkSlateGray;
                newBtn.BorderThickness = new Thickness(0);
                newBtn.Focusable = false;
                newBtn.Click += getOffset;
                newBtn.Content = (i+1).ToString();
                newBtn.Name = "Tab" + (i+1).ToString();
                newBtn.Width = 28;
                newBtn.Margin = new Thickness(1, 1, 1, 0);
                newBtn.FontSize = 15;

                Tabs.Children.Add(newBtn);

                //change back and foreground color on active button
                if (int.Parse(dbQueryOffset) == i*200)
                {
                    newBtn.Background = Brushes.DarkSlateGray;
                    newBtn.Foreground = Brushes.White;
                }
            }
            dbQueryOffset = "0";
        }

        private void getOffset(object sender, RoutedEventArgs e)
        {
            dbQueryOffset = ((int.Parse((sender as Button).Content.ToString()+"00")-100)*2).ToString();
            ListBtn_Click(sender, e);
        }

        #region datagrid helper functions

        #endregion

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
            String resultat = "";
            String result = "";

            var connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            using (var conn = new NpgsqlConnection(connstring))
            {
                conn.Open();

                for (int i = 0; i < Tabs.Children.Count; i++)
                {
                    //fill datagrid
                    string sql = getSQLcommand();
                    var dataAdapter = new NpgsqlDataAdapter(sql, conn);
                    dataSet.Reset();
                    dataAdapter.Fill(dataSet);
                    dataTable = dataSet.Tables[0];
                    resultDataGrid.ItemsSource = dataTable.AsDataView();

                    dbQueryOffset = ((i+1) * 200).ToString();

                    // concatenate the tabs
                    resultDataGrid.SelectAllCells();
                    if (i == 0)
                        resultDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                    else
                        resultDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
                    ApplicationCommands.Copy.Execute(null, resultDataGrid);
                    resultat += (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                    result += (string)Clipboard.GetData(DataFormats.Text);
                    resultDataGrid.UnselectAllCells();

                    // set back the default result
                    if (i == Tabs.Children.Count-1)
                    {
                        foreach (var item in Tabs.Children)
                        {
                            if ((item as Button).Background == Brushes.DarkSlateGray)
                                dbQueryOffset = (int.Parse((item as Button).Content.ToString())*200-200).ToString();
                        }
                        ListBtn_Click(sender, e as RoutedEventArgs);

                    }
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel |*.xls";
            //saveFileDialog.DefaultExt = "xls";
            //saveFileDialog.AddExtension = true;
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

        private void ShowImage(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (resultDataGrid.Items.Count > 0) // there is row in datagrid
                {
                    //get the indexes
                    int column = resultDataGrid.CurrentColumn.DisplayIndex;
                    int row = resultDataGrid.Items.IndexOf(resultDataGrid.CurrentItem);

                    if (dataTable.Columns[column].ColumnName.Contains("pic") && !(dataTable.Rows[row][column] is DBNull)) //check if the focused cell is a picture
                    {
                        byte[] blob = (byte[])dataTable.Rows[row][column];
                        MemoryStream stream = new MemoryStream();
                        if (blob.Length > 10)
                        {
                            stream.Write(blob, 0, blob.Length);
                            stream.Position = 0;
                            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            MemoryStream ms = new MemoryStream();
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Seek(0, SeekOrigin.Begin);
                            bi.StreamSource = ms;
                            bi.EndInit();

                            w.SetImg(bi);
                        }
                        else
                        {
                            w.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        w.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception)
            {

            }

        }

        private void quantityBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
                var conn = new NpgsqlConnection(connstring);
                conn.Open();

                string start = "'" + startDate.SelectedDate.Value.Year.ToString() + "-" + startDate.SelectedDate.Value.Month.ToString() + "-" + startDate.SelectedDate.Value.Day.ToString() + "'";
                string end = "'" + endDate.SelectedDate.Value.Year.ToString() + "-" + endDate.SelectedDate.Value.Month.ToString() + "-" + endDate.SelectedDate.Value.Day.ToString() + "'";

                new NpgsqlCommand("SELECT todaycount(" + start + "," + end + ");", conn).ExecuteNonQuery();
                var dataAdapter = new NpgsqlDataAdapter("SELECT * FROM count_today_product", conn);
                dataSet.Reset();
                dataAdapter.Fill(dataSet);
                dataTable = dataSet.Tables[0];
                resultDataGrid.ItemsSource = dataTable.AsDataView();

                conn.Close();

                int sum = 0;
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    sum += int.Parse(dataTable.Rows[i][2].ToString());
                }
                resultRowCount.Content = sum.ToString();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
