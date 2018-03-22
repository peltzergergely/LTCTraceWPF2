using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Xml;

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
        private string MbDm;
        private string FbDm;

        private StringBuilder strbuilder = new StringBuilder();

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
                    strbuilder.AppendFormat(WoName + "\r\n");
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        strbuilder.AppendFormat("   {0, -25}\t{1}\r\n", dataTable.Columns[j].ColumnName, dataTable.Rows[i][j]);
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
            catch (Exception)
            {
                strbuilder.AppendLine("Error or Missing: " + WoName);
            }
            strbuilder.AppendLine();
        }

        private void reportGenBtn_Click(object sender, RoutedEventArgs e)
        {
            //QueryDb(costumquery.Text);
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string folderpath = Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_FB" + searchedDM.Text).ToString();
            Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_" + HousingDm);

            HousingDm = searchedDM.Text;
            QueryDb("48 Firewall", "SELECT housing_dm, pc_name, started_on, saved_on FROM firewall WHERE housing_dm = '" + HousingDm + "'");
            ImageSaver("SELECT * FROM firewall WHERE housing_dm = '" + HousingDm + "'", folderpath);
            QueryDb("47 EOL", "SELECT housing_dm, test_result, internal_id, pc_name, started_on, saved_on FROM eol WHERE housing_dm = '" + HousingDm + "'");
            XmlSaver("select * from eol where housing_dm = '" + HousingDm + "'");
            QueryDb("46 HiPot II.", "SELECT housing_dm, test_result, pc_name, started_on, saved_on FROM hipot_test_two WHERE housing_dm = '" + HousingDm + "'");
            XmlSaver("select * from hipot_test_two where housing_dm = '" + HousingDm + "'");
            QueryDb("45 Leak Test II.", "SELECT housing_dm, leak_test_result, pc_name, saved_on FROM housing_leak_test_two WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("44 Final Assy II.", "SELECT housing_dm, gw_dm, pc_name, started_on, saved_on FROM final_assy_two WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("43 Calibration ", "SELECT housing_dm, test_result, internal_id, pc_name, started_on, saved_on FROM calibration WHERE housing_dm = '" + HousingDm + "'");
            XmlSaver("select * from calibration where housing_dm = '" + HousingDm + "'");
            QueryDb("32 HiPot Test I. ", "SELECT housing_dm, test_result, pc_name, started_on, saved_on FROM hipot_test_one WHERE housing_dm = '" + HousingDm + "'");
            XmlSaver("select * from hipot_test_one where housing_dm = '" + HousingDm + "'");
            QueryDb("41 Final Assy I. ", "SELECT housing_dm, mb_dm, pc_name, started_on, saved_on FROM final_assy_one WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("34 Housing Connector Assy ", "SELECT housing_dm, pc_name, started_on, saved_on FROM housing_connector_assy WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("33 Potting ", "SELECT housing_dm, pc_name, started_on, saved_on FROM housing_fb_assy WHERE housing_dm = '" + HousingDm + "'");
            ImageSaver("SELECT * FROM potting WHERE housing_dm = '" + HousingDm + "'", folderpath);
            QueryDb("32 Housing FB Assy ", "SELECT housing_dm, fb_dm, pc_name, started_on, saved_on FROM housing_fb_assy WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("31 Leak Test I. ", "SELECT housing_dm, leak_test_result, pc_name, saved_on FROM housing_leak_test_one WHERE housing_dm = '" + HousingDm + "'");
            QueryDb("22 Filterboard EMC Assy ", "SELECT fb_dm, pc_name, started_on, saved_on FROM fb_emc_assy WHERE fb_dm = '" + FbDm + "'");
            ImageSaver("SELECT * FROM fb_emc_assy WHERE fb_dm = '" + FbDm + "'", folderpath);
            QueryDb("21 Filterboard ACDC Assy ", "SELECT fb_dm, pc_name, started_on, saved_on FROM fb_acdc_assy WHERE fb_dm = '" + FbDm + "'");
            QueryDb("12 Mainboard DSP Assy ", "SELECT mb_dm, dsp_one_one, dsp_one_two, dsp_one_three, dsp_two_one, dsp_two_two, dsp_two_three, pc_name, started_on, saved_on FROM mb_dsp_assy WHERE mb_dm = '" + MbDm + "'");
            ImageSaver("SELECT * FROM mb_dsp_assy WHERE mb_dm = '" + MbDm + "'", folderpath);
            QueryDb("11 Mainboard Heatsink Assy ", "SELECT mb_dm, pc_name, started_on, saved_on FROM mb_dsp_assy WHERE mb_dm = '" + MbDm + "'");
            txtBlck.Text = strbuilder.ToString();
            var complete = Path.Combine(systemPath + "\\LTCReportFolder\\Report_" + HousingDm, HousingDm + ".txt");
            //StreamWriter file = new StreamWriter(complete);
            System.Diagnostics.Process.Start("explorer.exe", systemPath + "\\LTCReportFolder");

            using (StreamWriter writetext = new StreamWriter(complete))
            {
                writetext.WriteLine(strbuilder);
            }
            //file.Write(strbuilder.ToString()); // "sb" is the StringBuilder
        }

        DataSet ds;

        private void XmlSaver(string query)
        {
            string constr = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_" + HousingDm);
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(constr))
                {
                    conn.Open();
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, conn))
                    {
                        ds = new DataSet("myDataSet");
                        adapter.Fill(ds);
                        DataTable dt = ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Text += "\nError with filesaving SQL: " + ex.Message + "\n";
            }

            try //saving the xml(s)
            {
                DataTable dataTable = ds.Tables[0];
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        if (dataTable.Columns[j].ColumnName == "file" || dataTable.Columns[j].ColumnName == "file1")
                        {
                            byte[] blob = (byte[])dataTable.Rows[i][j];
                            string filename = (string)dataTable.Rows[i][j - 1];
                            File.WriteAllBytes(systemPath + "\\LTCReportFolder\\Report_" + HousingDm + "\\" + filename, blob);
                        }
                        if (dataTable.Columns[j].ColumnName == "logfile")
                        {
                            byte[] blob = (byte[])dataTable.Rows[i][j];
                            string filename = (string)dataTable.Rows[i][j + 1];
                            File.WriteAllBytes(systemPath + "\\LTCReportFolder\\Report_" + HousingDm + "\\" + filename, blob);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Text += "\nError with file saving: " + ex.Message + "\n";
            }
        }

        private void ImageSaver(string query, string folderpath)
        {
            string constr = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
            //var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            //Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_" + HousingDm);
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(constr))
                {
                    conn.Open();
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(query, conn))
                    {
                        ds = new DataSet("myDataSet");
                        adapter.Fill(ds);
                        DataTable dt = ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Text += "\n" + "Error with imagesaver SQL: " + ex.Message + "\n";
            }

            try //saving the images
            {
                DataTable dataTable = ds.Tables[0];
                int i = 0;
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {

                    if (dataTable.Columns[j].ColumnName.Contains("pic"))
                    {
                        byte[] blob = (byte[])dataTable.Rows[i][j];
                        if (blob.Length > 5)
                        {
                            MemoryStream stream = new MemoryStream();
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
                            var complete = Path.Combine(folderpath, dataTable.Rows[i][1] + "" + dataTable.Rows[i][0] + "_" + j + ".Jpeg");
                            img.Save(complete, ImageFormat.Jpeg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.Text += "\n" + "Error with image saving: " + ex.Message + "\n";
            }
        }

        private void FBReportGenBtn_Click(object sender, RoutedEventArgs e)
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string folderpath = systemPath + "\\LTCReportFolder\\Report_FB" + searchedDM.Text;
            Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_FB" + searchedDM.Text);

            QueryDb("22 Filterboard EMC Assy ", "SELECT fb_dm, pc_name, started_on, saved_on FROM fb_emc_assy WHERE fb_dm = '" + searchedDM.Text + "'");
            ImageSaver("SELECT * FROM fb_emc_assy WHERE fb_dm = '" + searchedDM.Text + "'", folderpath);
            QueryDb("21 Filterboard ACDC Assy ", "SELECT fb_dm, pc_name, started_on, saved_on FROM fb_acdc_assy WHERE fb_dm = '" + searchedDM.Text + "'");
            txtBlck.Text = strbuilder.ToString();

            var complete = Path.Combine(systemPath + "\\LTCReportFolder\\Report_FB" + searchedDM.Text, searchedDM.Text + ".txt");
            //StreamWriter file = new StreamWriter(complete);
            System.Diagnostics.Process.Start("explorer.exe", systemPath + "\\LTCReportFolder");

            using (StreamWriter writetext = new StreamWriter(complete))
            {
                writetext.WriteLine(strbuilder);
            }
        }

        private void MBReportGenBtn_Click(object sender, RoutedEventArgs e)
        {
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string folderpath = systemPath + "\\LTCReportFolder\\Report_MB" + searchedDM.Text;
            Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_MB" + searchedDM.Text);

            QueryDb("12 Mainboard DSP Assy ", "SELECT mb_dm, dsp_one_one, dsp_one_two, dsp_one_three, dsp_two_one, dsp_two_two, dsp_two_three, pc_name, started_on, saved_on FROM mb_dsp_assy WHERE mb_dm = '" + searchedDM.Text + "'");
            ImageSaver("SELECT * FROM mb_dsp_assy WHERE mb_dm = '" + searchedDM.Text + "'", folderpath);
            QueryDb("11 Mainboard Heatsink Assy ", "SELECT mb_dm, pc_name, started_on, saved_on FROM mb_dsp_assy WHERE mb_dm = '" + searchedDM.Text + "'");
            txtBlck.Text = strbuilder.ToString();
            
            var complete = Path.Combine(systemPath + "\\LTCReportFolder\\Report_MB" + searchedDM.Text, searchedDM.Text + ".txt");
            //StreamWriter file = new StreamWriter(complete);
            System.Diagnostics.Process.Start("explorer.exe", systemPath + "\\LTCReportFolder");

            using (StreamWriter writetext = new StreamWriter(complete))
            {
                writetext.WriteLine(strbuilder);
            }
        }
    }
}
