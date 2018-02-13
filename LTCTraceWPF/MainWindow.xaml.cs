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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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

        private void PottingBtn_Click(object sender, RoutedEventArgs e)
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

        private void GwThtBtn_Click(object sender, RoutedEventArgs e)
        {
            var GwThtWindow = new GwThtWindow();
            GwThtWindow.Show();
        }

        private void HsPreAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var HsPreAssyWindow = new HsPreAssyWindowxaml();
            HsPreAssyWindow.Show();
        }

        private void MbHsAssyBtn_Click(object sender, RoutedEventArgs e)
        {
            var MbHsAssyWindow = new MbHsAssyWindow();
            MbHsAssyWindow.Show();
        }

        private void MbThtBtn_Click(object sender, RoutedEventArgs e)
        {
            var MbDspAssy = new MbDspAssy();
            MbDspAssy.Show();
        }

        private void MbReworkBtn_Click(object sender, RoutedEventArgs e)
        {
            var MbReworkWindow = new MbReworkWindow();
            MbReworkWindow.Show();
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

        private void imgViewer_Click(object sender, RoutedEventArgs e)
        {
            var ImgViewer = new ImgViewer();
            ImgViewer.Show();
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
    }
}
