using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for PrintHousingDMCWindow.xaml
    /// </summary>
    public partial class PrintHousingDMCWindow : Window
    {
        public PrintHousingDMCWindow()
        {
            InitializeComponent();
        }

        private void ClearValue(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Foreground = Brushes.Black;

            if ((sender as TextBox).Text == "HÁZ DATAMATRIX")
            {
                (sender as TextBox).Text = String.Empty;
            }
        }

        private void SetDefaultValue(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == "")
            {
                (sender as TextBox).Text = "HÁZ DATAMATRIX";
                (sender as TextBox).Foreground = Brushes.LightBlue;
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HousingDm_KeyUp(object sender, KeyEventArgs e)
        {
            if (HousingDm.Text.Contains("P514LTC"))
            {
                PartNumber.Text = "LTC P514";
            }
            else if (HousingDm.Text.Contains("35LTC"))
            {
                PartNumber.Text = "LTC B3.5 LEVC PN";
            }
            else if (HousingDm.Text.Contains("LTC") && !HousingDm.Text.Contains("p514LTC") && !HousingDm.Text.Contains("35LTC"))
            {
                PartNumber.Text = "LTC 1E0002187AD B2.5";
            }
            else
            {
                PartNumber.Text = "";
            }
        }

        private void PrintBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!(Regex.IsMatch(HousingDm.Text, ConfigurationManager.AppSettings["HousingDmRegEx"]) && PartNumber.Text != ""))
            {
                new MessageForm("Hibás kitöltés! A ház datamatrix formátuma nem felel meg!").Show();
                return;
            }

            string s = @"^XA^MMT^PW406^LL0280^LS0^BY252,252^FT16,266^BXN,18,200,0,0,1,~^FH\^FD" + HousingDm.Text + @"^FS^FT345,274^A0B,25,26^FH\^FD" + PartNumber.Text + @"^FS^FT376,274^A0B,25,26^FH\^FD" + HousingDm.Text + @"^FS^PQ1,0,1,Y^XZ";

            //Set printername
            string printerName = String.Empty;

            if (Environment.MachineName != "STATION-8-TRACE")

                printerName = @"\\STATION-8-TRACE\ZDesigner ZT420-203dpi ZPL";
            else
                printerName = "ZDesigner ZT420-203dpi ZPL";

            RawPrinterHelper.SendStringToPrinter(printerName, s);

            HousingDm.Text = "";
            PartNumber.Text = "";
            SetDefaultValue(HousingDm, e);
        }
    }
}
