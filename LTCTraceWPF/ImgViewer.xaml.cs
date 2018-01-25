using Npgsql;
using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ImgViewer.xaml
    /// </summary>
    public partial class ImgViewer : Window
    {
        public ImgViewer()
        {
            InitializeComponent();
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
                foreach (DataRow column in dataTable.Rows)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        //Store binary data read from the database in a byte array
                        byte[] blob = (byte[])column[i + 6];
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
                        if (i == 0)
                            image2.Source = bi;
                        if (i == 1)
                            image3.Source = bi;
                        if (i == 2)
                            image4.Source = bi;
                        if (i == 3)
                            image5.Source = bi;
                        if (i == 4)
                            image6.Source = bi;
                        if (i == 5)
                            image7.Source = bi;
                        if (i == 6)
                            image8.Source = bi;
                        if (i == 7)
                            image9.Source = bi;
                    }
                }
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
            System.IO.Directory.CreateDirectory("D:\\Traceimages\\");
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
                foreach (DataRow column in dataTable.Rows)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        byte[] blob = (byte[])column[i + 6];
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
                        img.Save("C:\\TraceImages\\" + column[2] + "_" + i + ".Jpeg", ImageFormat.Jpeg);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Process.Start("explorer.exe", "C:\\TraceImages\\");
        }
    }
}
