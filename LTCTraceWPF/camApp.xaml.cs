﻿using System;
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

using Microsoft.Expression.Encoder.Devices;
using System.Collections.ObjectModel;
using System.IO;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for camApp.xaml
    /// </summary>
    public partial class camApp : Window
    {
        public Collection<EncoderDevice> VideoDevices { get; set; }

        public static int NumOfPics { get; set; } = 0;

        public camApp()
        {
            InitializeComponent();
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            this.DataContext = this;
            VideoDevices = EncoderDevices.FindDevices(EncoderDeviceType.Video);

            CheckIfImgFolderEmpty();

            VidDevices.SelectedItem = "C922 Pro Stream Webcam";
            VidDevices.SelectedIndex = 0;
            StartCapture();

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void CheckIfImgFolderEmpty()
        {
            if (Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg").Length > 0)
            {
                var msgToShow = "A képek mappa nem üres, töröld a felesleges képeket!";
                var msgWindow = new MessageForm(msgToShow);
                msgWindow.Show();
                msgWindow.Activate();
                System.Diagnostics.Process.Start("explorer.exe", @"c:\TraceImages\");
            }
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();
        }

        private void StartCapture()
        {
            try
            {
                // Display webcam video
                WebcamViewer.StartPreview();
            }
            catch (Microsoft.Expression.Encoder.SystemErrorException)
            {
                MessageBox.Show("Device is in use by another application");
            }
        }

        private void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Display webcam video
                //WebcamViewer.StartPreview();
            }
            catch (Microsoft.Expression.Encoder.SystemErrorException)
            {
                MessageBox.Show("Device is in use by another application");
            }
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the display of webcam video.
            WebcamViewer.StopPreview();
        }

        public string FilePath { get; set; }

        private void TakeSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            //Take snapshot of webcam video.
            WebcamViewer.TakeSnapshot();
            FilePath = (WebcamViewer.TakeSnapshot());
            var ImageToDb = new ImageToDb(FilePath);
            ImageToDb.Show();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", @"c:\TraceImages\");
        }
    }
}
                                              