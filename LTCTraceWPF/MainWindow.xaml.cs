using System;
using System.Configuration;
using System.Windows;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (ConfigurationManager.AppSettings["transistordate"] == "false")
            {
                TransistorDateBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["MbHsAssy"] == "false")
            {
                MbHsAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["MbDspAssy"] == "false")
            {
                HousingConnectorAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FbAcdcAssy"] == "false")
            {
                FbAcdcAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FbEmcAssy"] == "false")
            {
                FbEmcAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["LeakTestOne"] == "false")
            {
                LeakTestOne.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HousingFbAssy"] == "false")
            {
                HousingFbAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Potting"] == "false")
            {
                PottingBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HousingConnectorAssy"] == "false")
            {
                HousingConnectorAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FinalAssyOne"] == "false")
            {
                FinalAssyOneBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HiPotTestOne"] == "false")
            {
                HiPotTestOneBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Calibration"] == "false")
            {
                CalibrationBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FinalAssyTwo"] == "false")
            {
                FinalAssyTwoBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["LeakTestTwo"] == "false")
            {
                LeakTestTwoBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HiPotTestTwo"] == "false")
            {
                HiPotTestTwoBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["EOL"] == "false")
            {
                EolBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Firewall"] == "false")
            {
                FirewallBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["ErrorReport"] == "false")
            {
                ErrorReportBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Rework"] == "false")
            {
                ReworkBtn.IsEnabled = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
            //this.Close();
        }

        private void FbAcdcAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var fbACDCAssy = new FbACDCAssy();
            fbACDCAssy.Show();
        }
        private void FbEmcBtn_Click(object sender, RoutedEventArgs e)
        {
            var FbEmcWindow = new FbEmcWindow();
            FbEmcWindow.Show();
        }

        private void HouseLeakTestBtn_Click(object sender, RoutedEventArgs e)
        {
            var HousingLeakTestWindow = new LeakTest1Win();
            HousingLeakTestWindow.Show();
        }

        private void HousingFbAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var PottingWindow = new HousingFbAssy();
            PottingWindow.Show();
        }

        private void HipotBtn_Click(object sender, RoutedEventArgs e)
        {
            var Hipot1Window = new Hipot1Window();
            Hipot1Window.Show();
        }

        private void HousingConnectorAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var HousingConnectorAssyWindow = new HousingConnectorAssyWindow();
            HousingConnectorAssyWindow.Show();
        }

        private void MbHsAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var MbHsAssyWindow = new MbHsAssyWindow();
            MbHsAssyWindow.Show();
        }

        private void MbDspAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var MbDspAssy = new MbDspAssy();
            MbDspAssy.Show();
        }

        private void FinalAssy1Btn_Click(object sender, RoutedEventArgs e)
        {
            var FinalAssy1Window = new FinalAssy1Window();
            FinalAssy1Window.Show();
        }

        private void CalibBtn_Click(object sender, RoutedEventArgs e)
        {
            var CalibWindow = new CalibWindow();
            CalibWindow.Show();
        }

        private void FinalAssy2Btn_Click(object sender, RoutedEventArgs e)
        {
            var FinalAssy2Window = new FinalAssy2Window();
            FinalAssy2Window.Show();
        }

        private void LeakTest2btn_Click(object sender, RoutedEventArgs e)
        {
            var LeakTest2Window = new LeakTest2Window();
            LeakTest2Window.Show();
        }

        private void FirewallBtn_Click(object sender, RoutedEventArgs e)
        {
            var FirewallWindow = new FirewallWindow();
            FirewallWindow.Show();
        }

        private void DbBtn_Click(object sender, RoutedEventArgs e)
        {
            var DbWindow = new DbWindow();
            DbWindow.Show();
        }

        private void HiPotTwoBtn_Click(object sender, RoutedEventArgs e)
        {
            var HipotTestTwo = new HipotTestTwo();
            HipotTestTwo.Show();
        }

        private void EolBtn_Click(object sender, RoutedEventArgs e)
        {
            var eolTest = new EOLTest();
            eolTest.Show();
        }

        private void TransistorDateBtn_Click(object sender, RoutedEventArgs e)
        {
            var tranzistordate = new TransistorDateWindow();
            tranzistordate.Show();
        }

        private void ReworkBtn_Click(object sender, RoutedEventArgs e)
        {
            var rework = new ReworkWindow();
            rework.Show();
        }

        private void ErrorReportBtn_Click(object sender, RoutedEventArgs e)
        {
            var errorReport = new ErrorReport();
            errorReport.Show();
        }

        private void Potting_Click(object sender, RoutedEventArgs e)
        {
            var pottingwindow = new PottingWindow();
            pottingwindow.Show();
        }
    }
}
