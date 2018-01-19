using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
using NpgsqlTypes;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ImageToDb.xaml
    /// Showing the last made image in a separate window and option to upload it to the database or discard
    /// saving the image on the hard drive then after closing this window the caller can look into 
    /// the harddrive and save the appropiaet image batch to the database
    /// this window needs the following inputs - nothing?
    /// calling this frame to confirm the save of the image
    /// image saved on the hardware
    /// </summary>
    public partial class ImageToDb : Window
    {
        public string FilePathStr { get; set; } = "";

        public string constr { get; set; } = @ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;


        //BitmapImage image = new BitmapImage();
        //image.BeginInit();
        //    image.CacheOption = BitmapCacheOption.OnLoad;
        //    image.UriSource = new Uri(FilePathStr);
        //image.EndInit();

        public ImageToDb()
        {
            ShowImage();

        }
        public ImageToDb(string filePathName)
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
            FilePathStr = filePathName;
            camApp.NumOfPics = 0 ;
            ShowImage();
        }

        private void ShowImage()
        {
            //image.Source = new BitmapImage(new Uri(FilePathStr));
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(FilePathStr);
            image.EndInit();
            img.Source = image;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void deleteImgBtn_Click(object sender, RoutedEventArgs e)
        {
            File.Delete(FilePathStr);
            this.Close();
        }
    }
}
