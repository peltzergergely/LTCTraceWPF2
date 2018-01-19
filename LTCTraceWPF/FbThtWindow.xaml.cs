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
    /// Interaction logic for fbtht.xaml
    /// </summary>
    public partial class fbtht : Window
    {
        public fbtht()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void MainMenuBtn_Click_(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
