using SuperM3UI_Nav.ViewModel;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using static SuperM3UI_Nav.ViewModel.HomeVM;
using DataFormats = System.Windows.DataFormats;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;
using UserControl = System.Windows.Controls.UserControl;


namespace SuperM3UI_Nav.View
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public string iniDir = Path.Combine(HomeVM.SharedDataStore.iniDirectory, "Supermodel.ini");
        public Settings()
        {
            InitializeComponent();
            cbSelectInput2.ItemsSource = ControlVM.inputOptions;
            cbSelectInput2.DisplayMemberPath = "name";
            cbSelectInput2.SelectedValuePath = "name";
            cbSelectInput2.IsReadOnly = true;
            cbSelectInput2.SelectedIndex = 1;
            string joyindex = HomeVM.IniFileHelper.ReadValue("SuperM3UI", "DefaultJoystickThemeIndex", iniDir);
            if (joyindex != "") HomeVM.SharedDataStore.selectedJoytickIndex=Convert.ToInt32(joyindex);            

            tsHideCmd.IsChecked = HomeVM.SharedDataStore.hideCmd;

            string filePath = Path.Combine(HomeVM.SharedDataStore.currentDirectory, @"resources\readme.rtf");
            if (File.Exists(filePath))
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    TextRange textRange = new TextRange(rtbReadMe.Document.ContentStart, rtbReadMe.Document.ContentEnd);
                    textRange.Load(fs, DataFormats.Rtf);
                }
            }
            tbRomDir.Text = HomeVM.SharedDataStore.romDirectory;
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            tbRomDir.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Roms");
            string preDir = HomeVM.SharedDataStore.romDirectory;
            if (tbRomDir.Text != preDir)
            {
                HomeVM.SharedDataStore.romDirectory = tbRomDir.Text;
                HomeVM.IniFileHelper.WriteValue("SuperM3UI", "RomsDirectory", tbRomDir.Text, iniDir);
                HomeVM.updateRomStatus();
            }
        }

        private void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFileDlg = new FolderBrowserDialog();
            var result = openFileDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Directory.Exists(openFileDlg.SelectedPath))
                {
                    tbRomDir.Text = openFileDlg.SelectedPath;
                    string preDir = HomeVM.SharedDataStore.romDirectory;
                    if (tbRomDir.Text != preDir)
                    {
                        HomeVM.SharedDataStore.romDirectory = tbRomDir.Text;
                        HomeVM.IniFileHelper.WriteValue("SuperM3UI", "RomsDirectory", tbRomDir.Text, iniDir);
                        HomeVM.updateRomStatus();
                    }
                }
                else
                {
                    MessageBox.Show("請選擇一個存在的目錄。");
                }
            }
        }

        private void cbSelectInput2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HomeVM.SharedDataStore.selectedJoytickIndex = cbSelectInput2.SelectedIndex;
            IniFileHelper.WriteValue("SuperM3UI", "DefaultJoystickThemeIndex", cbSelectInput2.SelectedIndex.ToString(), iniDir);
        }

        private void tsHideCmd_Checked(object sender, RoutedEventArgs e)
        {
            IniFileHelper.WriteValue("SuperM3UI", "HideCommand", "true", iniDir);
            HomeVM.SharedDataStore.hideCmd = true;
        }

        private void tsHideCmd_Unchecked(object sender, RoutedEventArgs e)
        {
            IniFileHelper.WriteValue("SuperM3UI", "HideCommand", "false", iniDir);
            HomeVM.SharedDataStore.hideCmd = false;
        }

    }
}
