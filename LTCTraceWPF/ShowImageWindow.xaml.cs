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

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for ShowImageWindow.xaml
    /// </summary>
    public partial class ShowImageWindow : Window
    {
        public ShowImageWindow()
        {
            InitializeComponent();
        }

        public void SetImg(BitmapImage bi)
        {
            img.Source = bi;
            this.Show();
        }

        private void hide(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
    }
}
