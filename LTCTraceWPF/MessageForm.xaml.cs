using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for MessageForm.xaml
    /// </summary>
    public partial class MessageForm : Window
    {
        private bool beep = true;

        public MessageForm(string msgToDisplay)
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
            this.Focus();
            OkBtn.Focus();
            msgToShow.Content = msgToDisplay;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            ErrorSound();
        }

        private void ErrorSound()
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (beep)
                {
                    Console.Beep(5000, 500);

                }
            }).Start();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            beep = false;
            this.Close();
        }
    }
}
