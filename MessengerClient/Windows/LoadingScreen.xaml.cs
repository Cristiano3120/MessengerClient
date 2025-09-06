using System.Windows;

namespace MessengerClient
{
    public partial class LoadingScreen : Window
    {
        public LoadingScreen()
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, RootGrid);
            ResizeMode = ResizeMode.NoResize;
        }
    }
}