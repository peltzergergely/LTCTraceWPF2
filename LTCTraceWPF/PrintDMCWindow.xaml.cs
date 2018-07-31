using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
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
    /// Interaction logic for PrintDMCWindow.xaml
    /// </summary>
    public partial class PrintDMCWindow : Window
    {
        private string printerName = "ZDesigner ZT420-203dpi ZPL";

        public PrintDMCWindow()
        {
            InitializeComponent();

            //Set printername 
            if (Environment.MachineName != "STATION-8-TRACE")
            {
                printerName = @"\\STATION-8-TRACE\ZDesigner ZT420-203dpi ZPL";
            }

            //Set starting focus
            HousingTbx.Focus();
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TwoFivePrintBtn_Click(object sender, RoutedEventArgs e)
        {
            string s = @"CT~~CD,~CC^~CT~^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR2,2~SD22^JUS^LRN^CI0^XZ^XA^MMT^PW406^LL0280^LS0^FO0,64^GFA,01024,01024,00016,:Z64:eJztkSFuxDAQRcey1GEOXWAlVzA0qLpXaW/gsIBIthRQtrhsr9AjJFpg1h5hZxVpSx22IHJquz3Dkvazp68/M18D8K/7irsaCJ0GJYASV2RxbcYIxrK18Pa67inzEBPLdvayWzyE1teJd2o6CP0ywI1OovjThzCtZ+HiMS8wzAl4Rv5O14cfHiUY5O1UfEbcSaCaK5pzno049owkmuXrnO+jCjSnJ6HsvJV8Ax2OKI2dCusaQuX4m97RNXOf0g3gcU2Tsq07PtZ8OEagvF/qgO6R+eO6+Krcn9uzAdRlOCRuls1Hu2ypr//t/xltiKD2/JYYp+2cuAcjWbjbl/6KvgG2R3Au:0CE3^FO0,160^GFA,01024,01024,00016,:Z64:eJztkL0KwjAUhVsFu0i6Zgj4Ctm8YLGvoji4Xjc3qxm61VdxdCx0cHN2bOkujgVraiT9RXBy7IGE+3FyONwYRq+u+KjLQLpMyW//O88g9Z5eNsscnWd0Hb6ifHqi2gfYJYUv3TN82JWMbpKLzxzKS4YsFQcCrPYx2YcMGFZ5TIWQQFD3K39l+jkfY9X/SMUxABJX/bjcX8d8FGrf+eQDAnZc5re4VP3cKv1J9kiKowTNlNg3XLwuee1bZ4xn4g5Dr9naVHOLB6LLZqRmq3k+LBS7rXihrknrF6U6c6PXv/UGkY1SzA==:18AC^FO0,128^GFA,01024,01024,00016,:Z64:eJzt0DEKwzAMQFEFQ7wEd80gfAaPGgy5So+gMUOhOUgP46P4CBk9BFJbcYO7l07R9vgIYQNc899hoxYH+4pdEK9GBSrW69FRf3lGHQkieENil7uDAI9hFJM3pXdb7TgOd4LQvRCPTtLBfrpDzvcj1m5nn6gxMnJrw0Syb4/9nqdU7p89TlvpaLxYx2d27gNWs23v68Ao+70TqxDNlPaEt9T6fL9aqlX8yf9f084bVVdK7Q==:3ABE^FO0,96^GFA,01536,01536,00024,:Z64:eJztkjFqwzAUhiUE0RIssnkQ1tALuJtCQnyVQi+gsUPATwiaLblOR5kOuYaKB68ePQi7L6qdC7RDh7zp8fHx8+vZhDzmMb8ZSidvGJG0p+SoIoPKJ87sFKIItRsYjCpyXw2Jczs1UYWaR9xuXMXEhQN2Lvr2VAjXgRZBHxJX1rOVNM37WtkrKbkvi8R3yM9at+ds5xy8ZUFniW+s5zx/bvx6g75Zv+SrxJ8wn6MP2ZPtSLj72P+afEnJSLw0JV/6d5ePbeP32J+E/aAFJN/O/pHCRNHPZ98BP+ltCwNznQOttfjJn/s0PbWOQjXc8z2/9XeBOXxAFZc+yFeybD49+g7qmGdLPr63bE+QfFPc++MdpHnluF0ZMbKUS/92FGG6jLg5CFkV5/zpC69ei5HCyPEQqp/zp4Cf6iBGBpohr8If/SGP+b/zDYVXmj8=:E317^FO320,64^GFA,02304,02304,00012,:Z64:eJztlL9rFEEUx984kgETdxVBI6wZwcZOyyuW7L/in3BdLEJ2g4Vgc/4BIldKLEQrQZAFi+tC/gLZYHHp7jqXM9nn25l581ZNGUgKH1d87svu+/HdNwNwadEAqCrwUwDN+ujG5zXmDdtdZ06237fMusSjyAcHS2bVwDMpQP84xmoaefRF9AcoDT05qyKb+6I/+ih8+1j41vH5+p1PwncHeZJEOFsTHmnh8YBP0ppR1cL61Yc4r75pX0v69A3zhvuF9ACx2NDni4oUsRvw6UXoVzoeAlwLqOlL7AS2LWisPJctGN4sbGk0rytiG3SN76BYed2QCWXt9ZSWnF81DQD7QwugmC0xnwVbgY7sO3JRkI6t6MzF37z8V//j+Upykq4iD+sOekupZ/6OZins5gr26E7m7QsVPHC5jP4Mfeu9VXhOIhgD3IOrGpWgORVGnDIqxHiW09Uk3m9k6B5z4b6Bj13KxDo9rJuQpnVXnwvdVwpJ9XdwFrkOJuDuAse2M5CFsgVOYNtzYnHGepL/WkC4sbLs+Zw5z3N8HPWtnT3DOh3gCevm5+yl51GuFy+YMz1Xs8AF4v7cD7ZpESHqXwuYcx5auRnnhxQWUU/3g75F+rfDqJsfh9w/rc/bMCPpGOYypHd54P794Imu/Go5P2u/u44b8bn3UteByee0Cly4s+HDTuPOUxItdzXiCTOUWEdeX8H/uKT4DQ7I0SM=:ADE8^FO0,0^GFA,03840,03840,00040,:Z64:eJztlj/Oq0YUxS9jyUgU9hLoKdiC9+MiSzCjly5NtkCJrCfWQKq3izxKy8UXl46ETM45d/Cf71OygfjahmEGfhwO987Y7B3veMd/RdWahePxGNE+qiNWanVp0COfZ23mq1l2Y0+Dj6E1pUGP4nyOFs4IYM/s2eNjaJ3SoEczz6N4uHKlq2fwslk8DF7SeVDWSh8EBuk72l0fejs/LQNoct6AHe9yu/N8UBH6WPe2xmP1rW16nvfdanZjj+2mT48L2M1yqGhG2/4ccIcJvNUcMx+Z/UECVVgAr2it+I2dHfQFqO2s4KCft73wAclDqyRvdQUv/2uAvpK3Gdy+EYbFgG04gTdov4eTA/zjYDXqvBIju0hefrXyd3Si3dj2xwjeDoOlG1hEvlDqg9LqG/ct9BVo4ThK9sIrB/KgaycueeWPi/G5dcjYRP7Wzqu537TwbwMefTRbn77yGj33SN6vl8x56F70FXd9dLOgvupbB33HZHDiuQhceOPF6Glsl12hj+m4mu7+4SL376ycG+BfhRaPTWloiz3OyyZosjKCd0BrUnpniafbJ3380DTos3SUasa208LLwaOiHSvkYORxzGvGPL+Sb53hqOb3u34KT8BcBUBeecHTHgizBqRDEmbOCypc6qtavVn/dvrZQ1+migIvm0e9WV7fZMzBF57tvXARA1ysVLl7eFkl49xEyPJCQzALdywLa1ZsJV6aEQJLVPWrNxtlIZQW8UUfa/4m3o1vthzEA7kcXnko0g9b9x/9ibm4GSBNebgZkn8fdgfGXNOBeEyZJkct//GJx1qlf3qzRSsrC7Y+6UvOGesOyaypYQsLxlf/yBmZf+IFNffIwTC++kcLL85r+GIgzRooXV2clz14LkpvNrRF0he+6CNk4a0ukAZefPB8pxJt1+L1TGoWdF2zlfzzXfPEg5LsygJskNPZ35P9afd6q2hg0kdBnSYc9nYv9fvEo5JsgonWIK1tmvQqcp8POCWF5B8NOxG1p2cnzoNe2v4OFp5mkmnHm1DT7WW+Es/1ScmRvKrzzaf5b+s8KTkcFh42nJtLvy+qFj/5t2bPLzS0ptIaNYyeetB5nO9LzxdtHEXy7nW+75b52ZUs0rR5Wj9yeDRrPuVEKgoYPCwn2940zgjnMZy1fvhUWFHaXocnriJVSr9MK694mlrL6xPvab30Jdb1xaTX/xUUXVqcPRr+MxCPq4+en0unz4xa7D02fb9c8jUwGFNz9fhL8TXyu7xk1r/F4//LO97xv4t/AA4EHZ0=:1E2E^BY1,3,62^FT17,271^B3N,N,,N,N^FD" + TwoFiveBotTxt.Text + @"^FS^FT125,171^A0N,28,24^FH\^FD" + TwoFiveTopTxt.Text + @"^FS^FT125,202^A0N,28,24^FH\^FD" + TwoFiveBotTxt.Text+@"^FS^PQ2,0,1,Y^XZ";
            RawPrinterHelper.SendStringToPrinter(printerName, s);
        }

        private void PFOFPrintBtn_Click(object sender, RoutedEventArgs e)
        {
            string s = @"CT~~CD,~CC^~CT~^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR2,2~SD22^JUS^LRN^CI0^XZ^XA^MMT^PW406^LL0280^LS0^FO0,64^GFA,01024,01024,00016,:Z64:eJzt0TsKwzAMBmAFD96UC7jJNRQakit5zFT7BD1BD9LRJ8gZdILg0YNp6jiPLWsLpf8g+BASCAH889vpo3ESQFhC4NW+BJC2xeKwBemSfTKBHrK7w1Q7KLlDETZfHCD3KJ4nbhkqfb3Lx7I/6IEY1GKVbXx2M+5mrYGoGRHzvHHZNG3WYHzsA03la/Mcool0q+cTcyqKOiVXF04BI3Uk7HLv7urMwmLhJGFqfPJT38gbPd9cbQ==:426D^FO0,96^GFA,01024,01024,00016,:Z64:eJzt0bFuwyAQBuCzGG6DtcPJvAaVaP1KZGsmt8rgzXklLAY2P0EHIg9d6eYBhULs9BEqReq/fQNw/wHwn8dKZ1e0uonilE+2ekoy9piky0Ms7t1VxjykbsnntdiwUb8t51ZF5Gqzep5Gboygp2rE4rmYE9Xzu8G0N+cB6TB5AkObPcrvySeI+uaLR3FZ3NoE1eo64BeW931iQYm0mUzviVmDu5V6mTm+G/Z5dzNzAQbqfWG3vHsp87qRv0KgOv/BjfpY+mgIrfrti6mLQdT+3UcSIbO13y1h21cxs3/2S4+SHx58ci8=:EA23^FO0,160^GFA,01024,01024,00016,:Z64:eJztzzEKwjAYBeBIh271AgGv8cBAruRoF6l4DQ/iGAiYrb3CHxwcjVuHakzRmrRzxz744X3DG37GloyDYm4D96rUbuv44IfyppWtGOytN50Eess38LS64SKabvrKIRIflQDfDftg3Y1tJm7OKFzimqOgaJ1Y7MmaWiD/WbZkfdMhV19vLkQvE70GUalrZCp+vQo9q/7MeueJTxP70GUy92xkdggn2JK58wG/ZWB8:83D1^FO0,128^GFA,00768,00768,00012,:Z64:eJztz7ENAjEMBdCPUrg5JQt84TVSRGKlTMIECFZhlIxw5RVRwFzuLgUjgJs8WZbzDfzru05Pba5odQVw5rDoKyzmok3rboKJck+AzEQ8091ozuZpOKM7xNXiI+Cj1oy+Z3OTGSBHn+zzV5vXOvZcmnkajr7b3iNDFiJt2bKMzB/bLetfxT2OG3+33rFEP+8=:3407^FO0,0^GFA,03072,03072,00032,:Z64:eJzt1E1uwyAQBWAQlVClStygvkgljoarLHotH8W9gZcsEK9v8A8GJc2+9SSxbD6Ezcw4Sl1xxX+OgLye4Bh6aR1T52+dL51/dB47T52n1vUTN73nX310T9zgd9dPnKJzcR5dKo6Rp+PhjjmEsjx6jB6LZc4wzId7ueasWTHbvHDcM3z1wBzKrKiBmT6Ih+lwyZHMSobV8PLNaqubPL+WHO0e6eHsJhvcIKvcsk2uuFxj3PNnsi6us402cX/Q1bm25chIVbDRFJfP9+7JRi6z+0R/F5/2+sXNkwo2b/mFWpcv/eUW5SdwPHwxReL67HP1GzcmPmCvl/Tv5gD3Cbk/E1I9qeqfvBvdnVwqefjIp6X7h+4h+w8P3RVHuuelr5DFp4fOaqS1GtXX/BVfjHi+5xxlJvU93+qn4eLquvG1/lrqn8r66Jyds/VPmfGKr8ab/uMvos1P7V/OZI/HkumTH/3PmQs9IDTu5c1h0RfO5FkM2Td+fv8mFeKQmE9VQ5e/CHnRpQGH2S7qiiuu+OvxA2X231k=:1F51^FO320,32^GFA,02304,02304,00012,:Z64:eJzt1L1KA0EQB/BdVzyrO0uLkPgIARvRkDyMLxBJo01uwV58A19lwxWW8Q1ykCKlKym8QMg4S2Z2R8HKEFK4cPhT9+v+s7dK/bf9t2N8Tskn+JyTDT5dsrbvdsgDJlDX7Ntp6dgDACscF2irPDrH37hl6jravGySZz76CN6i9d1z8s1Zcls4/+Xvl8JynvQqSrv4isrYbnRm79P+7Tq6ZdOerybpfYcih9qtHBvBi2nswIvJnHfWAMAJ17vwobcL+llaldEJ0liJATlDl+QCrAZyB6xhDxpluHh4sDMudolD+FBi0Yuapsf/d5wwDdUNTkU2wTTU+PBl2L9bzKl/mvcg9hb2HA1p/yGqoiFjThkfaJmJzCpkyDYiW4WOAWEKOn3BB9zE0c4gfdfy7ipdvN8w0JgbFiO+YoYdeHBhleqTw2Hgy26AfQp2Ha6UrR988gi8N7TYEhqvyXNYe71dWM9nfU+XiK6mT8mv06X15GoB9fZyMdUEwLMdbBryoxuve+x63LTY2F26zWNh4fM4j2Hrymnug1ZNy0evW7xnpzY9Cg4NK/IS/UHvOELPyXjZmgVl0sPxn+SQa8nZqpRzyLxHlrWQNZK1kzX9Vmt5BlS6PvfXvgAWmxIX:FD95^BY1,3,62^FT17,271^B3N,N,,N,N^FD"+ PFOFBotTxt.Text + @"^FS^FO16,76^GB67,0,5^FS^FT126,201^A0N,28,24^FH\^FD"+ PFOFBotTxt.Text + @"^FS^FT126,171^A0N,28,24^FH\^FD"+PFOFTopTxt.Text+@"^FS^PQ2,0,1,Y^XZ";
            RawPrinterHelper.SendStringToPrinter(printerName, s);
        }

        private void ClearValue(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Foreground = Brushes.Black;
            
            if ((sender as TextBox).Text == "YYMMDD" || (sender as TextBox).Text == "T58YYWWDLTCID" || (sender as TextBox).Text == "B/VV/C")
            {
                (sender as TextBox).Text = String.Empty;
            }
        }

        private void SetDefaultValue(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == "")
            {
                if ((sender as TextBox).Width == 150)
                    (sender as TextBox).Text = "YYMMDD";
                else if ((sender as TextBox).Width == 102)
                    (sender as TextBox).Text = "B/VV/C";
                else
                    (sender as TextBox).Text = "T58YYWWDLTCID";

                (sender as TextBox).Foreground = Brushes.LightBlue;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool foundDataOnCalibration = false;

            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["LTCTrace.DBConnectionString"].ConnectionString))
                {
                    conn.Open();

                    if (new DatabaseHelper().CountRowInDB("calibration","housing_dm",HousingTbx.Text) > 0)
                    {
                        DateTime date = Convert.ToDateTime(new NpgsqlCommand("SELECT saved_on FROM calibration where housing_dm = '" + HousingTbx.Text + "' order by saved_on desc limit 1", conn).ExecuteScalar());

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
                        second += "0" + HousingTbx.Text.Substring(HousingTbx.Text.Length - 4, 4); // LTC housing number

                        if (HousingTbx.Text.Contains("P514LTC"))
                        {
                            ClearValue(PFOFTopTxt, e);
                            ClearValue(PFOFBotTxt, e);
                            PFOFTopTxt.Text = first;
                            PFOFBotTxt.Text = second;
                        }
                        else if (HousingTbx.Text.Contains("35LTC"))
                        {
                            ClearValue(ThreeFiveTopTxt, e);
                            ClearValue(ThreeFiveBotTxt, e);
                            ThreeFiveTopTxt.Text = first;
                            ThreeFiveBotTxt.Text = second;
                        }
                        else
                        {
                            ClearValue(TwoFiveTopTxt, e);
                            ClearValue(TwoFiveBotTxt, e);
                            TwoFiveTopTxt.Text = first;
                            TwoFiveBotTxt.Text = second;
                        }

                        foundDataOnCalibration = true;
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            if (foundDataOnCalibration == false && HousingTbx.Text.Length >= 7)
            {
                string first = String.Empty;
                string second = "T58";

                //Build up the upper text
                int dayOfCalib = 0;
                if(int.TryParse(calibDateTbx.Text,out dayOfCalib))
                {
                    DateTime date = new DateTime(DateTime.Now.Year, 1, 1).AddDays(dayOfCalib-1);
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
                    second += "0" + HousingTbx.Text.Substring(HousingTbx.Text.Length - 4, 4); // LTC housing number

                    if (HousingTbx.Text.Contains("P514LTC"))
                    {
                        ClearValue(PFOFTopTxt, e);
                        ClearValue(PFOFBotTxt, e);
                        PFOFTopTxt.Text = first;
                        PFOFBotTxt.Text = second;
                    }
                    else if (HousingTbx.Text.Contains("35LTC"))
                    {
                        ClearValue(ThreeFiveTopTxt, e);
                        ClearValue(ThreeFiveBotTxt, e);
                        ThreeFiveTopTxt.Text = first;
                        ThreeFiveBotTxt.Text = second;
                    }
                    else
                    {
                        ClearValue(TwoFiveTopTxt, e);
                        ClearValue(TwoFiveBotTxt, e);
                        TwoFiveTopTxt.Text = first;
                        TwoFiveBotTxt.Text = second;
                    }
                }
            }
        }

        private void HousingKeyUpEvent(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && HousingTbx.IsFocused == true)
                calibDateTbx.Focus();

            if (e.Key == Key.Enter && calibDateTbx.IsFocused == true)
                Button_Click(sender, e);
        }

        private void ThreeFivePrintBtn_Click(object sender, RoutedEventArgs e)
        {
            string s = @"CT~~CD,~CC^~CT~^XA~TA000~JSN^LT0^MNW^MTT^PON^PMN^LH0,0^JMA^PR2,2~SD22^JUS^LRN^CI0^XZ^XA^MMT^PW406^LL0280^LS0^FO0,64^GFA,01024,01024,00016,:Z64:eJztkTFqxDAQRccIPM1itymEdYUtVYT4KoFAapk0LgQSbLFNyF4gh5FJoS7JDTJmL6DtXBgpkvcO2yS/EDweYv5IAP+5bZjtgJiVbOlpydySwVW4iKuZ143TcRUUcenPMTMfzp7fXzyCwlPmu/301sj50MCe1ZufPhtV+Tp7LANUZZs9YEPPUF/Z8ReLbTCu+CoP573r2sW4prBDpw1xrgfqSj9qQY70ADvIR7kvYBwVAoegC8sOgnxkNvvSH7SuSCm0HIZQtByZk0+H1x3NVPrJgFYO/j0v4rb+lDn3l+rDZhaX5KOYEy7Gp+v+X7HN7xH6Y2Gc0k9E0NW3wHSjP/o7+QVw+20N:E78C^FO0,160^GFA,01024,01024,00016,:Z64:eJztkL0KwjAUhVsFu0i6Zgj4Ctm8YLGvoji4Xjc3qxm61VdxdCx0cHN2bOkujgVraiT9RXBy7IGE+3FyONwYRq+u+KjLQLpMyW//O88g9Z5eNsscnWd0Hb6ifHqi2gfYJYUv3TN82JWMbpKLzxzKS4YsFQcCrPYx2YcMGFZ5TIWQQFD3K39l+jkfY9X/SMUxABJX/bjcX8d8FGrf+eQDAnZc5re4VP3cKv1J9kiKowTNlNg3XLwuee1bZ4xn4g5Dr9naVHOLB6LLZqRmq3k+LBS7rXihrknrF6U6c6PXv/UGkY1SzA==:18AC^FO0,128^GFA,01024,01024,00016,:Z64:eJzt0DEKwzAMQFEFQ7wEd80gfAaPGgy5So+gMUOhOUgP46P4CBk9BFJbcYO7l07R9vgIYQNc899hoxYH+4pdEK9GBSrW69FRf3lGHQkieENil7uDAI9hFJM3pXdb7TgOd4LQvRCPTtLBfrpDzvcj1m5nn6gxMnJrw0Syb4/9nqdU7p89TlvpaLxYx2d27gNWs23v68Ao+70TqxDNlPaEt9T6fL9aqlX8yf9f084bVVdK7Q==:3ABE^FO0,96^GFA,01536,01536,00024,:Z64:eJztkjFOwzAYhZ9lCS+VvWYwzRU6RqhSjsOaDQZEjSKRjXIDrpKIIdcw6pANjBgIUhTzO07gADB06Bs/fXp6v2XglFP+ktxxMI+C+RZ2C+W0iHxQlo1wzL8Z640a18IEngYefdb4OvDJz6R23KDgRtwdylqqNPKNTApGnNWctaxeiXTxs2vye+LlHkaqp2rqT1bkPgafNRVAfhv9NSwfQz8vHwDqb+edbOLMN89nwT+fec8t3xva/9KRr0bdmdl3EJPfSuKD7uY9KCAM9aMLvNefcb/AZvJrXIZ+6KvoK2Tk0368StoPncf9IslE9G8F3QudLvdmvAr7idP7kD+/jy74/eTf8PLXz0flDiP8l7db7kO/+nn/ZsDunTh82KOWe43tsfvwNsMFyBf/9kdOOdZ8AwMHgJg=:6D45^FO320,64^GFA,02304,02304,00012,:Z64:eJztlL9rFEEUx984kgETdxVBI6wZwcZOyyuW7L/in3BdLEJ2g4Vgc/4BIldKLEQrQZAFi+tC/gLZYHHp7jqXM9nn25l581ZNGUgKH1d87svu+/HdNwNwadEAqCrwUwDN+ujG5zXmDdtdZ06237fMusSjyAcHS2bVwDMpQP84xmoaefRF9AcoDT05qyKb+6I/+ih8+1j41vH5+p1PwncHeZJEOFsTHmnh8YBP0ppR1cL61Yc4r75pX0v69A3zhvuF9ACx2NDni4oUsRvw6UXoVzoeAlwLqOlL7AS2LWisPJctGN4sbGk0rytiG3SN76BYed2QCWXt9ZSWnF81DQD7QwugmC0xnwVbgY7sO3JRkI6t6MzF37z8V//j+Upykq4iD+sOekupZ/6OZins5gr26E7m7QsVPHC5jP4Mfeu9VXhOIhgD3IOrGpWgORVGnDIqxHiW09Uk3m9k6B5z4b6Bj13KxDo9rJuQpnVXnwvdVwpJ9XdwFrkOJuDuAse2M5CFsgVOYNtzYnHGepL/WkC4sbLs+Zw5z3N8HPWtnT3DOh3gCevm5+yl51GuFy+YMz1Xs8AF4v7cD7ZpESHqXwuYcx5auRnnhxQWUU/3g75F+rfDqJsfh9w/rc/bMCPpGOYypHd54P794Imu/Go5P2u/u44b8bn3UteByee0Cly4s+HDTuPOUxItdzXiCTOUWEdeX8H/uKT4DQ7I0SM=:ADE8^FO0,0^GFA,03840,03840,00040,:Z64:eJztlj9r21AUxe9DPDQFTRqNPoqcIbsDftDB+iqRQin9WlpKVxVs6JCAto5V6RBDjdRz7n1K5NAGOnXxw36R9KRfjs798yxyGZfxf0eOrwsh6PGaJxu/wSSYbVVHMo2cpumEk7rFlB7TQewjqV5WxOHQiDtg4CTwPt/5TuzD1XhfPU298ibcg0ORrAcpOypvmvQSESFsVF+ANn7Fr72K83Z9E+VRmfL4+PDMO5E368bLVetqJ+4dDjb6FVmRt9opUlc50lGyUZKjSHkUp7yiBakYycNSNul9KoJuid+Ku1XLyMu3XMp1lSOjqoa8BDJ/4a+U5JXKK/Gy9AEPYw6Ng1VuL+5Lx0sNnMsD/aOf6inEAGA8EH5oUBqQ6pK8uhFlWkTzNfU5KLrd0lEqC7nOvEMNLMlryXN4tx6xlomksSR1jG9As/iFfyI7uWppVkVlDyuodA84vTJeQ43kyczTNx0L8Nyo6TNnn1d9soXQoH/9xm0ZZbc1h6O+rI+8oq2hk5F1pwwuJnz95Dj7hzX6tsdJUB+hrPMt5r3oeeTFZ0aIhdyEmZcc056zaJzmipDoX065WhmoEeqjNBUpxXHm4eXqpph5QzpEnr4084zH8M/t4Bzs1ExWomWy0zuyqYk86KgZbq20tCfR7lOe1/yP+QfnZt76mSfbWB+N8VBhjAg/6ZD17hh5GmRWbmf+hQ4O+lY/cM/tWb+ivnKgoiTWGyKB5DBeK6czHhIXRWv166wmKBE+bs/04YXn+mXWIDlgYTpgGs95UlUPKNzH6lGr+Kus2GEw7WZ9VsAEKk+rGJ+SvFLk7hUPHSb2EdYyg2z6wrk+9jjLZ0bCMSg4uEMmvebJodX8o3NItkB9YOQfzvxjDlo+0zk5yR15YJQfX/NySzQGVxhk6CNvzpdZHyJrPPaGkY+nA3K4+BzzZe7QTF/Wr9byjunmbzFd3SzzzyrUeKzlmo+nP5g/34xn9cF/7qI+1nKgHM8+6G+W+shLFrzyHofpT07flvXLBuA680+70z0OPfug/2T1a2GuFzwel++5Cz1hSp6W/SUs9CnvmtK4j7iw7C/smPP78unic09ezxw3nvW/av3in3a7G5jtD+RVnTxGV2O/Gl542fdeM0h0M2EtvvTnZSPxN+sXffK850kxPOeLBlJROnGz0/2j0djuuUOof+bWQfVxDYWddyhwyxUWR2ONTic+niqj1l0us/RDf8ltf4tT0ItqJyomXAdLP61cQ6ULnhgvxaJawfK1/Tca7yoecZIVN+SqilVX8gnlqfEWUtVU2u+DVqJAM+jPI7ffNRJ/v/x1pLM8q7I3gG8tXsZlXMY/jN8QkgJF:E5D8^BY1,3,62^FT17,271^B3N,N,,N,N^FD" + ThreeFiveBotTxt.Text + @"^FS^FT124,172^A0N,28,28^FH\^FD" + ThreeFiveTopTxt.Text + @"^FS^FT124,200^A0N,28,28^FH\^FD"+ ThreeFiveBotTxt.Text + @"^FS^FT253,171^A0N,28,28^FH\^FD"+ ThreeFiveRightTxt.Text + @"^FS^PQ2,0,1,Y^XZ";
            RawPrinterHelper.SendStringToPrinter(printerName, s);
        }
    }
}
