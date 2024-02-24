using Newtonsoft.Json;
using SuperM3UI_Nav.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static SuperM3UI_Nav.ViewModel.HomeVM;
using Formatting = Newtonsoft.Json.Formatting;
using Path = System.IO.Path;


namespace SuperM3UI_Nav.View
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public string iniDir = Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini");
        public Home()
        {
            InitializeComponent();
            DataContext = this;

            string section = "SuperM3UI";
            if (IniFileHelper.ReadValue(section, "FavoriteOnly", iniDir) == "true")
            {
                ts_favoriateOnly.IsChecked = true;
            }
            else if (IniFileHelper.ReadValue(section, "FavoriteOnly", iniDir) == "false")
            {
                ts_favoriateOnly.IsChecked = false;
            }
            else
            {
                IniFileHelper.WriteValue(section, "FavoriteOnly", "false", iniDir);
                ts_favoriateOnly.IsChecked = false;
            }


            if (IniFileHelper.ReadValue(section, "AvaliableRomsOnly", iniDir) == "true")
            {
                ts_IsRomOnly.IsChecked = true;
            }
            else if (IniFileHelper.ReadValue(section, "AvaliableRomsOnly", iniDir) == "false")
            {
                ts_IsRomOnly.IsChecked = false;
            }
            else
            {
                IniFileHelper.WriteValue(section, "AvaliableRomsOnly", "false", iniDir);
                ts_IsRomOnly.IsChecked = false;
            }
        }

        public class GameInfo
        {
            public int id { get; set; }
            public int favoriate { get; set; }
            public string Games { get; set; }
            public string rom { get; set; }
            public string version { get; set; }
            public int is_rom { get; set; }
            public string movie { get; set; }
        }
        public string preVideoPath { get; set; } = null;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            ts_IsRomOnly.IsChecked = HomeVM.SharedDataStore.romOnly;
            ts_favoriateOnly.IsChecked = HomeVM.SharedDataStore.favOnly;
            LoadDataGrid(HomeVM.SharedDataStore.romOnly, HomeVM.SharedDataStore.favOnly);
            RridGameLists.SelectedIndex = 0;
        }

        private void DataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridRow dgr = sender as DataGridRow;
            dgr.IsSelected = true;
        }

        private void SaveDataGrid()
        {
            if (RridGameLists.ItemsSource != null)
            {
                //DataView dv = RridGameLists.ItemsSource as DataView;

                DataView dv = (DataView)RridGameLists.ItemsSource;

                if (dv != null)
                {
                    // 清除 RowFilter，將所有資料重新顯示
                    dv.RowFilter = string.Empty;

                    DataTable dt = dv.ToTable();
                    string json = JsonConvert.SerializeObject(dt, Formatting.Indented);
                    File.WriteAllText(HomeVM.SharedDataStore.glistJsonDir, json);
                }
            }
        }
        private void LoadDataGrid(bool is_rom, bool is_fav)
        {
            string json = File.ReadAllText(HomeVM.SharedDataStore.glistJsonDir);
            DataTable dt = JsonConvert.DeserializeObject<DataTable>(json);
            DataView dv = new DataView(dt);
            if (is_rom && is_fav)
            {
                // 同時滿足 is_rom=1 和 is_fav=1 時，使用 AND 連接條件
                dv.RowFilter = "is_rom=1 AND favoriate=1";
            }
            else if (is_rom)
            {
                dv.RowFilter = "is_rom=1";
            }
            else if (is_fav)
            {
                dv.RowFilter = "favoriate=1";
            }
            RridGameLists.ItemsSource = dv;
        }

        private void ChangeVideo(string videoPath)
        {
            // 新的影片檔案路徑
            string newVideoPath = videoPath;

            // 檢查檔案是否存在
            if (System.IO.File.Exists(videoPath) && (newVideoPath != preVideoPath))
            {

                // 停止目前的播放
                snapVideo.Visibility = Visibility.Collapsed;

                // 創建新的 MediaTimeline
                MediaTimeline newMediaTimeline = new MediaTimeline
                {
                    Source = new Uri(newVideoPath, UriKind.Absolute),
                    RepeatBehavior = RepeatBehavior.Forever
                };

                // 創建新的 StoryboardStoryboard();
                Storyboard newStoryboard = new Storyboard();
                // 將新的 MediaTimeline 加入到 Storyboard 中
                newStoryboard.Children.Add(newMediaTimeline);
                // 開始播放
                snapVideo.Visibility = Visibility.Visible;
                snapVideo.BeginStoryboard(newStoryboard);
            }
            preVideoPath = videoPath;
        }

        private void btnLaunch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process externalProcess = new Process();
                externalProcess.StartInfo.FileName = "cmd.exe";
                if (HomeVM.SharedDataStore.hideCmd)
                {
                    externalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    externalProcess.StartInfo.CreateNoWindow = true;
                }
                else
                {
                    externalProcess.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                }
                externalProcess.StartInfo.WorkingDirectory = HomeVM.SharedDataStore.emuDirectory;
                string filePath = Path.Combine(HomeVM.SharedDataStore.romDirectory, HomeVM.SharedDataStore.selectedGame + ".zip");
                if (System.IO.File.Exists(filePath))
                {
                    externalProcess.StartInfo.Arguments = "/c supermodel " + filePath;
                    externalProcess.Start();
                    // 等待外部程式結束
                    externalProcess.WaitForExit();
                }
                else MessageBox.Show("ROM files are not found", "Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void ts_IsRomOnly_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.romOnly = true;
            IniFileHelper.WriteValue("SuperM3UI", "AvaliableRomsOnly", "true", iniDir);
            LoadDataGrid(HomeVM.SharedDataStore.romOnly, HomeVM.SharedDataStore.favOnly);
        }

        private void ts_IsRomOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.romOnly = false;
            IniFileHelper.WriteValue("SuperM3UI", "AvaliableRomsOnly", "false", iniDir);
            LoadDataGrid(HomeVM.SharedDataStore.romOnly, HomeVM.SharedDataStore.favOnly);
        }


        private void ts_favoriateOnly_Checked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.favOnly = true;
            IniFileHelper.WriteValue("SuperM3UI", "FavoriteOnly", "true", iniDir);
            LoadDataGrid(HomeVM.SharedDataStore.romOnly, HomeVM.SharedDataStore.favOnly);
        }

        private void ts_favoriateOnly_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeVM.SharedDataStore.favOnly = false;
            IniFileHelper.WriteValue("SuperM3UI", "FavoriteOnly", "false", iniDir);
            LoadDataGrid(HomeVM.SharedDataStore.romOnly, HomeVM.SharedDataStore.favOnly);
        }


        private void RridGameLists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 檢查是否有選中項目
            if (RridGameLists.SelectedItem != null)
            {
                // 取得選中行的資料物件（DataRowView）
                DataRowView rowView = (DataRowView)RridGameLists.SelectedItem;

                // 確保資料物件不為空
                if (rowView != null)
                {
                    string rom = rowView["rom"].ToString();
                    string game = rowView["Games"].ToString();
                    string ver = rowView["version"].ToString();
                    string movie = rowView["movie"].ToString();

                    string logoDir = Path.Combine(HomeVM.SharedDataStore.currentDirectory, @"Snaps\" + rom) + "-c.png";
                    HomeVM.SharedDataStore.selectedGame = rom;
                    if (File.Exists(logoDir))
                    {
                        logImg.Source = new BitmapImage(new Uri(logoDir));
                        msgBox.Text = game;
                        verBox.Text = ver;
                    }


                    string snapDir = Path.Combine(HomeVM.SharedDataStore.moviesDirectory, movie);
                    if (File.Exists(snapDir))
                    {
                        ChangeVideo(snapDir);
                        snapVideo.Volume = 0;
                    }
                    else
                    {
                        snapVideo.Visibility = Visibility.Collapsed;
                        snapVideo.Volume = 0;
                    }

                }
            }
        }

        private void RridGameLists_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RridGameLists.SelectedItem != null)
            {
                // get the selected row (DataRowView）
                DataGrid dg = sender as DataGrid;
                DataRowView rowView = dg.SelectedItem as DataRowView;

                if (rowView != null)
                {
                    // change the value of the favoriate field
                    int fav = Convert.ToInt32(rowView["favoriate"]);
                    if (fav == 1) rowView["favoriate"] = 0;
                    else rowView["favoriate"] = 1;

                    // write the updated data back to the JSON file
                    SaveDataGrid();

                    // update the DataGrid
                    LoadDataGrid(HomeVM.SharedDataStore.romOnly, HomeVM.SharedDataStore.favOnly);
                }
            }
        }

    }
}
