using Newtonsoft.Json;
using SuperM3UI_Nav.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using static SuperM3UI_Nav.View.Home;
using Path = System.IO.Path;

namespace SuperM3UI_Nav.ViewModel
{
    class HomeVM : Utilities.ViewModelBase
    {
        public static class SharedDataStore
        {
            public static string currentDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;
            //public static string currentDirectory { get; set; } = @"C:\Development\SuperM3UI_Nav\SuperM3UI_Nav";

            public static string emuDirectory { get; set; } = currentDirectory;
            //public static string emuDirectory { get; set; } = @"C:\spikeout\";

            public static string iniDirectory { get; set; } = Path.Combine(emuDirectory, @"config");
            //public static string iniDirectory { get; } = @"C:\spikeout\config";
            public static string romDirectory { get; set; } = Path.Combine(emuDirectory, @"Roms");
            //public static string romDirectory { get; set;  } = @"C:\spikeout\Roms";
            
            public static string iniBakDirectory { get; } = Path.Combine(currentDirectory, @"resources\Supermodel.ini");
            public static string moviesDirectory { get; } = Path.Combine(currentDirectory, @"movies");
            public static string glistJsonDir { get; } = Path.Combine(currentDirectory, @"resources\m3gameslist.json");
            public static bool romOnly { get; set; } = false;
            public static bool favOnly { get; set; } = false;
            public static string selectedGame { get; set; } = "spikeofe";
            public static int selectedJoytickIndex { get; set; } = 1;
            public static int selectedSpikeoutIndex { get; set; } = 0;
            public static int SpikeoutMode { get; set; } = 0;
            public static bool initIni { get; set; } = true;
            public static bool hideCmd { get; set; } = false;

            public static void updateConfig(string key, string value)
            {
                if (!initIni)
                {
                    // 設定要搜尋和修改的項目
                    string[] searchKeys = { key };

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

                    // 讀取檔案內容
                    string[] lines = File.ReadAllLines(Path.Combine(HomeVM.SharedDataStore.iniDirectory, "supermodel.ini"), Encoding.UTF8);

                    // 搜尋和修改特定項目
                    bool found = false;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        foreach (string keyI in searchKeys)
                        {
                            if (lines[i].StartsWith(keyI))
                            {
                                // 如果找到對應的項目，進行數值修改
                                string[] parts = lines[i].Split('=');
                                if (parts.Length == 2)
                                {
                                    found = true;
                                    // 修改數值
                                    parts[1] = value;  // 在這裡設定新的數值

                                    // 組合成修改後的行
                                    lines[i] = string.Join("=", parts);
                                }
                            }
                        }
                    }

                    // 寫回檔案
                    if (!found)
                    {
                        // 將 string[] 轉換成 List<string>
                        List<string> linesList = new List<string>(lines);
                        // 新增一行
                        linesList.Add(key + "=" + value);
                        // 將 List<string> 轉換回 string[]
                        lines = linesList.ToArray();
                    }
                    File.WriteAllLines(Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini"), lines, Encoding.UTF8);

                    //MessageBox.Show("INI檔案已經成功修改。");
                }
            }
            public static void addConfig(string key, string value)
            {
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

                    // 讀取檔案內容
                    string[] lines = File.ReadAllLines(Path.Combine(HomeVM.SharedDataStore.iniDirectory, "supermodel.ini"), Encoding.UTF8);

                    // 將 string[] 轉換成 List<string>
                    List<string> linesList = new List<string>(lines);
                    // 新增一行
                    linesList.Add(key + "=" + value);
                    // 將 List<string> 轉換回 string[]
                    lines = linesList.ToArray();
                    File.WriteAllLines(Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini"), lines, Encoding.UTF8);
            }
        }

        public static class IniFileHelper
        {
            public static int capacity = 512;

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder value, int size, string filePath);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            private static extern int GetPrivateProfileString(string section, string key, string defaultValue, [In][Out] char[] value, int size, string filePath);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            private static extern int GetPrivateProfileSection(string section, IntPtr keyValue, int size, string filePath);

            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool WritePrivateProfileString(string section, string key, string value, string filePath);

            public static bool WriteValue(string section, string key, string value, string filePath)
            {
                return WritePrivateProfileString(section, key, value, filePath);
            }

            public static bool DeleteSection(string section, string filepath)
            {
                return WritePrivateProfileString(section, null, null, filepath);
            }

            public static bool DeleteKey(string section, string key, string filepath)
            {
                return WritePrivateProfileString(section, key, null, filepath);
            }

            public static string ReadValue(string section, string key, string filePath, string defaultValue = "")
            {
                StringBuilder stringBuilder = new StringBuilder(capacity);
                GetPrivateProfileString(section, key, defaultValue, stringBuilder, stringBuilder.Capacity, filePath);
                return stringBuilder.ToString();
            }

            public static string[] ReadSections(string filePath)
            {
                char[] value;
                int privateProfileString;
                while (true)
                {
                    value = new char[capacity];
                    privateProfileString = GetPrivateProfileString(null, null, "", value, capacity, filePath);
                    if (privateProfileString == 0)
                    {
                        return new string[0];
                    }
                    if (privateProfileString < capacity - 2)
                    {
                        break;
                    }
                    capacity *= 2;
                }
                return new string(value, 0, privateProfileString).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
            }

            public static string[] ReadKeys(string section, string filePath)
            {
                char[] value;
                int privateProfileString;
                while (true)
                {
                    value = new char[capacity];
                    privateProfileString = GetPrivateProfileString(section, null, "", value, capacity, filePath);
                    if (privateProfileString == 0)
                    {
                        return null;
                    }
                    if (privateProfileString < capacity - 2)
                    {
                        break;
                    }
                    capacity *= 2;
                }
                return new string(value, 0, privateProfileString).Split(new char[1], StringSplitOptions.RemoveEmptyEntries);
            }

            public static string[] ReadKeyValuePairs(string section, string filePath)
            {
                IntPtr intPtr;
                int privateProfileSection;
                while (true)
                {
                    intPtr = Marshal.AllocCoTaskMem(capacity * 2);
                    privateProfileSection = GetPrivateProfileSection(section, intPtr, capacity, filePath);
                    if (privateProfileSection == 0)
                    {
                        Marshal.FreeCoTaskMem(intPtr);
                        return null;
                    }
                    if (privateProfileSection < capacity - 2)
                    {
                        break;
                    }
                    Marshal.FreeCoTaskMem(intPtr);
                    capacity *= 2;
                }
                string text = Marshal.PtrToStringAuto(intPtr, privateProfileSection - 1);
                Marshal.FreeCoTaskMem(intPtr);
                return text.Split(default(char));
            }
        }

        public static void updateRomStatus()
        {
            // Read the content of the JSON file
            string json = File.ReadAllText(HomeVM.SharedDataStore.glistJsonDir);

            // Parse the JSON data into a List<GameInfo> object
            List<GameInfo> games = JsonConvert.DeserializeObject<List<GameInfo>>(json);

            // Check if ROM files exist and update the is_rom field
            foreach (var game in games)
            {
                if (File.Exists(Path.Combine(HomeVM.SharedDataStore.romDirectory, game.rom + ".zip")))
                {
                    game.is_rom = 1;
                }
                else
                {
                    game.is_rom = 0;
                }
            }

            // Convert the modified List<GameInfo> object back to JSON format
            string updatedJson = JsonConvert.SerializeObject(games, Formatting.Indented);

            // Write the updated JSON content back to the file
            File.WriteAllText(HomeVM.SharedDataStore.glistJsonDir, updatedJson);
        }
    }
}
