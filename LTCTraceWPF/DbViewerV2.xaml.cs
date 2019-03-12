using Microsoft.Win32;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for DbViewerV2.xaml
    /// </summary>
    public partial class DbViewerV2 : Window
    {
        //Variables for animation
        private Button MovableBtn { get; set; } = null;
        private int MovableBtnPos { get; set; } = 0;
        private bool AnimationInProgress { get; set; } = false;

        //Variables for logic
        public IDictionary<string, string> workSteps = new Dictionary<string, string>();
        public IDictionary<string, string> workFlow = new Dictionary<string, string>();
        public IDictionary<string, string> filter = new Dictionary<string, string>();
        private string connstring = ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString;
        private DataSet dataSet = new DataSet();
        private DataTable dataTable = new DataTable();
        private DateTime LastStartingDate { get; set; } = DateTime.Today;
        private DateTime LastEndingDate { get; set; } = DateTime.Today;
        private string DbQueryOffset { get; set; } = "0";
        private string SqlCommandForCounter { get; set; } = String.Empty;
        private string OrderBy { get; set; } = "id";
        private string OrderByDirection { get; set; } = "DESC";
        public ShowImageWindow w = new ShowImageWindow();
        private string MainFileName { get; set; }
        private List<byte[]> Docs { get; set; }

        public DbViewerV2()
        {
            InitializeComponent();
            InitFields();

            Menus.Visibility = Visibility.Visible;
            Menus.IsEnabled = true;
            showMenuPnl.Visibility = Visibility.Hidden;
            riportOutput.Visibility = Visibility.Hidden;
            riportOutput.IsEnabled = false;
            GreetingPage.Height = 700;
        }

        private void test(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("valtozott");
        }

        private void InitFields()
        {
            // Starting and Ending date
            startDate.SelectedDate = DateTime.Today;
            endDate.SelectedDate = DateTime.Today;

            //Fill workstation combobox
            workSteps.Add("00 Transistor Dátumok", "transdate");
            workSteps.Add("11 MB HS Szerelés", "mb_hs_assy");
            workSteps.Add("12 MB DSP Szerelés", "mb_dsp_assy");
            workSteps.Add("21 FB ACDC Szerelés", "fb_acdc_assy");
            workSteps.Add("22 FB EMC Szerelés", "fb_emc_assy");
            workSteps.Add("31 Ház Leak Teszt I.", "housing_leak_test_one");
            workSteps.Add("32 Hűtőkör Leak Teszt", "cooling_leak_test");
            workSteps.Add("33 Ház FB Szerelés", "housing_fb_assy");
            workSteps.Add("34 Potting után Kapton", "potting");
            workSteps.Add("35 Ház Konnektor Szerelés", "housing_connector_assy");
            workSteps.Add("41 Végszerelés I. MB", "final_assy_one");
            workSteps.Add("41 Végszerelés I. GW", "final_assy_two");
            workSteps.Add("42 HiPot Teszt I.", "hipot_test_one");
            workSteps.Add("43 Kalibráció", "calibration");
            workSteps.Add("45 Leak Teszt II.", "housing_leak_test_two");
            workSteps.Add("46 Hipot Teszt II.", "hipot_test_two");
            workSteps.Add("47 EOL", "eol");
            workSteps.Add("48 Firewall", "firewall");
            workSteps.Add("XX Rework", "reworked_products");
            workSteps.Add("Módosított házszámok", "updated_housing_dms");
            

            workStationCbx.ItemsSource = workSteps;
            workStationCbx.DisplayMemberPath = "Key";
            workStationCbx.SelectedValuePath = "Value";
            workStationCbx.SelectedIndex = 0;

        }


        //Show the image from DB by click on cell
        private void ShowImage(object sender, SelectedCellsChangedEventArgs e)
        {
            //try
            //{
                if (resultDataGrid.Items.Count > 0 && resultDataGrid.SelectedCells.Count == 1) // there is row in datagrid and only one cell is selected
                {
                    //get the indexes
                    int column = resultDataGrid.CurrentColumn.DisplayIndex;
                    int row = resultDataGrid.Items.IndexOf(resultDataGrid.CurrentItem);

                    if (dataTable.Columns[column].ColumnName.Contains("pic") && !(dataTable.Rows[row][column] is DBNull)) //check if the focused cell is a picture
                    {
                        byte[] blob = (byte[])dataTable.Rows[row][column];
                        MemoryStream stream = new MemoryStream();
                        if (blob.Length > 10)
                        {
                            stream.Write(blob, 0, blob.Length);
                            stream.Position = 0;
                            System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            MemoryStream ms = new MemoryStream();
                            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                            ms.Seek(0, SeekOrigin.Begin);
                            bi.StreamSource = ms;
                            bi.EndInit();

                            w.SetImg(bi);
                        }
                        else
                        {
                            w.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        w.Visibility = Visibility.Hidden;
                    }
                }
            //}
            //catch (Exception)
            //{

            //}
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AnimationInProgress)
                return;

            MovableBtn = (sender as Button);
            MovableBtnPos = int.Parse((sender as Button).Margin.Right.ToString());

            // move left
            AnimationInProgress = true;
            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Tick += new EventHandler(GoToLeft);
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dTimer.Start();

            switch ((sender as Button).Name.ToString())
            {
                case "searchBtn":
                    //hide
                    fullRiportPanel.Visibility = Visibility.Hidden;
                    fullRiportPanel.IsEnabled = false;
                    riportOutput.Visibility = Visibility.Hidden;
                    riportOutput.IsEnabled = false;
                    //show
                    showSearchPanel.Visibility = Visibility.Visible;
                    showSearchPanel.IsEnabled = true;
                    SaveBtn.Visibility = Visibility.Visible;
                    SaveBtn.IsEnabled = true;
                    ListBtn.Visibility = Visibility.Visible;
                    ListBtn.IsEnabled = true;
                    resultDataGrid.Visibility = Visibility.Visible;
                    resultDataGrid.Height = 700;
                    DatagridTabs.Height = 80;
                    GreetingPage.Height = 0;
                    resultDataGrid.IsEnabled = true;
                    SearchBtnClicked();
                    //i really dont know why but it works.
                    #region ugly stuff
                    workSteps.Remove("productQuantity");
                    workSteps.Remove("finishedProducts");
                    #endregion
                    break;
                case "finishedProductsBtn":
                    //hide
                    fullRiportPanel.Visibility = Visibility.Hidden;
                    fullRiportPanel.IsEnabled = false;
                    riportOutput.Visibility = Visibility.Hidden;
                    riportOutput.IsEnabled = false;
                    //show
                    showSearchPanel.Visibility = Visibility.Visible;
                    showSearchPanel.IsEnabled = true;
                    SaveBtn.Visibility = Visibility.Visible;
                    SaveBtn.IsEnabled = true;
                    ListBtn.Visibility = Visibility.Visible;
                    ListBtn.IsEnabled = true;
                    resultDataGrid.Visibility = Visibility.Visible;
                    resultDataGrid.Height = 700;
                    DatagridTabs.Height = 80;
                    GreetingPage.Height = 0;
                    resultDataGrid.IsEnabled = true;
                    FinishedProductsBtnClicked();
                    //i really dont know why but it works.
                    #region ugly stuff
                    workSteps.Remove("productQuantity");
                    workSteps.Remove("finishedProducts");
                    #endregion
                    break;
                case "quantityBtn":
                    //hide other panel
                    fullRiportPanel.Visibility = Visibility.Hidden;
                    fullRiportPanel.IsEnabled = false;
                    riportOutput.Visibility = Visibility.Hidden;
                    riportOutput.IsEnabled = false;
                    //show used panel
                    showSearchPanel.Visibility = Visibility.Visible;
                    showSearchPanel.IsEnabled = true;
                    SaveBtn.Visibility = Visibility.Visible;
                    SaveBtn.IsEnabled = true;
                    ListBtn.Visibility = Visibility.Visible;
                    ListBtn.IsEnabled = true;
                    resultDataGrid.Visibility = Visibility.Visible;
                    resultDataGrid.Height = 700;
                    DatagridTabs.Height = 80;
                    GreetingPage.Height = 0;
                    resultDataGrid.IsEnabled = true;
                    QunatitiyBtnClicked();
                    //i really dont know why but it works.
                    #region ugly stuff
                    workSteps.Remove("productQuantity");
                    workSteps.Remove("finishedProducts");
                    #endregion
                    break;
                case "bugreportBtn":
                    //reset form
                    txtBlck.Text = "";
                    riportByTbx.Text = "";
                    Documentums.Text = "";
                    MainFileName = "";
                    Images.Children.Clear();
                    //hide other panel
                    showSearchPanel.Visibility = Visibility.Hidden;
                    showSearchPanel.IsEnabled = false;
                    SaveBtn.Visibility = Visibility.Hidden;
                    SaveBtn.IsEnabled = false;
                    ListBtn.Visibility = Visibility.Hidden;
                    ListBtn.IsEnabled = false;
                    resultDataGrid.Visibility = Visibility.Hidden;
                    resultDataGrid.Height = 0;
                    DatagridTabs.Height = 0;
                    GreetingPage.Height = 0;
                    resultDataGrid.IsEnabled = false;
                    //show used panel
                    fullRiportPanel.Visibility = Visibility.Visible;
                    fullRiportPanel.IsEnabled = true;
                    riportOutput.Visibility = Visibility.Visible;
                    riportOutput.IsEnabled = true;
                    break;
                default:
                    break;
            }



        }

        private void QunatitiyBtnClicked()
        {
            //reset form
            resultDataGrid.ItemsSource = null;
            Tabs.Children.Clear();
            startDate.SelectedDate = DateTime.Today;
            endDate.SelectedDate = DateTime.Today;


            //set item changes
            workStationCbx.SelectedIndex = 0;
            workSteps.Remove("finishedProducts");
            workSteps.Remove("productQuantity");
            workSteps.Add("productQuantity", "get_product_quantity(current_date,current_date)");
            workStationCbx.SelectedIndex = workStationCbx.Items.Count - 1;

            //set style changes
            workStationlbl.Content = "Idő Int";
            ervallumtxt.Visibility = Visibility.Visible;
            workStationlbl.HorizontalAlignment = HorizontalAlignment.Right;
            workStationlbl.FontSize = 35;
            workStationlbl.FontStyle = FontStyles.Italic;
            workStationlbl.Foreground = Brushes.Gray;
            workStationCbx.Visibility = Visibility.Hidden;
            workStationCbx.IsEnabled = false;
            //-------------
            filterLbl.Content = "Termékek megtekintése";
            filterColumn.Content = "Munkafolyamat";
            FilterByCbx.Visibility = Visibility.Hidden;
            FilterByCbx.IsEnabled = false;
            workflowCbx.Visibility = Visibility.Visible;
            workflowCbx.IsEnabled = true;
            valueLbl.Visibility = Visibility.Hidden;
            filterByTbx.Visibility = Visibility.Hidden;
            checkProductBtn.Visibility = Visibility.Visible;
            checkProductBtn.IsEnabled = true;
        }

        private void FinishedProductsBtnClicked()
        {
            //reset form
            resultDataGrid.ItemsSource = null;
            Tabs.Children.Clear();
            startDate.SelectedDate = DateTime.Today;
            endDate.SelectedDate = DateTime.Today;
            filterByTbx.Text = String.Empty;

            //set item changes
            workStationCbx.SelectedIndex = 0;
            workSteps.Remove("productQuantity");
            workSteps.Remove("finishedProducts");
            workSteps.Add("finishedProducts", "get_finished_products()");
            workStationCbx.SelectedIndex = workStationCbx.Items.Count - 1;

            //set style changes
            workStationlbl.Content = "Idő Int";
            ervallumtxt.Visibility = Visibility.Visible;
            workStationlbl.HorizontalAlignment = HorizontalAlignment.Right;
            workStationlbl.FontSize = 35;
            workStationlbl.FontStyle = FontStyles.Italic;
            workStationlbl.Foreground = Brushes.Gray;
            workStationCbx.Visibility = Visibility.Hidden;
            workStationCbx.IsEnabled = false;
            //-------------
            filterLbl.Content = "Szűrés";
            filterColumn.Content = "Oszlop";
            FilterByCbx.Visibility = Visibility.Visible;
            FilterByCbx.IsEnabled = true;
            workflowCbx.Visibility = Visibility.Hidden;
            workflowCbx.IsEnabled = false;
            valueLbl.Visibility = Visibility.Visible;
            filterByTbx.Visibility = Visibility.Visible;
            checkProductBtn.Visibility = Visibility.Hidden;
            checkProductBtn.IsEnabled = false;
        }

        private void SearchBtnClicked()
        {
            //reset form
            resultDataGrid.ItemsSource = null;
            Tabs.Children.Clear();
            workStationCbx.SelectedIndex = 0;
            startDate.SelectedDate = DateTime.Today;
            endDate.SelectedDate = DateTime.Today;
            filterByTbx.Text = String.Empty;

            //set item changes
            workSteps.Remove("productQuantity");
            workSteps.Remove("finishedProducts");
            //set style changes
            workStationlbl.Content = "Munkaállomás";
            ervallumtxt.Visibility = Visibility.Hidden;
            workStationlbl.HorizontalAlignment = HorizontalAlignment.Center;
            workStationlbl.FontSize = 25;
            workStationlbl.FontStyle = FontStyles.Normal;
            workStationlbl.Foreground = Brushes.DarkSlateGray;
            workStationCbx.Visibility = Visibility.Visible;
            workStationCbx.IsEnabled = true;
            //-------------
            filterLbl.Content = "Szűrés";
            filterColumn.Content = "Oszlop";
            FilterByCbx.Visibility = Visibility.Visible;
            FilterByCbx.IsEnabled = true;
            workflowCbx.Visibility = Visibility.Hidden;
            workflowCbx.IsEnabled = false;
            valueLbl.Visibility = Visibility.Visible;
            filterByTbx.Visibility = Visibility.Visible;
            checkProductBtn.Visibility = Visibility.Hidden;
            checkProductBtn.IsEnabled = false;
            //--------------
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            w.Close();
            this.Close();
            Application.Current.Shutdown();
        }

        #region Animation Methods
        private void GoToLeft(object sender, EventArgs e)
        {
            Button checkerButton = null;

            // fade out the unselected menus
            FadeOut:
            foreach (var menu in Menus.Children)
            {
                if ((menu as Button) != MovableBtn)
                {
                    (menu as Button).Opacity -= 0.1;
                    checkerButton = menu as Button;
                }
            }
            int actualRightMargin = int.Parse(MovableBtn.Margin.Right.ToString());
            int speed = 20;
            int distanceFromDestonation = 1600 - actualRightMargin;

            if (distanceFromDestonation > 1000)
                speed = 70;

            if (distanceFromDestonation > 500 && distanceFromDestonation <= 1000)
                speed = 50;

            if (distanceFromDestonation > 200 && distanceFromDestonation <= 500)
                speed = 25;

            if (distanceFromDestonation <= 200)
                speed = 10;

            int right = int.Parse(MovableBtn.Margin.Right.ToString()) + speed;
            MovableBtn.Margin = new Thickness(0, 0, right, 0);
            if (right >= 1600)
            {
                if (checkerButton.Opacity > 0)
                    goto FadeOut;

                (sender as DispatcherTimer).Stop();
                MovableBtn.Margin = new Thickness(0, 0, 1600, 0);

                showMenuPnl.Visibility = Visibility.Visible;
                Menus.IsEnabled = false;
                AnimationInProgress = false;
            }

        }

        private void UndoMenus(object sender, EventArgs e)
        {

            // fade in the unselected menus
            Button checkerButton = null;

            FadeIn:
            foreach (var menu in Menus.Children)
            {
                if ((menu as Button) != MovableBtn)
                {
                    (menu as Button).Opacity += 0.1;
                    checkerButton = menu as Button;
                }
            }

            int actualRightMargin = int.Parse(MovableBtn.Margin.Right.ToString());
            int speed = 20;
            int distanceFromDestonation = actualRightMargin - MovableBtnPos;

            if (distanceFromDestonation > 1000)
                speed = 70;

            if (distanceFromDestonation > 500 && distanceFromDestonation <= 1000)
                speed = 50;

            if (distanceFromDestonation > 200 && distanceFromDestonation <= 500)
                speed = 25;

            if (distanceFromDestonation <= 200)
                speed = 10;

            int right = int.Parse(MovableBtn.Margin.Right.ToString()) - speed;
            MovableBtn.Margin = new Thickness(0, 0, right, 0);
            if (right <= MovableBtnPos)
            {
                if (checkerButton.Opacity < 1)
                    goto FadeIn;
                (sender as DispatcherTimer).Stop();
                MovableBtn.Margin = new Thickness(0, 0, MovableBtnPos, 0);
                AnimationInProgress = false;
            }

        }

        private void undoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (AnimationInProgress)
                return;

            AnimationInProgress = true;

            showMenuPnl.Visibility = Visibility.Hidden;
            Menus.IsEnabled = true;

            resultDataGrid.Height = 0;
            DatagridTabs.Height = 0;
            riportOutput.Visibility = Visibility.Hidden;
            riportOutput.IsEnabled = false;
            GreetingPage.Height = 700;
            resultRowCount.Content = "";


            DispatcherTimer dTimer = new DispatcherTimer();
            dTimer.Tick += new EventHandler(UndoMenus);
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dTimer.Start();

        }
        #endregion

        #region Selection changed events
        private void workStationCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OrderByDirection = "DESC";

            if (startDate.IsEnabled == true)
                LastStartingDate = startDate.SelectedDate.Value;
            if (endDate.IsEnabled == true)
                LastEndingDate = endDate.SelectedDate.Value;

            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    DataTable Dt = new DataTable();
                    DataSet Ds = new DataSet();
                    conn.Open();

                    var adapter = new NpgsqlDataAdapter("SELECT * FROM " + workStationCbx.SelectedValue + " limit 0", conn);
                    Ds.Reset();
                    adapter.Fill(Ds);
                    Dt = Ds.Tables[0];

                    string[] columnNames = Dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();

                    OrderBy = columnNames[0];

                    FilterByCbx.Items.Clear();
                    FilterByCbx.Items.Add("");
                    foreach (var item in columnNames)
                    {
                        FilterByCbx.Items.Add(item);
                    }
                    FilterByCbx.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FilterByCbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterByCbx.Items.Count == 0)
                return;

            if (FilterByCbx.SelectedValue.ToString() == "")
            {
                startDate.SelectedDate = LastStartingDate;
                endDate.SelectedDate = LastEndingDate;
                startDate.IsEnabled = true;
                endDate.IsEnabled = true;
            }
            else
            {
                if (startDate.IsEnabled == true)
                    LastStartingDate = startDate.SelectedDate.Value;
                if (endDate.IsEnabled == true)
                    LastEndingDate = endDate.SelectedDate.Value;
                startDate.SelectedDate = new DateTime(1900, 1, 1);
                startDate.IsEnabled = false;
                endDate.SelectedDate = DateTime.Today;
                endDate.IsEnabled = false;
            }
        }

        #endregion

        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is DataGridColumnHeader columnHeader)
            {
                if (OrderByDirection == "DESC")
                    OrderByDirection = "";
                else
                    OrderByDirection = "DESC";

                OrderBy = columnHeader.Content.ToString();
                ListBtn_Click(SaveBtn, null);

            }

        }

        private void ListBtn_Click(object sender, RoutedEventArgs e)
        {
            if (checkProductBtn.Visibility.ToString() == "Visible")
            {
                if ((sender as Button).Background.ToString() == "#FFADD8E6" || (sender as Button).Name.ToString() == "SaveBtn" || (sender as Button).Name.ToString().Contains("Tab"))
                {
                    workStationCbx.SelectedIndex = workflowCbx.SelectedIndex;
                }
                else
                {
                    workSteps.Remove("productQuantity");
                    workSteps.Add("productQuantity", "get_product_quantity(current_date,current_date)");
                    workStationCbx.SelectedIndex = workStationCbx.Items.Count - 1;
                }
            }

            Int32 Tabcount = 0;
            using (new WaitCursor())
            {
                try
                {
                    using (var conn = new NpgsqlConnection(connstring))
                    {
                        conn.Open();

                        string sql = BuildSqlQueryCommand();
                        //MessageBox.Show(sql);
                        var dataAdapter = new NpgsqlDataAdapter(sql, conn);
                        dataSet.Reset();
                        dataAdapter.Fill(dataSet);
                        dataTable = dataSet.Tables[0];
                        resultDataGrid.ItemsSource = dataTable.AsDataView();

                        //Hide byte [] array text where data is null
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            if (dataTable.Columns[i].ColumnName.Contains("pic"))
                            {
                                for (int j = 0; j < dataTable.Rows.Count; j++)
                                {
                                    byte[] blob = (byte[])dataTable.Rows[j][i];
                                    MemoryStream stream = new MemoryStream();
                                    if (blob.Length > 10)
                                    {
                                        // change the "byte [] array" text to something more readable.
                                    }
                                    else
                                    {
                                        dataTable.Rows[j][i] = null;
                                    }
                                }

                            }
                        }

                        // get the query result count
                        var countcmd = new NpgsqlCommand("SELECT COUNT(*)" + SqlCommandForCounter.Substring(8, SqlCommandForCounter.Length - 8), conn);
                        Tabcount = Convert.ToInt32(countcmd.ExecuteScalar());
                        resultRowCount.Content = Tabcount;

                    }

                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.Message);
                }

                if (workStationCbx.SelectedValue.ToString() == "get_product_quantity(current_date,current_date)")
                {
                    int sum = 0;
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        sum += int.Parse(dataTable.Rows[i][2].ToString());
                    }
                    resultRowCount.Content = sum.ToString();

                    workFlow.Clear();
                    workFlow.Add("00 Transistor Dátumok", "TRANZISZTOR DÁTUM");
                    workFlow.Add("11 MB HS Szerelés", "MAINBOARD HEATSZINK SZERELÉS");
                    workFlow.Add("12 MB DSP Szerelés", "MAINBOARD DSP SZERELÉS");
                    workFlow.Add("21 FB ACDC Szerelés", "FILTERBOARD AC DC SZERELÉS");
                    workFlow.Add("22 FB EMC Szerelés", "FILTERBOARD EMC SHIELD SZERELÉS");
                    workFlow.Add("31 Ház Leak Teszt I.", "HÁZ LEAK TESZT");
                    workFlow.Add("32 Hűtőkör Leak Teszt", "COOLING LEAK TESZT");
                    workFlow.Add("33 Ház FB Szerelés", "HÁZ FILTERBOARD SZERELÉS");
                    workFlow.Add("34 Potting után Kapton", "POTTING UTÁN KAPTONOZÁS");
                    workFlow.Add("35 Ház Konnektor Szerelés", "HÁZ KONNEKTOR SZERELÉS");
                    workFlow.Add("41 Végszerelés I.", "VÉGSZERELÉS I.");
                    workFlow.Add("42 HiPot Teszt I.", "HIPOT I. GEN");
                    workFlow.Add("43 Kalibráció", "KALIBRÁCIÓ GEN");
                    workFlow.Add("44 Végszerelés II.", "VÉGSZERELÉS II.");
                    workFlow.Add("45 Leak Teszt II.", "LEAK TESZT VÉGSZERELÉS II. UTÁN");
                    workFlow.Add("46 Hipot Teszt II.", "HIPOT II.");
                    workFlow.Add("47 EOL", "EOL");
                    workFlow.Add("48 Firewall", "FIREWALL");

                    workflowCbx.ItemsSource = workFlow;
                    workflowCbx.DisplayMemberPath = "Value";
                    workflowCbx.SelectedValuePath = "Key";
                    workflowCbx.SelectedIndex = 0;
                }

                Tabs.Children.Clear();
                for (int i = 0; i <= Tabcount / 200; i++)
                {
                    Button newBtn = new Button
                    {
                        Background = Brushes.White,
                        Foreground = Brushes.DarkSlateGray,
                        BorderThickness = new Thickness(0),
                        Focusable = false
                    };
                    newBtn.Click += getOffset;
                    newBtn.Content = (i + 1).ToString();
                    newBtn.Name = "Tab" + (i + 1).ToString();
                    newBtn.Width = 34;
                    newBtn.Margin = new Thickness(1, 1, 1, 0);
                    newBtn.FontSize = 20;

                    Tabs.Children.Add(newBtn);

                    //change back and foreground color on active button
                    if (int.Parse(DbQueryOffset) == i * 200)
                    {
                        newBtn.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF008F95");
                        newBtn.Foreground = Brushes.White;
                    }
                }
                DbQueryOffset = "0";
            }
        }

        private void getOffset(object sender, RoutedEventArgs e)
        {
            DbQueryOffset = ((int.Parse((sender as Button).Content.ToString() + "00") - 100) * 2).ToString();
            ListBtn_Click(sender, e);
        }

        private string BuildSqlQueryCommand()
        {
            string start = "'" + startDate.SelectedDate.Value.Year.ToString() + "-" + startDate.SelectedDate.Value.Month.ToString() + "-" + startDate.SelectedDate.Value.Day.ToString() + "'";
            string end = "'" + endDate.SelectedDate.Value.Year.ToString() + "-" + endDate.SelectedDate.Value.Month.ToString() + "-" + endDate.SelectedDate.Value.Day.ToString() + "'";
            string Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " WHERE date(saved_on) >= " + start + " and date(saved_on) <= " + end;

            if (FilterByCbx.SelectedValue.ToString() != "")
            {
                Querycmd += " AND \"" + FilterByCbx.SelectedValue.ToString() + "\" = '" + filterByTbx.Text + "'";
            }

            // temporary solution while errorreport can be listed
            if (workStationCbx.SelectedValue.ToString() == "errorreport")
            {
                Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " order by id desc";
                //// if there are many then you can filter by date
                //Querycmd = "SELECT * FROM " + workStationCbx.SelectedValue.ToString() + " WHERE date(created_on) >= " + start + " and date(created_on) <= " + end +" order by id desc";
            }

            if (workStationCbx.SelectedValue.ToString() == "get_product_quantity(current_date,current_date)")
            {
                Querycmd = "SELECT * FROM get_product_quantity(" + start + "," + end + ")";
            }

            SqlCommandForCounter = Querycmd;
            Querycmd += " ORDER BY \"" + OrderBy + "\" " + OrderByDirection;

            return Querycmd + " OFFSET " + DbQueryOffset + " LIMIT 200";
        }

        private void checkProductBtn_Click(object sender, RoutedEventArgs e)
        {

            ListBtn_Click(sender, null);

            
        }

        //Save to Excel
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            String resultat = "";
            String result = "";

            using (var conn = new NpgsqlConnection(connstring))
            {
                conn.Open();

                for (int i = 0; i < Tabs.Children.Count; i++)
                {
                    //fill datagrid
                    string sql = BuildSqlQueryCommand();
                    var dataAdapter = new NpgsqlDataAdapter(sql, conn);
                    dataSet.Reset();
                    dataAdapter.Fill(dataSet);
                    dataTable = dataSet.Tables[0];
                    resultDataGrid.ItemsSource = dataTable.AsDataView();

                    DbQueryOffset = ((i + 1) * 200).ToString();

                    // concatenate the tabs
                    resultDataGrid.SelectAllCells();
                    if (i == 0)
                        resultDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                    else
                        resultDataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.ExcludeHeader;
                    ApplicationCommands.Copy.Execute(null, resultDataGrid);
                    resultat += (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                    result += (string)Clipboard.GetData(DataFormats.Text);
                    resultDataGrid.UnselectAllCells();

                    // set back the default result
                    if (i == Tabs.Children.Count - 1)
                    {
                        foreach (var item in Tabs.Children)
                        {
                            if ((item as Button).Foreground == Brushes.White)
                            {
                                DbQueryOffset = (int.Parse((item as Button).Content.ToString()) * 200 - 200).ToString();
                            }
                        }
                        ListBtn_Click(sender, e as RoutedEventArgs);
                    }
                }
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel |*.xls";
            //saveFileDialog.DefaultExt = "xls";
            //saveFileDialog.AddExtension = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                StreamWriter file1 = new StreamWriter(saveFileDialog.FileName);
                file1.WriteLine(result.Replace(',', ' '));
                file1.Close();
            }
        }

        //Full Riport
        #region test

        #endregion

        private void housingReportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(riportByTbx.Text, ConfigurationManager.AppSettings["HousingDmRegEx"]))
                return;

            MainFileName = riportByTbx.Text;

            string mainboard = string.Empty;
            string filterboard = string.Empty;
            txtBlck.Text = "";
            Documentums.Text = "";
            imgContainer = new StackPanel { Orientation = Orientation.Horizontal };
            Docs = new List<byte[]>();
            Images.Children.Clear();

            GetFbAndMb(ref mainboard, ref filterboard);

            txtBlck.Text += GetDatasFromDb("11 MAINBOARD HEATSZINK SZERELÉS:", "SELECT * FROM mb_hs_assy WHERE mb_dm = '" + mainboard + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("12 MAINBOARD DSP SZERELÉS:", "SELECT * FROM mb_dsp_assy WHERE mb_dm = '" + mainboard + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("21 FILTERBOARD AC DC SZERELÉS:", "SELECT * FROM fb_acdc_assy WHERE fb_dm = '" + filterboard + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("22 FILTERBOARD EMC SHIELD SZERELÉS:", "SELECT * FROM fb_emc_assy WHERE fb_dm = '" + filterboard + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("31 HÁZ LEAK TESZT:", "SELECT * FROM housing_leak_test_one WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("32 HŰTŐKÖR LEAK TESZT:", "SELECT * FROM cooling_leak_test WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("33 HÁZ FILTERBOARD SZERELÉS:", "SELECT * FROM housing_fb_assy WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("34 POTTING UTÁN KAPTONOZÁS:", "SELECT * FROM potting WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("35 HÁZ KONNEKTOR SZERELÉS:", "SELECT * FROM housing_connector_assy WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("41 VÉGSZERELÉS I. MB:", "SELECT * FROM final_assy_one WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("41 VÉGSZERELÉS I. GW:", "SELECT * FROM final_assy_two WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("42 HIPOT I. GEN:", "SELECT * FROM hipot_test_one WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("43 KALIBRÁCIÓ GEN:", "SELECT * FROM calibration WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("45 LEAK TESZT VÉGSZERELÉS II. UTÁN:", "SELECT * FROM housing_leak_test_two WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("46 HIPOT II.:", "SELECT * FROM hipot_test_two WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("47 EOL:", "SELECT * FROM eol WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("48 FIREWALL:", "SELECT * FROM firewall WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
        }

        private void GetFbAndMb(ref string mainboard, ref string filterboard)
        {
            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();

                    mainboard = new NpgsqlCommand("SELECT mb_dm FROM final_assy_one WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1", conn).ExecuteScalar().ToString();
                }
            } catch (Exception)
            {
                mainboard = "";
            }

            try
            {
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();

                    filterboard = new NpgsqlCommand("SELECT fb_dm FROM housing_fb_assy WHERE housing_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1", conn).ExecuteScalar().ToString();
                }
            }
            catch (Exception)
            {
                filterboard = "";
            }
        }

        private StackPanel imgContainer = new StackPanel{Orientation = Orientation.Horizontal};
        private string GetDatasFromDb(string WoTitle,string query)
        {
            string Data = WoTitle+Environment.NewLine;

            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                using (var conn = new NpgsqlConnection(connstring))
                {
                    conn.Open();
                    var dataAdapter = new NpgsqlDataAdapter(query, conn);
                    ds.Reset();
                    dataAdapter.Fill(ds);
                    dt = ds.Tables[0];
                }

                string[] columnNames = dt.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
                int i = 0;
                foreach (var item in columnNames)
                {
                    if (!item.Contains("pic") && !item.Contains("file"))
                        Data += "\t- " + item + ": " + dt.Rows[0][i].ToString() + Environment.NewLine;
                    else
                    {
                        if (dt.Rows[0][i].ToString().Contains("Byte") && item.Contains("file"))
                        {
                            //MessageBox.Show(dt.Rows[0][i].ToString()+" "+item);
                            Docs.Add((byte[])dt.Rows[0][i]);
                        }

                        if (item.Contains("filename"))
                        {
                            //MessageBox.Show("DOC "+WoTitle+" "+item+" "+ dt.Rows[0][i] +" "+i.ToString());
                            // its a file
                            Documentums.Text += Environment.NewLine + "- " + Convert.ToString(dt.Rows[0][i]);
                            
                        }
                        else if (item.Contains("pic"))
                        {
                            // its an image
                            BitmapImage imgFromDb = LoadImage((byte[])dt.Rows[0][i]);

                            if (imgFromDb == null) break; //break if there is no pic in byte array

                            System.Windows.Controls.Image img = new System.Windows.Controls.Image
                            {
                                Margin = new Thickness(18, 10, 18, 0),
                                Width = 200,
                                Height = 150,
                                ToolTip = WoTitle.Substring(3,WoTitle.Length-4),
                                Source = imgFromDb
                            };

                            
                                imgContainer.Children.Add(img);

                            if (imgContainer.Children.Count == 1)
                                Images.Children.Add(imgContainer);

                            if (imgContainer.Children.Count == 4)
                            imgContainer = new StackPanel { Orientation = Orientation.Horizontal };

                            //MessageBox.Show("kep"+WoTitle+" "+item+" "+ dt.Rows[0][i] +" "+i);
                        }

                        
                    }
                    i++;
                }
                Data += Environment.NewLine;
            }
            catch (Exception)
            {
                return "";
            }

            return Data;
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length < 10) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private void MbRiportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(riportByTbx.Text, ConfigurationManager.AppSettings["MbDmRegEx"]))
                return;

            MainFileName = riportByTbx.Text;

            txtBlck.Text = "";
            Documentums.Text = "";
            imgContainer = new StackPanel { Orientation = Orientation.Horizontal };
            Docs = new List<byte[]>();
            Images.Children.Clear();

            txtBlck.Text += GetDatasFromDb("11 MAINBOARD HEATSZINK SZERELÉS:", "SELECT * FROM mb_hs_assy WHERE mb_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("12 MAINBOARD DSP SZERELÉS:", "SELECT * FROM mb_dsp_assy WHERE mb_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("41 VÉGSZERELÉS I.:", "SELECT * FROM final_assy_one WHERE mb_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
        }

        private void FbRiportBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(riportByTbx.Text, ConfigurationManager.AppSettings["FbDmRegEx"]))
                return;

            MainFileName = riportByTbx.Text;

            txtBlck.Text = "";
            Documentums.Text = "";
            imgContainer = new StackPanel { Orientation = Orientation.Horizontal };
            Docs = new List<byte[]>();
            Images.Children.Clear();

            txtBlck.Text += GetDatasFromDb("21 FILTERBOARD AC DC SZERELÉS:", "SELECT * FROM fb_acdc_assy WHERE fb_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("22 FILTERBOARD EMC SHIELD SZERELÉS:", "SELECT * FROM fb_emc_assy WHERE fb_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
            txtBlck.Text += GetDatasFromDb("33 HÁZ FILTERBOARD SZERELÉS:", "SELECT * FROM housing_fb_assy WHERE fb_dm = '" + riportByTbx.Text + "' ORDER BY ID DESC LIMIT 1");
        }

        private void saveReportBtn_Click(object sender, RoutedEventArgs e)
        {
            //Create Folder
            var systemPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\LTCReportFolder\" + MainFileName + "_REPORT_" + DateTime.Now.ToString().Replace("/", String.Empty).Replace(" ", String.Empty).Replace(":", String.Empty) + @"\";
            Directory.CreateDirectory(systemPath);
            Directory.CreateDirectory(systemPath + @"Images\");
            Directory.CreateDirectory(systemPath + @"Documentums\");

            //Save text part
            File.WriteAllText(systemPath + MainFileName + ".txt", txtBlck.Text);

            //Save Docs part
            string[] FileNames = Documentums.Text.Split(new[] { Environment.NewLine },StringSplitOptions.None);
            for (int i = 1; i < FileNames.Length; i++)
            {
                //MessageBox.Show(FileNames[i].Substring(2));
                File.WriteAllBytes(systemPath + @"Documentums\" + FileNames[i].Substring(2), Docs[i-1]);
            }

            //Save img part
            int Idx = 1;
            foreach (var panel in Images.Children)
            {
                foreach (var img in (panel as StackPanel).Children)
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)(img as Image).Source));
                    using (FileStream stream = new FileStream(systemPath+@"Images\"+(Idx++) + ".png", FileMode.Create))
                    encoder.Save(stream);
                }
            }

            System.Diagnostics.Process.Start("explorer.exe", systemPath);
        }
    }
}