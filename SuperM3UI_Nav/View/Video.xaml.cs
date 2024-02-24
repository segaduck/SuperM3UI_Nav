using SharpDX;
using SuperM3UI_Nav.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Path = System.IO.Path;

namespace SuperM3UI_Nav.View
{
    public partial class Video : UserControl
    {
        public static int selectRes;
        public string iniDir = Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini");
        public Video()
        {
            HomeVM.SharedDataStore.initIni = true;
            InitializeComponent();
            DataContext = this;
            loadConfig();
            cb_Resolution.ItemsSource = ResItems;
            cb_Resolution.DisplayMemberPath = "name";
            cb_Resolution.SelectedValuePath = "name";
            cb_Resolution.IsReadOnly = true;
            cb_Resolution.SelectedIndex = selectRes;
        }

        public List<ResolutionItem> ResItems = new List<ResolutionItem>
            {
            new ResolutionItem { name = "496 x 384", width = 496, height = 384 },
            new ResolutionItem { name = "640 x 480", width = 640, height = 480 },
            new ResolutionItem { name = "800 x 600", width = 800, height = 600 },
            new ResolutionItem { name = "1024 x 768", width = 1024, height = 768 },
            new ResolutionItem { name = "1152 x 864", width = 1152, height = 864 },
            new ResolutionItem { name = "1280 x 960", width = 1280, height = 960 },
            new ResolutionItem { name = "2048 x 1536", width = 2048, height = 1536 },
            new ResolutionItem { name = "2880 x 2160", width = 2880, height = 2160 },
            new ResolutionItem { name = "854 x 480", width = 854, height = 480 },
            new ResolutionItem { name = "1280 x 720", width = 1280, height = 720 },
            new ResolutionItem { name = "1280 x 800", width = 1280, height = 800 },
            new ResolutionItem { name = "1366 x 768", width = 1366, height = 768 },
            new ResolutionItem { name = "1600 x 900", width = 1600, height =900 },
            new ResolutionItem { name = "1600 x 1024", width = 1600, height = 1024},
            new ResolutionItem { name = "1680 x 1050", width = 1680, height = 1050},
            new ResolutionItem { name = "1920 x 1080", width = 1920, height = 1080},
            new ResolutionItem { name = "1920 x 1200", width = 1920, height = 1200},
            new ResolutionItem { name = "2560 x 1440", width = 2560, height = 1440},
            new ResolutionItem { name = "2730 x 1536", width = 2730, height = 1536},
            new ResolutionItem { name = "3840 x 2160", width = 3840, height = 2160},
            };

        public class ResolutionItem
        {
            public string name { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public bool isSep { get; set; } = false;
        }

        private void loadConfig()
        {
            HomeVM.SharedDataStore.initIni = true;

            // 讀取檔案內容
            string[] lines = File.ReadAllLines(Path.Combine(HomeVM.SharedDataStore.iniDirectory, "supermodel.ini"), Encoding.UTF8);

            string keyValue = null;
            int xRes = 0;
            int yRes = 0;
            int result = 0;

            keyValue = searchIniKey(lines, "XResolution");
            //keyValue = HomeVM.IniFileHelper.ReadValue("Global", "XResolution", iniDir);
            if (int.TryParse(keyValue, out result))
            {
                xRes = Convert.ToInt32(keyValue);
            }
            else xRes = 0;

            result = 0;
            keyValue = searchIniKey(lines, "YResolution");
            //keyValue = HomeVM.IniFileHelper.ReadValue("Global", "YResolution", iniDir);
            if (int.TryParse(keyValue, out result))
            {
                yRes = Convert.ToInt32(keyValue);
            }
            else yRes = 0;

            int i = 0;
            selectRes = -1;
            foreach (ResolutionItem item in ResItems)
            {
                if (item.width == xRes && item.height == yRes)
                {
                    cb_Resolution.SelectedIndex = i;
                    selectRes = i;
                    break;
                }
                i++;
            }
            if (selectRes == -1)
            {
                selectRes = 0;
                cb_Resolution.SelectedIndex = 0;
                HomeVM.IniFileHelper.WriteValue("Global", "XResolution", "496", iniDir);
                HomeVM.IniFileHelper.WriteValue("Global", "YResolution", "384", iniDir);
            }

            cleanupIniCheckBox(lines, ts_FullScreen, "Global", "FullScreen");
            cleanupIniCheckBox(lines, ts_WideScreen, "Global", "WideScreen");
            cleanupIniCheckBox(lines, ts_WideBG, "Global", "WideBackground");
            cleanupIniCheckBox(lines, ts_Vsync, "Global", "VSync");

            keyValue = searchIniKey(lines, "RefreshRate");
            //keyValue = HomeVM.IniFileHelper.ReadValue("Global", "RefreshRate", iniDir);
            if (keyValue == "57.524")
            {
                ts_TrueHz.IsChecked = true;
            }
            else if (keyValue == "60.000")
            {
                ts_TrueHz.IsChecked = false;
            }
            else
            {
                ts_TrueHz.IsChecked = false;
                HomeVM.IniFileHelper.WriteValue("Global", "RefreshRate", "60.000", iniDir);
            }

            cleanupIniCheckBox(lines, ts_Stretch, "Global", "Stretch");
            cleanupIniCheckBox(lines, ts_Throttle, "Global", "Throttle");
            cleanupIniCheckBox(lines, ts_ShowFR, "Global", "ShowFrameRate");
            cleanupIniCheckBox(lines, ts_New3D, "Global", "New3DEngine");
            cleanupIniCheckBox(lines, ts_GPUMT, "Global", "GPUMultiThreaded");
            cleanupIniCheckBox(lines, ts_MultiThread, "Global", "MultiThreaded");
            cleanupIniCheckBox(lines, ts_MultiTexture, "Global", "MultiTexture");
            cleanupIniCheckBox(lines, ts_QuadR, "Global", "QuadRendering");
            cleanupIniCheckBox(lines, ts_EmuSound, "Global", "EmulateSound");
            cleanupIniCheckBox(lines, ts_EmuDSB, "Global", "EmulateDSB");
            cleanupIniCheckBox(lines, ts_LegacySDSP, "Global", "LegacySoundDSP");
            cleanupIniCheckBox(lines, ts_FlipStereo, "Global", "FlipStereo");
            cleanupIniCheckBox(lines, ts_Network, "Global", "Network");
            cleanupIniCheckBox(lines, ts_SimNet, "Global", "SimulateNet");

            cleanupIniIntNum(lines, tb_PPCFreq, "Global", "PowerPCFrequency", 99, 0, "49");
            cleanupIniIntNum(lines, tb_PortIn, "Global", "PortIn", 9999, 0, "1970");
            cleanupIniIntNum(lines, tb_PortOut, "Global", "PortOut", 9999, 0, "1971");

            keyValue = searchIniKey(lines, "AddressOut");
            //keyValue = HomeVM.IniFileHelper.ReadValue("Global", "AddressOut", iniDir);
            if (keyValue != "")
            {
                tb_IPAddr.Text = keyValue.Replace("_", "");
            }
            HomeVM.SharedDataStore.initIni = false;
        }

        public void cleanupIniCheckBox(String[] lines, CheckBox checkBox, String section, String key)
        {
            String value = searchIniKey(lines, key);
            //String value = HomeVM.IniFileHelper.ReadValue(section, key, iniDir);
            if (value.Contains("\""))
            {
                value = value.Replace("\"", "");
                HomeVM.IniFileHelper.WriteValue(section, key, value, iniDir);
            }
            if (value == "true")
            {
                checkBox.IsChecked = true;
            }
            else
            {
                checkBox.IsChecked = false;
            }
        }

        public void cleanupIniIntNum (String[] lines, TextBox textbox, String section, String key, int max, int min, String def)
        {
            String value = searchIniKey(lines, key);
            //String value = HomeVM.IniFileHelper.ReadValue("Global", key, iniDir);
            int result = 0;
            if (value != "")
            {
                result = 0;
                if (int.TryParse(value, out result))
                {
                    if ((result < min) || (result > max))
                    {
                        HomeVM.IniFileHelper.WriteValue("Global", key, def, iniDir);
                        textbox.Text = def;
                    }
                    else textbox.Text = value;
                }
                else
                {
                    HomeVM.IniFileHelper.WriteValue("Global", key, def, iniDir);
                    textbox.Text = def;
                }
            }
            else
            {
                HomeVM.IniFileHelper.WriteValue("Global", key, def, iniDir);
                textbox.Text = def;
            }
        }

        private string searchIniKey(string[] lines, string key)
        {
            string value = "Null";
            string[] searchKeys = { key };
            // 搜尋特定項目
            for (int i = 0; i < lines.Length; i++)
            {
                foreach (string keyI in searchKeys)
                {
                    if (lines[i].StartsWith(keyI))
                    {
                        string[] parts = lines[i].Split('=');
                        if (parts.Length == 2)
                        {
                            value = parts[1];
                        }
                    }
                }
            }
            return value;
        }

        // Graphics Config 
        private void ts_FullScreen_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("FullScreen", "true");
            //HomeVM.IniFileHelper.WriteValue("Global", "FullScreen", "true", iniDir);
        }

        private void ts_FullScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("FullScreen", "false");
        }

        private void ts_WideScreen_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("WideScreen", "true");
        }

        private void ts_WideScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("WideScreen", "false");
        }

        private void ts_WideBG_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("WideBackground", "true");
        }

        private void ts_WideBG_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("WideBackground", "false");
        }

        private void ts_Vsync_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("VSync", "true");
        }

        private void ts_Vsync_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("VSync", "false");
        }

        private void ts_TrueHz_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("RefreshRate", "57.524");
        }

        private void ts_TrueHz_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("RefreshRate", "60.000");
        }

        private void ts_Stretch_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("Stretch", "true");
        }

        private void ts_Stretch_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("Stretch", "false");
        }

        private void ts_Throttle_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("Throttle", "true");
        }

        private void ts_Throttle_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("Throttle", "false");
        }

        private void ts_ShowFR_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("ShowFrameRate", "true");
        }

        private void ts_ShowFR_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("ShowFrameRate", "false");
        }

        private void ts_New3D_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("New3DEngine", "true");
        }

        private void ts_New3D_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("New3DEngine", "false");
        }

        private void ts_GPUMT_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("GPUMultiThreaded", "true");
        }

        private void ts_GPUMT_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("GPUMultiThreaded", "false");
        }

        private void ts_MultiThread_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("MultiThreaded", "true");
        }

        private void ts_MultiThread_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("MultiThreaded", "false");
        }

        private void ts_MultiTexture_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("MultiTexture", "true");
        }

        private void ts_MultiTexture_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("MultiTexture", "false");
        }

        private void ts_QuadR_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("QuadRendering", "true");
        }

        private void ts_QuadR_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("QuadRendering", "false");
        }

        // Sound Config
        private void ts_EmuSound_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("EmulateSound", "true");
        }
        private void ts_EmuSound_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("EmulateSound", "false");
        }

        private void ts_EmuDSB_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("EmulateDSB", "true");
        }
        private void ts_EmuDSB_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("EmulateDSB", "false");
        }

        private void ts_LegacySDSP_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("LegacySoundDSP", "true");
        }

        private void ts_LegacySDSP_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("LegacySoundDSP", "false");
        }


        private void ts_FlipStereo_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("FlipStereo", "true");
        }

        private void ts_FlipStereo_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("FlipStereo", "false");
        }

        // Network Config
        private void ts_Network_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("Network", "true");
        }

        private void ts_Network_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("Network", "false");
        }

        private void ts_SimNet_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("SimulateNet", "true");
        }

        private void ts_SimNet_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("SimulateNet", "false");
        }

        private void tb_PPCFreq_TextChanged(object sender, TextChangedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("PowerPCFrequency", tb_PPCFreq.Text.Replace("_", ""));
        }

        private void tb_PortIn_TextChanged(object sender, TextChangedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("PortIn", tb_PortIn.Text.Replace("_", ""));
        }

        private void tb_PortOut_TextChanged(object sender, TextChangedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("PortOut", tb_PortOut.Text.Replace("_", ""));
        }

        private void tb_IPAddr_TextChanged(object sender, TextChangedEventArgs e)
        {
            HomeVM.SharedDataStore.updateConfig("AddressOut", tb_IPAddr.Text.Replace("_", ""));
        }

        private void cb_Resolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!HomeVM.SharedDataStore.initIni)
            {
                ResolutionItem selectedItem = cb_Resolution.SelectedItem as ResolutionItem;
                if (selectedItem != null)
                {
                    HomeVM.SharedDataStore.updateConfig("XResolution", selectedItem.width.ToString());
                    HomeVM.SharedDataStore.updateConfig("YResolution", selectedItem.height.ToString());
                }
            }
        }

        private void btnSetControl_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process externalProcess = new Process();
                externalProcess.StartInfo.FileName = "cmd.exe";
                if (HomeVM.SharedDataStore.hideCmd == true)
                    externalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                else externalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                externalProcess.StartInfo.WorkingDirectory = HomeVM.SharedDataStore.emuDirectory;
                externalProcess.StartInfo.Arguments = "supermodel "+ HomeVM.SharedDataStore.romDirectory+ ".zip -input-system=xinput";
                externalProcess.Start();
                // 等待外部程式結束
                externalProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
