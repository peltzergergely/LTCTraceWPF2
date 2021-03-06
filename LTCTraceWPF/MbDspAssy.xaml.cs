﻿using ErrorLogging;
using Npgsql;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LTCTraceWPF
{
    /// <summary>
    /// Interaction logic for MbThtWindow.xaml
    /// </summary>
    public partial class MbDspAssy : Window
    {
        public bool IsDmValidated { get; set; } = false;

        public bool AllFieldsValidated { get; set; } = false;

        public bool IsCameraLaunched { get; set; } = true;

        public DateTime? StartedOn { get; set; } = null;

        public bool IsPreChkPassed { get; set; } = false;

        public string[] FilePathStr { get; set; }

        public MbDspAssy()
        {
            Loaded += (sender, e) => MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            InitializeComponent();
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

            if (e.Key == Key.Enter && MbDmTxbx.Text.Length > 0)
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
                if (Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg").Length >= 2)
                {
                    AllFieldsValidated = true;
                }
                else
                {
                    errorMsg += "Legalább 3 képet készíteni kell!";
                }

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
            if (RegexValidation(MbDmTxbx.Text, "MbDmRegEx") == true
                && RegexValidation(DspOne1.Text, "DspDmRegEx") == true
                && RegexValidation(DspOne2.Text, "DspDmRegEx") == true
                && RegexValidation(DspOne3.Text, "DspDmRegEx") == true
                && RegexValidation(DspTwo1.Text, "DspDmRegEx") == true
                && RegexValidation(DspTwo2.Text, "DspDmRegEx") == true
                && RegexValidation(DspTwo3.Text, "DspDmRegEx") == true
                )
                IsDmValidated = true;
            else
                IsDmValidated = false;
        }

        private void ResetForm()
        {
            IsDmValidated = false;
            AllFieldsValidated = false;
            IsCameraLaunched = false;
            IsPreChkPassed = false;
            MbDmTxbx.Text = "";
            DspOne1.Text = "";
            DspOne2.Text = "";
            DspOne3.Text = "";
            DspTwo1.Text = "";
            DspTwo2.Text = "";
            DspTwo3.Text = "";
            MbDmTxbx.Focus();
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
            using (new WaitCursor())
            {
                FilePathStr = Directory.GetFiles(@"c:\TraceImages\", "*.Jpeg");
                int imgArrayLimit = 9;
                if (FilePathStr.Length > imgArrayLimit)
                {
                    MessageBox.Show("A készített képek száma meghaladja a maximum 9 képes limitet! Töröld ki a fölösleget a TraceImages mappából!'");
                    System.Diagnostics.Process.Start("explorer.exe", @"c:\TraceImages\");
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
                            var cmd = new NpgsqlCommand("insert into " + table + " (mb_dm, dsp_one_one, dsp_one_two, dsp_one_three, dsp_two_one, dsp_two_two, dsp_two_three, pc_name, started_on, saved_on, pic1, pic2, pic3, pic4, pic5, pic6, pic7, pic8, pic9) " +
                            "values(:mb_dm, :dsp_one_one, :dsp_one_two, :dsp_one_three, :dsp_two_one, :dsp_two_two, :dsp_two_three, :pc_name, :started_on, :saved_on, :pic1, :pic2, :pic3, :pic4, :pic5, :pic6, :pic7, :pic8, :pic9)", conn);
                            cmd.Parameters.Add(new NpgsqlParameter("mb_dm", MbDmTxbx.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("dsp_one_one", DspOne1.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("dsp_one_two", DspOne2.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("dsp_one_three", DspOne3.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("dsp_two_one", DspTwo1.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("dsp_two_two", DspTwo2.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("dsp_two_three", DspTwo3.Text));
                            cmd.Parameters.Add(new NpgsqlParameter("pc_name", System.Environment.MachineName));
                            cmd.Parameters.Add(new NpgsqlParameter("started_on", StartedOn));
                            cmd.Parameters.Add(new NpgsqlParameter("saved_on", DateTime.Now));
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
                                System.IO.Directory.CreateDirectory("C:\\TraceImagesArchive\\" + "MBDM_" + MbDmTxbx.Text);
                                for (int i = 0; i < FilePathStr.Length; i++)
                                {
                                    File.Move(FilePathStr[i], "C:\\TraceImagesArchive\\" + "MBDM_" + MbDmTxbx.Text + "\\" + Path.GetFileName(FilePathStr[i]));
                                }
                                Resultlbl.Text = "Adatok elmentve!" + DateTime.Now;
                                ResetForm();
                            }
                            else CallMessageForm("Hiba a feltöltésben!");
                            conn.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void MbDmTxbx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ConfigurationManager.AppSettings["MbHsAssy"] == "true" && MbDmTxbx.Text.Length > 0 )
            {
                var preCheck = new DatabaseHelper();
                if (preCheck.CountRowInDB("mb_hs_assy", "mb_dm", MbDmTxbx.Text) == 0)
                {
                    if (ConfigurationManager.AppSettings["PreCheckMode"] == "hard")
                    {
                        CallMessageForm("Előző munkafolyamaton nem szerepelt a termék!");
                    }
                    else
                    {
                        ErrorLog.Create("mb_hs_assy", "mb_dm", MbDmTxbx.Text,MethodBase.GetCurrentMethod().Name.ToString(), "Előző munkafolyamaton nem szerepelt a termék!", this.GetType().Name.ToString());
                    }
                }
                
            }

            StartedOn = DateTime.Now;
        }

        private void MainMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WebCamLaunchClick(object sender, RoutedEventArgs e)
        {
            using (new WaitCursor())
            {
                // very long task
                SaveBtn.Focus();
                var webCam = new camApp();
                webCam.Show();
                IsCameraLaunched = true;
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            FormValidator();
            if (AllFieldsValidated)
            {
                DbInsert("mb_dsp_assy");
            }
        }

        private void DSPnotReadable(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name.Contains("11"))
            {
                DspOne1.Text = "NEM OLVASHATÓ";
                DspOne1.Focus();
            }

            if ((sender as Button).Name.Contains("12"))
            {
                DspOne2.Text = "NEM OLVASHATÓ";
                DspOne2.Focus();
            }

            if ((sender as Button).Name.Contains("13"))
            {
                DspOne3.Text = "NEM OLVASHATÓ";
                DspOne3.Focus();
            }

            if ((sender as Button).Name.Contains("21"))
            {
                DspTwo1.Text = "NEM OLVASHATÓ";
                DspTwo1.Focus();
            }

            if ((sender as Button).Name.Contains("22"))
            {
                DspTwo2.Text = "NEM OLVASHATÓ";
                DspTwo2.Focus();
            }

            if ((sender as Button).Name.Contains("23"))
            {
                DspTwo3.Text = "NEM OLVASHATÓ";
                DspTwo3.Focus();
            }

            TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
            UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;
            keyboardFocus.MoveFocus(tRequest);
        }
    }
}
