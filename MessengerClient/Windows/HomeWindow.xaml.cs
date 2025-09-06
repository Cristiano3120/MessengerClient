using System.Windows;

namespace MessengerClient.Windows
{
    public partial class HomeWindow : Window
    {
        private readonly User _user;

        public HomeWindow(User user)
        {
            InitializeComponent();
            _user = user;
        }
    }
}
