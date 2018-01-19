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

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for PottingWindow.xaml
    /// </summary>
    public partial class PottingWindow : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool CameraLaunched { get; set; } = false;

        public PottingWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
        }

        private void OnKeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();

            if (e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }

            //if (Keyboard.FocusedElement == FbDmTxbx && FbDmTxbx.Text.Length > 0) //calls the validator for the field in focus
            //{
            //    FbDmValidator();
            //}
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            SaveBtn.Focus();
            CameraLaunched = true;
            var webCam = new camApp();
            webCam.Show();
        }
    }
}
