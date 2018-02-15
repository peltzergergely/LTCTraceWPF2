using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LTCTraceWPF
{
    public static class ImageLoader
    {
        public static List<BitmapImage> LoadImages()
        {
            List<BitmapImage> robotImages = new List<BitmapImage>();
            DirectoryInfo robotImageDir = new DirectoryInfo(@"C:\ProgramData\LTCReportFolder\Report_Images");
            foreach (FileInfo robotImageFile in robotImageDir.GetFiles("*.Jpeg"))
            {
                Uri uri = new Uri(robotImageFile.FullName);
                robotImages.Add(new BitmapImage(uri));
            }
            return robotImages;
        }
    }
    /// <summary>
    /// Interaction logic for ImgViewer.xaml
    /// </summary>
    public partial class ImgViewer : Window
    {
        public ImgViewer()
        {
            InitializeComponent();
        }

        public static List<BitmapImage> LoadImages()
        {
            List<BitmapImage> robotImages = new List<BitmapImage>();
            DirectoryInfo robotImageDir = new DirectoryInfo(@"C:\ProgramData\LTCReportFolder\Report_Images");
            foreach (FileInfo robotImageFile in robotImageDir.GetFiles("*.Jpeg"))
            {
                Uri uri = new Uri(robotImageFile.FullName);
                robotImages.Add(new BitmapImage(uri));
            }
            return robotImages;
        }

        DataSet ds;

        string constr = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;

        private void listDbBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(constr))
                {
                    conn.Open();

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(queryTb.Text, conn))
                    {
                        ds = new DataSet("myDataSet");
                        adapter.Fill(ds);
                        DataTable dt = ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                DataTable dataTable = ds.Tables[0];
                //foreach (DataRow column in dataTable.Rows)
                int picCounter = 0;
                //for (int i = 0; i < dataTable.Rows.Count; i++)
                //{
                int i = 0;
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (dataTable.Columns[j].ColumnName.Contains("pic"))
                    {
                        picCounter += 1;
                        //Store binary data read from the database in a byte array
                        byte[] blob = (byte[])dataTable.Rows[i][j];
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
                            if (picCounter == 1)
                                image0.Source = bi;
                            if (picCounter == 2)
                                image1.Source = bi;
                            if (picCounter == 3)
                                image2.Source = bi;
                            if (picCounter == 4)
                                image3.Source = bi;
                            if (picCounter == 5)
                                image4.Source = bi;
                            if (picCounter == 6)
                                image5.Source = bi;
                            if (picCounter == 7)
                                image6.Source = bi;
                            if (picCounter == 8)
                                image7.Source = bi;
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveImgBtn_Click(object sender, RoutedEventArgs e)
        {
            var systemPath = System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            System.IO.Directory.CreateDirectory(systemPath + "\\LTCReportFolder\\Report_Images");
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(constr))
                {
                    conn.Open();
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(queryTb.Text, conn))
                    {
                        ds = new DataSet("myDataSet");
                        adapter.Fill(ds);
                        DataTable dt = ds.Tables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try //saving the images
            {
                DataTable dataTable = ds.Tables[0];
                int i = 0;
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    if (dataTable.Columns[j].ColumnName.Contains("pic"))
                    {
                        //        foreach (DataRow column in dataTable.Rows)
                        //{
                        //    for (int i = 0; j < 5; j++)
                        //    {
                        byte[] blob = (byte[])dataTable.Rows[i][j];
                        if (blob.Length > 10)
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
                            var complete = Path.Combine(systemPath + "\\LTCReportFolder\\Report_" + "Images", dataTable.Rows[i][2] + "_" + j + ".Jpeg");
                            img.Save(complete, ImageFormat.Jpeg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            System.Diagnostics.Process.Start("explorer.exe", systemPath + "\\LTCReportFolder");
        }
    }
}
