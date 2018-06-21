﻿using ErrorLogging;
using Npgsql;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for FirewallWindow.xaml
    /// </summary>
    public partial class FirewallWindow : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsCameraLaunched { get; set; } = false;

        public DateTime? StartedOn { get; set; } = null;

        private string printerName = "ZDesigner ZT420-203dpi ZPL";

        private string BeforeHousingDm = String.Empty;


        public string[] FilePathStr; // = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");

        public FirewallWindow()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            InitializeComponent();

            if (Environment.MachineName != "STATION-8-TRACE")
            {
                printerName = @"\\STATION-8-TRACE\ZDesigner ZT420-203dpi ZPL";
            }
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

            if (e.Key == Key.Enter && HousingDmTxbx.Text.Length > 0)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }
                e.Handled = true;
            }

            if (Keyboard.FocusedElement == SaveBtn)
            {
                SaveBtn_Click(sender, e);
            }
            DmValidator();
        }

        private void FormValidator()
        {
            string errorMsg = "";
            if (IsDmValidated == true)
            {
                if (Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg").Length > 2)
                {
                    AllFieldsValidated = true;
                }
                else errorMsg += "Legalább 3 képnek kell készülnie";
            }
            if (IsDmValidated == false)
            {
                errorMsg += "DataMátrix nem megfelelő! ";
            }
            if (IsCameraLaunched == false)
            {
                errorMsg += "Kamera nem volt elindítva! ";
            }
            if (errorMsg != "")
            {
                CallMessageForm(errorMsg);
            }
        }

        public bool RegexValidation(string dataToValidate, string datafieldName)
        {
            string rgx = ConfigurationManager.AppSettings[datafieldName];
            return (Regex.IsMatch(dataToValidate, rgx));
        }

        private void DmValidator()
        {
            if (RegexValidation(HousingDmTxbx.Text, "HousingDmRegEx") == true)
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            IsCameraLaunched = false;
            HousingDmTxbx.Text = "";
            Label1Txbx.Text = "";
            Label2Txbx.Text = "";
            HousingDmTxbx.Focus();
        }

        private void CallMessageForm(string msgToShow)
        {
            ResetForm();
            var msgWindow = new MessageForm(msgToShow);
            msgWindow.Show();
            msgWindow.Activate();
        }

        private void DbInsert(string table)
        {
            FilePathStr = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");
            int imgArrayLimit = 9;
            if (FilePathStr.Length > imgArrayLimit)
            {
                MessageBox.Show("A készített képek száma meghaladja a maximum 9 képes limitet! Töröld ki a fölösleget a TraceImages mappából!'");
                System.Diagnostics.Process.Start("explorer.exe", "C:\\TraceImages\\");
            }
            else
            {
                imgArrayLimit = FilePathStr.Length;
                try
                {
                    byte[][] imgByteArray = new byte[9][];
                    for (int i = 0; i < imgArrayLimit; i++)
                    {
                        FileStream fs = new FileStream(FilePathStr[i], FileMode.Open, FileAccess.Read);
                        imgByteArray[i] = new byte[fs.Length];
                        fs.Read(imgByteArray[i], 0, Convert.ToInt32(fs.Length));
                        fs.Close();
                    }

                    using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ltctrace.dbconnectionstring"].ConnectionString))
                    {
                        conn.Open();
                        var cmd = new NpgsqlCommand("insert into " + table + " (housing_dm, pc_name, started_on, saved_on, label_one, label_two, pic1, pic2, pic3, pic4, pic5, pic6, pic7, pic8, pic9) " +
                        "values(:housing_dm, :pc_name, :started_on, :saved_on, :label_one, :label_two, :pic1, :pic2, :pic3, :pic4, :pic5, :pic6, :pic7, :pic8, :pic9)", conn);
                        cmd.Parameters.Add(new NpgsqlParameter("housing_dm", HousingDmTxbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                        cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                        cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
                        cmd.Parameters.Add(new NpgsqlParameter("label_one", Label1Txbx.Text));
                        cmd.Parameters.Add(new NpgsqlParameter("label_two", Label2Txbx.Text));
                        //uploading the pictures
                        for (int i = 0; i < 9; i++)
                        {
                            if (i < FilePathStr.Length)
                                cmd.Parameters.Add(new NpgsqlParameter("pic" + (i + 1).ToString(), imgByteArray[i]));
                            else //making them empty
                            {
                                imgByteArray[i] = new byte[0];
                                cmd.Parameters.Add(new NpgsqlParameter("pic" + (i + 1).ToString(), imgByteArray[i]));
                            }
                        }

                        int result = cmd.ExecuteNonQuery();
                        if (result == 1)
                        {
                            FilePathStr = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");
                            System.IO.Directory.CreateDirectory("C:\\TraceImagesArchive\\" + "HOUSINGDM_" + HousingDmTxbx.Text);
                            for (int i = 0; i < FilePathStr.Length; i++)
                            {
                                File.Move(FilePathStr[i], "C:\\TraceImagesArchive\\" + "HOUSINGDM_" + HousingDmTxbx.Text + "\\" + Path.GetFileName(FilePathStr[i]));
                            }
                            Resultlbl.Text = "Adatok elmentve! " + DateTime.Now;
                            ResetForm();
                        }
                        conn.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void HousingDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (HousingDmTxbx.Text.Length > 0 && BeforeHousingDm != HousingDmTxbx.Text)
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("eol", "housing_dm", HousingDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        ErrorLog.Create("eol", "housing_dm", HousingDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a termék!", this.GetType().Name.ToString());
                    }
                }
                else
                {
                    //Precheck passed
                    //Check in calibration

                    if (new DatabaseHelper().CountRowInDB("calibration","housing_dm",HousingDmTxbx.Text) > 0)
                    {
                        //Print DMC
                        using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["ltctrace.dbconnectionstring"].ConnectionString))
                        {
                            conn.Open();
                            DateTime date = Convert.ToDateTime(new NpgsqlCommand("SELECT saved_on FROM calibration where housing_dm = '" + HousingDmTxbx.Text + "' order by saved_on desc limit 1", conn).ExecuteScalar());

                            string first = String.Empty;
                            string second = "T58";

                            //Build up the upper text
                            first = date.Year.ToString().Remove(0, 2); //YY
                            if (date.Month < 10)
                                first += "0" + date.Month.ToString(); //MM
                            else
                                first += date.Month.ToString(); //MM
                            if (date.Day < 10)
                                first += "0" + date.Day.ToString(); //DD
                            else
                                first += date.Day.ToString(); //DD

                            //Build up the bottom text
                            second += date.Year.ToString().Remove(0, 2); // YY
                            second += Math.Ceiling(Convert.ToDouble(date.DayOfYear) / 7).ToString(); // WW
                            second += ((int)date.DayOfWeek).ToString(); // D
                            second += "0" + HousingDmTxbx.Text.Substring(HousingDmTxbx.Text.Length - 4, 4); // LTC housing number

                            if (HousingDmTxbx.Text.Contains("P514LTC"))
                            {
                                //P514
                                string s = @"CT~~CD,~CC^~CT~^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR2,2~SD22^JUS^LRN^CI0^XZ^XA^MMT^PW406^LL0280^LS0^FO0,64^GFA,01024,01024,00016,:Z64:eJzt0TsKwzAMBmAFD96UC7jJNRQakit5zFT7BD1BD9LRJ8gZdILg0YNp6jiPLWsLpf8g+BASCAH889vpo3ESQFhC4NW+BJC2xeKwBemSfTKBHrK7w1Q7KLlDETZfHCD3KJ4nbhkqfb3Lx7I/6IEY1GKVbXx2M+5mrYGoGRHzvHHZNG3WYHzsA03la/Mcool0q+cTcyqKOiVXF04BI3Uk7HLv7urMwmLhJGFqfPJT38gbPd9cbQ==:426D^FO0,96^GFA,01024,01024,00016,:Z64:eJzt0bFuwyAQBuCzGG6DtcPJvAaVaP1KZGsmt8rgzXklLAY2P0EHIg9d6eYBhULs9BEqReq/fQNw/wHwn8dKZ1e0uonilE+2ekoy9piky0Ms7t1VxjykbsnntdiwUb8t51ZF5Gqzep5Gboygp2rE4rmYE9Xzu8G0N+cB6TB5AkObPcrvySeI+uaLR3FZ3NoE1eo64BeW931iQYm0mUzviVmDu5V6mTm+G/Z5dzNzAQbqfWG3vHsp87qRv0KgOv/BjfpY+mgIrfrti6mLQdT+3UcSIbO13y1h21cxs3/2S4+SHx58ci8=:EA23^FO0,160^GFA,01024,01024,00016,:Z64:eJztzzEKwjAYBeBIh271AgGv8cBAruRoF6l4DQ/iGAiYrb3CHxwcjVuHakzRmrRzxz744X3DG37GloyDYm4D96rUbuv44IfyppWtGOytN50Eess38LS64SKabvrKIRIflQDfDftg3Y1tJm7OKFzimqOgaJ1Y7MmaWiD/WbZkfdMhV19vLkQvE70GUalrZCp+vQo9q/7MeueJTxP70GUy92xkdggn2JK58wG/ZWB8:83D1^FO0,128^GFA,00768,00768,00012,:Z64:eJztz7ENAjEMBdCPUrg5JQt84TVSRGKlTMIECFZhlIxw5RVRwFzuLgUjgJs8WZbzDfzru05Pba5odQVw5rDoKyzmok3rboKJck+AzEQ8091ozuZpOKM7xNXiI+Cj1oy+Z3OTGSBHn+zzV5vXOvZcmnkajr7b3iNDFiJt2bKMzB/bLetfxT2OG3+33rFEP+8=:3407^FO0,0^GFA,03072,03072,00032,:Z64:eJzt1E1uwyAQBWAQlVClStygvkgljoarLHotH8W9gZcsEK9v8A8GJc2+9SSxbD6Ezcw4Sl1xxX+OgLye4Bh6aR1T52+dL51/dB47T52n1vUTN73nX310T9zgd9dPnKJzcR5dKo6Rp+PhjjmEsjx6jB6LZc4wzId7ueasWTHbvHDcM3z1wBzKrKiBmT6Ih+lwyZHMSobV8PLNaqubPL+WHO0e6eHsJhvcIKvcsk2uuFxj3PNnsi6us402cX/Q1bm25chIVbDRFJfP9+7JRi6z+0R/F5/2+sXNkwo2b/mFWpcv/eUW5SdwPHwxReL67HP1GzcmPmCvl/Tv5gD3Cbk/E1I9qeqfvBvdnVwqefjIp6X7h+4h+w8P3RVHuuelr5DFp4fOaqS1GtXX/BVfjHi+5xxlJvU93+qn4eLquvG1/lrqn8r66Jyds/VPmfGKr8ab/uMvos1P7V/OZI/HkumTH/3PmQs9IDTu5c1h0RfO5FkM2Td+fv8mFeKQmE9VQ5e/CHnRpQGH2S7qiiuu+OvxA2X231k=:1F51^FO320,32^GFA,02304,02304,00012,:Z64:eJzt1L1KA0EQB/BdVzyrO0uLkPgIARvRkDyMLxBJo01uwV58A19lwxWW8Q1ykCKlKym8QMg4S2Z2R8HKEFK4cPhT9+v+s7dK/bf9t2N8Tskn+JyTDT5dsrbvdsgDJlDX7Ntp6dgDACscF2irPDrH37hl6jravGySZz76CN6i9d1z8s1Zcls4/+Xvl8JynvQqSrv4isrYbnRm79P+7Tq6ZdOerybpfYcih9qtHBvBi2nswIvJnHfWAMAJ17vwobcL+llaldEJ0liJATlDl+QCrAZyB6xhDxpluHh4sDMudolD+FBi0Yuapsf/d5wwDdUNTkU2wTTU+PBl2L9bzKl/mvcg9hb2HA1p/yGqoiFjThkfaJmJzCpkyDYiW4WOAWEKOn3BB9zE0c4gfdfy7ipdvN8w0JgbFiO+YoYdeHBhleqTw2Hgy26AfQp2Ha6UrR988gi8N7TYEhqvyXNYe71dWM9nfU+XiK6mT8mv06X15GoB9fZyMdUEwLMdbBryoxuve+x63LTY2F26zWNh4fM4j2Hrymnug1ZNy0evW7xnpzY9Cg4NK/IS/UHvOELPyXjZmgVl0sPxn+SQa8nZqpRzyLxHlrWQNZK1kzX9Vmt5BlS6PvfXvgAWmxIX:FD95^BY1,3,62^FT17,271^B3N,N,,N,N^FD" + second + @"^FS^FO16,76^GB67,0,5^FS^FT126,201^A0N,28,24^FH\^FD" + second + @"^FS^FT126,171^A0N,28,24^FH\^FD" + first + @"^FS^PQ2,0,1,Y^XZ";
                                RawPrinterHelper.SendStringToPrinter(printerName, s);

                                Resultlbl.Text = "P514 címke nyomtatása megtörtént.";
                            }
                            else if (HousingDmTxbx.Text.Contains("35LTC"))
                            {
                                //3.5

                                //not implemented yet
                                Resultlbl.Text = "Nem képes a rendszer 3.5 címke nyomtatására.";
                            }
                            else
                            {
                                //2.5
                                string s = @"CT~~CD,~CC^~CT~^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR2,2~SD22^JUS^LRN^CI0^XZ^XA^MMT^PW406^LL0280^LS0^FO0,64^GFA,01024,01024,00016,:Z64:eJztkSFuxDAQRcey1GEOXWAlVzA0qLpXaW/gsIBIthRQtrhsr9AjJFpg1h5hZxVpSx22IHJquz3Dkvazp68/M18D8K/7irsaCJ0GJYASV2RxbcYIxrK18Pa67inzEBPLdvayWzyE1teJd2o6CP0ywI1OovjThzCtZ+HiMS8wzAl4Rv5O14cfHiUY5O1UfEbcSaCaK5pzno049owkmuXrnO+jCjSnJ6HsvJV8Ax2OKI2dCusaQuX4m97RNXOf0g3gcU2Tsq07PtZ8OEagvF/qgO6R+eO6+Krcn9uzAdRlOCRuls1Hu2ypr//t/xltiKD2/JYYp+2cuAcjWbjbl/6KvgG2R3Au:0CE3^FO0,160^GFA,01024,01024,00016,:Z64:eJztkL0KwjAUhVsFu0i6Zgj4Ctm8YLGvoji4Xjc3qxm61VdxdCx0cHN2bOkujgVraiT9RXBy7IGE+3FyONwYRq+u+KjLQLpMyW//O88g9Z5eNsscnWd0Hb6ifHqi2gfYJYUv3TN82JWMbpKLzxzKS4YsFQcCrPYx2YcMGFZ5TIWQQFD3K39l+jkfY9X/SMUxABJX/bjcX8d8FGrf+eQDAnZc5re4VP3cKv1J9kiKowTNlNg3XLwuee1bZ4xn4g5Dr9naVHOLB6LLZqRmq3k+LBS7rXihrknrF6U6c6PXv/UGkY1SzA==:18AC^FO0,128^GFA,01024,01024,00016,:Z64:eJzt0DEKwzAMQFEFQ7wEd80gfAaPGgy5So+gMUOhOUgP46P4CBk9BFJbcYO7l07R9vgIYQNc899hoxYH+4pdEK9GBSrW69FRf3lGHQkieENil7uDAI9hFJM3pXdb7TgOd4LQvRCPTtLBfrpDzvcj1m5nn6gxMnJrw0Syb4/9nqdU7p89TlvpaLxYx2d27gNWs23v68Ao+70TqxDNlPaEt9T6fL9aqlX8yf9f084bVVdK7Q==:3ABE^FO0,96^GFA,01536,01536,00024,:Z64:eJztkjFqwzAUhiUE0RIssnkQ1tALuJtCQnyVQi+gsUPATwiaLblOR5kOuYaKB68ePQi7L6qdC7RDh7zp8fHx8+vZhDzmMb8ZSidvGJG0p+SoIoPKJ87sFKIItRsYjCpyXw2Jczs1UYWaR9xuXMXEhQN2Lvr2VAjXgRZBHxJX1rOVNM37WtkrKbkvi8R3yM9at+ds5xy8ZUFniW+s5zx/bvx6g75Zv+SrxJ8wn6MP2ZPtSLj72P+afEnJSLw0JV/6d5ePbeP32J+E/aAFJN/O/pHCRNHPZ98BP+ltCwNznQOttfjJn/s0PbWOQjXc8z2/9XeBOXxAFZc+yFeybD49+g7qmGdLPr63bE+QfFPc++MdpHnluF0ZMbKUS/92FGG6jLg5CFkV5/zpC69ei5HCyPEQqp/zp4Cf6iBGBpohr8If/SGP+b/zDYVXmj8=:E317^FO320,64^GFA,02304,02304,00012,:Z64:eJztlL9rFEEUx984kgETdxVBI6wZwcZOyyuW7L/in3BdLEJ2g4Vgc/4BIldKLEQrQZAFi+tC/gLZYHHp7jqXM9nn25l581ZNGUgKH1d87svu+/HdNwNwadEAqCrwUwDN+ujG5zXmDdtdZ06237fMusSjyAcHS2bVwDMpQP84xmoaefRF9AcoDT05qyKb+6I/+ih8+1j41vH5+p1PwncHeZJEOFsTHmnh8YBP0ppR1cL61Yc4r75pX0v69A3zhvuF9ACx2NDni4oUsRvw6UXoVzoeAlwLqOlL7AS2LWisPJctGN4sbGk0rytiG3SN76BYed2QCWXt9ZSWnF81DQD7QwugmC0xnwVbgY7sO3JRkI6t6MzF37z8V//j+Upykq4iD+sOekupZ/6OZins5gr26E7m7QsVPHC5jP4Mfeu9VXhOIhgD3IOrGpWgORVGnDIqxHiW09Uk3m9k6B5z4b6Bj13KxDo9rJuQpnVXnwvdVwpJ9XdwFrkOJuDuAse2M5CFsgVOYNtzYnHGepL/WkC4sbLs+Zw5z3N8HPWtnT3DOh3gCevm5+yl51GuFy+YMz1Xs8AF4v7cD7ZpESHqXwuYcx5auRnnhxQWUU/3g75F+rfDqJsfh9w/rc/bMCPpGOYypHd54P794Imu/Go5P2u/u44b8bn3UteByee0Cly4s+HDTuPOUxItdzXiCTOUWEdeX8H/uKT4DQ7I0SM=:ADE8^FO0,0^GFA,03840,03840,00040,:Z64:eJztlj/Oq0YUxS9jyUgU9hLoKdiC9+MiSzCjly5NtkCJrCfWQKq3izxKy8UXl46ETM45d/Cf71OygfjahmEGfhwO987Y7B3veMd/RdWahePxGNE+qiNWanVp0COfZ23mq1l2Y0+Dj6E1pUGP4nyOFs4IYM/s2eNjaJ3SoEczz6N4uHKlq2fwslk8DF7SeVDWSh8EBuk72l0fejs/LQNoct6AHe9yu/N8UBH6WPe2xmP1rW16nvfdanZjj+2mT48L2M1yqGhG2/4ccIcJvNUcMx+Z/UECVVgAr2it+I2dHfQFqO2s4KCft73wAclDqyRvdQUv/2uAvpK3Gdy+EYbFgG04gTdov4eTA/zjYDXqvBIju0hefrXyd3Si3dj2xwjeDoOlG1hEvlDqg9LqG/ct9BVo4ThK9sIrB/KgaycueeWPi/G5dcjYRP7Wzqu537TwbwMefTRbn77yGj33SN6vl8x56F70FXd9dLOgvupbB33HZHDiuQhceOPF6Glsl12hj+m4mu7+4SL376ycG+BfhRaPTWloiz3OyyZosjKCd0BrUnpniafbJ3380DTos3SUasa208LLwaOiHSvkYORxzGvGPL+Sb53hqOb3u34KT8BcBUBeecHTHgizBqRDEmbOCypc6qtavVn/dvrZQ1+migIvm0e9WV7fZMzBF57tvXARA1ysVLl7eFkl49xEyPJCQzALdywLa1ZsJV6aEQJLVPWrNxtlIZQW8UUfa/4m3o1vthzEA7kcXnko0g9b9x/9ibm4GSBNebgZkn8fdgfGXNOBeEyZJkct//GJx1qlf3qzRSsrC7Y+6UvOGesOyaypYQsLxlf/yBmZf+IFNffIwTC++kcLL85r+GIgzRooXV2clz14LkpvNrRF0he+6CNk4a0ukAZefPB8pxJt1+L1TGoWdF2zlfzzXfPEg5LsygJskNPZ35P9afd6q2hg0kdBnSYc9nYv9fvEo5JsgonWIK1tmvQqcp8POCWF5B8NOxG1p2cnzoNe2v4OFp5mkmnHm1DT7WW+Es/1ScmRvKrzzaf5b+s8KTkcFh42nJtLvy+qFj/5t2bPLzS0ptIaNYyeetB5nO9LzxdtHEXy7nW+75b52ZUs0rR5Wj9yeDRrPuVEKgoYPCwn2940zgjnMZy1fvhUWFHaXocnriJVSr9MK694mlrL6xPvab30Jdb1xaTX/xUUXVqcPRr+MxCPq4+en0unz4xa7D02fb9c8jUwGFNz9fhL8TXyu7xk1r/F4//LO97xv4t/AA4EHZ0=:1E2E^BY1,3,62^FT17,271^B3N,N,,N,N^FD" + second + @"^FS^FT125,171^A0N,28,24^FH\^FD" + first + @"^FS^FT125,202^A0N,28,24^FH\^FD" + second + @"^FS^PQ2,0,1,Y^XZ";
                                RawPrinterHelper.SendStringToPrinter(printerName, s);
                                Resultlbl.Text = "2.5 címke nyomtatása megtörtént.";
                            }
                        }
                    }
                    else
                    {
                        // No data in calibration
                        Resultlbl.Text = "Manuálisan kell címkét nyomtatni! Nem találhatóak adatok kalibráción.";
                    }
                }
                StartedOn = DateTime.Now;
            }
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            SaveBtn.Focus();
            var webCam = new camApp();
            webCam.Show();
            IsCameraLaunched = true;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            FormValidator();
            if (AllFieldsValidated)
            {
                DbInsert("firewall");
            }
        }

        private void ManualPrintBtn_Click(object sender, RoutedEventArgs e)
        {
            new PrintDMCWindow().Show();
        }

        private void HousingDmTxbx_GotFocus(object sender, RoutedEventArgs e)
        {
            BeforeHousingDm = HousingDmTxbx.Text;
        }
    }
}
