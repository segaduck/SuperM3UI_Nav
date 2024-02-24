using Newtonsoft.Json;
using SuperM3UI_Nav.ViewModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using static SuperM3UI_Nav.View.Home;
using System.IO;
using Path = System.IO.Path;
using static SuperM3UI_Nav.ViewModel.HomeVM;
using System;
using System.Text;

namespace SuperM3UI_Nav
{
    public partial class MainWindow : Window
    {
        private string iniDir = Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini");
        public MainWindow()
        {
            InitializeComponent();

            // 檢查目標檔案是否存在
            if (!File.Exists(HomeVM.SharedDataStore.iniDirectory))
            {
                // 如果目標檔案不存在，則複製檔案
                try
                {
                    File.Copy(HomeVM.SharedDataStore.iniBakDirectory,
                              Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            string result = searchConfig("RomsDirectory");
            if (result != "Null")
            {
                HomeVM.SharedDataStore.romDirectory = result;
            }
            else
            {
                HomeVM.SharedDataStore.addConfig("RomsDirectory",
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Roms"));
                HomeVM.SharedDataStore.romDirectory = 
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Roms");
            }

            result = searchConfig("HideCommand");
            if (result != "Null")
            {
                if (result == "true")
                {
                    HomeVM.SharedDataStore.hideCmd = true;
                }
                else if (result == "false")
                {
                    HomeVM.SharedDataStore.hideCmd = false;
                }
                else
                {
                    HomeVM.SharedDataStore.addConfig("HideCommand", "false");
                    HomeVM.SharedDataStore.hideCmd = false;
                }
            }

            result = searchConfig("DefaultJoystickThemeIndex");
            if (result == "Null")
            {
                HomeVM.SharedDataStore.addConfig("DefaultJoystickThemeIndex", "1");
                HomeVM.SharedDataStore.selectedJoytickIndex = 1;
            }
            else
            {
                HomeVM.SharedDataStore.selectedJoytickIndex = Convert.ToInt32(result);
            }
            HomeVM.updateRomStatus();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        public string searchConfig(string key)
        {
            // 讀取檔案內容
            string[] lines = File.ReadAllLines(iniDir, Encoding.UTF8);
            string keyValue = searchIniKey(lines, key);
            if (keyValue != "Null")
            {
                return keyValue;
            }
            else return "Null";
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
    }
}
