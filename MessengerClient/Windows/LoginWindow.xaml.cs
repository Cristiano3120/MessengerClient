using MessengerClient.APIResponse;
using System.Windows;
using System.Windows.Media;

namespace MessengerClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, RootGrid);

            TextBoxWithDescription emailBoxWithDescription = new(EmailTextBox, EmailTextBlock);
            TextBoxWithDescription passwordBoxWithDescription = new(PasswordTextBox, PasswordTextBlock);
            GUIHelper.ChangeTextBlockPos(emailBoxWithDescription, passwordBoxWithDescription);

            LoginBtn.Click += LoginAsync;
            CreateAccHyperlink.Click += CreateAcc;
        }

        private void CreateAcc(object sender, RoutedEventArgs args)
        {
            _ = GUIHelper.SwitchWindows<Login, CreateAccWindow>();
        }

        private async void LoginAsync(object sender, RoutedEventArgs args)
        {
            LoginData loginData = new()
            {
                Email = EmailTextBox.Text,
                Password = PasswordTextBox.Text,
                IsAutoLogin = false,
            };

            if (string.IsNullOrEmpty(loginData.Email) || !loginData.Email.Contains('@') 
                || string.IsNullOrEmpty(loginData.Password))
            {
                _ = GUIHelper.DisplayInfosAsync(ErrorTextBlock, "Email or password is invalid!", Brushes.Red);
                return;
            }

            APIResponse<User> aPIResponse = await Http.PostAsync<LoginData, User>(loginData, "auth/login", CallerInfos.Create());
            if (aPIResponse.IsSuccess)
            {
                HomeWindow home = new(aPIResponse.Data);
                _ = GUIHelper.SwitchWindows<Login, HomeWindow>(home);

                return;
            }

            _ = GUIHelper.DisplayInfosAsync(ErrorTextBlock, aPIResponse.APIError.Message, Brushes.Red);
        }
    }
}
