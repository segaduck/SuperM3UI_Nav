using SuperM3UI_Nav.Utilities;
using System.Windows.Input;

namespace SuperM3UI_Nav.ViewModel
{
    class NavigationVM : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
        public ICommand VideoCommand { get; set; }
        public ICommand NetworkCommand { get; set; }
        public ICommand SettingsCommand { get; set; }
        public ICommand TestCommand { get; set; }
        public ICommand SoundCommand { get; set; }
        public ICommand ControlCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void Video(object obj) => CurrentView = new VideoVM();
        private void Networks(object obj) => CurrentView = new NetworkVM();
        private void Setting(object obj) => CurrentView = new SettingVM();
        private void Test(object obj) => CurrentView = new TestVM();
        private void Sound(object obj) => CurrentView = new SoundVM();
        private void Control(object obj) => CurrentView = new ControlVM();

        public NavigationVM()
        {
            HomeCommand = new RelayCommand(Home);
            NetworkCommand = new RelayCommand(Networks);
            VideoCommand = new RelayCommand(Video);
            SettingsCommand = new RelayCommand(Setting);
            TestCommand = new RelayCommand(Test);
            SoundCommand = new RelayCommand(Sound);
            ControlCommand = new RelayCommand(Control);

            // Startup Page
            CurrentView = new HomeVM();
        }
    }
}
