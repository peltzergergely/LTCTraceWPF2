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
        private bool admin;

        public MainWindow() : this(false, "00,11,12,21,22,31,32,33,34,35,41,42,43,44,45,46,47,48,XX") { }

        public MainWindow(bool admin,string trained)
        {
            InitializeComponent();

            this.admin = admin;

            if (ConfigurationManager.AppSettings["transistordate"] == "false" || !trained.Contains("00"))
            {
                TransistorDateBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["MbHsAssy"] == "false" || !trained.Contains("11"))
            {
                MbHsAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["MbDspAssy"] == "false" || !trained.Contains("12"))
            {
                MbDspAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FbAcdcAssy"] == "false" || !trained.Contains("21"))
            {
                FbAcdcAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FbEmcAssy"] == "false" || !trained.Contains("22"))
            {
                FbEmcAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["LeakTestOne"] == "false" || !trained.Contains("31"))
            {
                LeakTestOne.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["CoolingLeakTest"] == "false" || !trained.Contains("32"))
            {
                CoolingLeakTest.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HousingFbAssy"] == "false" || !trained.Contains("33"))
            {
                HousingFbAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Potting"] == "false" || !trained.Contains("34"))
            {
                PottingBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HousingConnectorAssy"] == "false" || !trained.Contains("35"))
            {
                HousingConnectorAssyBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FinalAssyOne"] == "false" || !trained.Contains("41"))
            {
                FinalAssyOneBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HiPotTestOne"] == "false" || !trained.Contains("42"))
            {
                HiPotTestOneBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Calibration"] == "false" || !trained.Contains("43"))
            {
                CalibrationBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["FinalAssyTwo"] == "false" || !trained.Contains("44"))
            {
                FinalAssyTwoBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["LeakTestTwo"] == "false" || !trained.Contains("45"))
            {
                LeakTestTwoBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["HiPotTestTwo"] == "false" || !trained.Contains("46"))
            {
                HiPotTestTwoBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["EOL"] == "false" || !trained.Contains("47"))
            {
                EolBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Firewall"] == "false" || !trained.Contains("48"))
            {
                FirewallBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["ErrorReport"] == "false")
            {
                ErrorReportBtn.IsEnabled = false;
            }
            if (ConfigurationManager.AppSettings["Rework"] == "false" || !trained.Contains("XX"))
            {
                ReworkBtn.IsEnabled = false;
            }
            if (trained.Contains("Trainer") || admin == true)
            {
                manageUsersBtn.IsEnabled = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Application.Current.Shutdown();
            this.Close();
            if (Owner != null)
                this.Owner.Show();

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

        private void CoolingLeakTest_Click(object sender, RoutedEventArgs e)
        {
            var coolingleaktestwindow = new CoolingLeakTest();
            coolingleaktestwindow.Show();
        }

        private void manageUsersBtn_Click(object sender, RoutedEventArgs e)
        {
            var manageUsers = new ManageUsers(admin);
            manageUsers.Owner = this;
            manageUsers.Show();
            this.Hide();
        }
    }
}
