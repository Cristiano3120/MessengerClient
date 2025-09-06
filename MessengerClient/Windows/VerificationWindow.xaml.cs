using MessengerClient.APIResponse;
using System.Windows;
using System.Windows.Media;

namespace MessengerClient.Windows
{
    public partial class VerificationWindow : Window
    {
        public VerificationWindow(string email, long userId)
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, RootGrid);
            GUIHelper.ChangeTextBlockPos(new TextBoxWithDescription(CodeTextBox, InfoTextBlock, -95));

            InfoTextBlock.Text = $"Code sent to: {email}";
            VerifyBtn.Click += async (sender, args) =>
            {
                if (!int.TryParse(CodeTextBox.Text, out int code) && code is < 9_999_999 or > 100_000_000)
                {
                    await GUIHelper.DisplayInfosAsync(InfoTextBlock, "Invalid code", Brushes.Red);
                    return;
                }

                Verify verify = new() { UserId = userId, VerificationCode = code };
                APIResponse<User> response = await Http.PostAsync<Verify, User>(verify, "auth/verify", CallerInfos.Create());
                if (!response.IsSuccess)
                { 
                    await GUIHelper.DisplayInfosAsync(InfoTextBlock, "Invalid code", Brushes.Red);
                    return;
                }

                HomeWindow homeWindow  = new(response.Data);
                _ = GUIHelper.SwitchWindows<VerificationWindow, HomeWindow>(homeWindow);
            };
        }
    }
}
