using MessengerClient.APIResponse;
using MessengerClient.GeneratePassword;
using Microsoft.Win32;
using System.IO;
using System.Net.Mail;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MessengerClient.Windows
{
    /// <summary>
    /// Interaction logic for CreateAccWindow.xaml
    /// </summary>
    public partial class CreateAccWindow : Window
    {
        private User _user;
        private string _profilePicturePath;

        public CreateAccWindow()
        {
            InitializeComponent();
            GUIHelper.SetBasicWindowUI(this, RootGrid);

            TextBoxWithDescription emailBoxWithDescription = new(EmailTextBox, EmailTextBlock);
            TextBoxWithDescription passwordBoxWithDescription = new(PasswordTextBox, PasswordTextBlock);
            TextBoxWithDescription usernameBoxWithDescription = new(UsernameTextBox, UsernameTextBlock);
            TextBoxWithDescription biographyBoxWithDescription = new(BiographyTextBox, BiographyTextBlock, -24);
            GUIHelper.ChangeTextBlockPos(emailBoxWithDescription, passwordBoxWithDescription
                , usernameBoxWithDescription, biographyBoxWithDescription);

            InitComboBoxes();
            InitGoBackBtn();
            InitProfilPicture();
            InitRandomPasswordGenBtn();
            InitContinueBtn();
            InitSignUpBtn();
            InitUsernameBox();
            InitBiographyTextBox();

            _user = new();
            _profilePicturePath = string.Empty;
        }

        #region Init

        private void InitProfilPicture()
        {
            string absolutePath = @"C:\\Users\\Crist\\.nuget\\packages\\mpcoding.wpf.loadinganimations\\1.0.2\\contentFiles\\any\\netcoreapp3.1\\MpCodingDP.ico";
            string dynamicPath = Helper.GetDynamicPath(absolutePath);

            if (ProfilPic.Fill is ImageBrush brush)
            {
                brush.ImageSource = new BitmapImage(new Uri(dynamicPath));
                _profilePicturePath = dynamicPath;
            }

            ProfilPic.MouseLeftButtonDown += (sender, args) =>
            {
                OpenFileDialog openFileDialog = new()
                {
                    Title = "Select an image",
                    Filter = "Bilder (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() is true)
                {
                    string selectedFile = openFileDialog.FileName;
                    _profilePicturePath = selectedFile;

                    if (ProfilPic.Fill is ImageBrush brush)
                    {
                        brush.ImageSource = new BitmapImage(new Uri(selectedFile));
                    }
                }
            };
        }

        private void InitRandomPasswordGenBtn()
        {
            RandomPasswordBtn.Click += (_, _) =>
            {
                string password = PasswordGenerator.GeneratePassword();
                PasswordTextBox.Text = password;

                Clipboard.SetText(password);
                _ = GUIHelper.DisplayInfosAsync(InfoTextBlock, "Copied password to clipboard!", Brushes.Black);
            };
        }

        private void InitContinueBtn()
        {
            ContinueBtn.Click += async (_, _) =>
            {
                if (await ValidateDataStage1())
                {
                    FirstStage.Visibility = Visibility.Collapsed;
                    SecondStage.Visibility = Visibility.Visible;

                    _user = new()
                    {
                        Email = EmailTextBox.Text,
                        Password = PasswordTextBox.Text,
                        Birthday = ConvertBoxesToDate(),
                    };
                }
            };
        }

        private void InitSignUpBtn()
        {
            SignUpBtn.Click += async (_, _) =>
            {
                if (!ValidateDataStage2())
                {
                    return;
                }

                byte[] profilePicture = [];
                if (File.Exists(_profilePicturePath))
                {
                    profilePicture = File.ReadAllBytes(_profilePicturePath);
                }

                _user = _user! with
                {
                    Biography = BiographyTextBox.Text,
                    Username = UsernameTextBox.Text,
                    ProfilPicture = profilePicture,
                    TFAEnabled = TFACheckBox.IsChecked,
                };
                
                APIResponse<ulong?> aPIResponse = await Http.PostAsync<User, ulong?>(_user, "auth/create", CallerInfos.Create());
                if (aPIResponse.IsSuccess)
                {
                    VerificationWindow verificationWindow = new(_user.Email!, aPIResponse.Data.Value);
                    _ = GUIHelper.SwitchWindows<CreateAccWindow, VerificationWindow>(verificationWindow, 0);
                    return;
                }

                HandleAccCreationError(aPIResponse);
            };
        }

        private void InitComboBoxes()
        {
            for (int i = 1; i <= 31; i++)
            {
                ComboBoxItem comboBoxItem = new()
                {
                    Content = i,
                };

                _ = DayBox.Items.Add(comboBoxItem);
            }

            for (int i = 1; i <= 12; i++)
            {
                ComboBoxItem comboBoxItem = new()
                {
                    Content = i,
                };

                _ = MonthBox.Items.Add(comboBoxItem);
            }

            for (int i = DateTime.Now.Year; i >= 1960; i--)
            {
                ComboBoxItem comboBoxItem = new()
                {
                    Content = i,
                };

                _ = YearBox.Items.Add(comboBoxItem);
            }
        }

        private void InitGoBackBtn()
        {
            GoBackBtn.MouseEnter += (_, _) => Cursor = Cursors.Hand;
            GoBackBtn.MouseLeave += (_, _) => Cursor = Cursors.Arrow;

            if (FirstStage.Visibility == Visibility.Visible)
            {
                GoBackBtn.MouseLeftButtonDown += (_, _) => GUIHelper.SwitchWindows<CreateAccWindow, Login>();
                return;
            }

            GoBackBtn.MouseLeftButtonDown += (_, _) =>
            {
                SecondStage.Visibility = Visibility.Collapsed;
                FirstStage.Visibility = Visibility.Visible;
            };
        }

        private void InitUsernameBox()
        {
            const byte MaxInputLength = 19;
            UsernameTextBox.PreviewKeyDown += (sender, args) =>
            {
                if (args.Key 
                    is Key.Back
                    or Key.Delete
                    or Key.Left
                    or Key.Right)
                {
                    return;
                }

                if (UsernameTextBox.Text.Length >= MaxInputLength)
                {
                    args.Handled = true;
                }
            };

            GUIHelper.RestrictPasting(UsernameTextBox, MaxInputLength);
        }

        private void InitBiographyTextBox()
        {
            const byte MaxLines = 4;

            BiographyTextBox.AcceptsReturn = true;
            BiographyTextBox.TextWrapping = TextWrapping.NoWrap;

            BiographyTextBox.TextChanged += (sender, args) =>
            {
                TextBox tb = (TextBox)sender!;
                double maxWidth = tb.ActualWidth - 10;
                string[] inputLines = tb.Text.Replace("\r\n", "\n").Split('\n');
                List<string> outputLines = [];

                foreach (string line in inputLines)
                {
                    string current = line;
                    while (GUIHelper.ExceedsLineWidth(current, maxWidth, tb))
                    {
                        int cutIndex = current.Length;
                        while (cutIndex > 0 && GUIHelper.ExceedsLineWidth(current[..cutIndex], maxWidth, tb))
                        {
                            cutIndex--;
                        }

                        if (cutIndex == 0) 
                            break;

                        outputLines.Add(current[..cutIndex]);
                        current = current[cutIndex..];

                        if (outputLines.Count >= MaxLines)
                            break;
                    }

                    if (current.Length > 0 && outputLines.Count < MaxLines)
                        outputLines.Add(current);

                    if (outputLines.Count >= MaxLines)
                        break;
                }

                string newText = string.Join(Environment.NewLine, outputLines);
                if (tb.Text != newText)
                {
                    int caret = tb.CaretIndex;
                    tb.Text = newText;
                    tb.CaretIndex = Math.Min(caret, tb.Text.Length) +2;
                }
            };
        }

        #endregion

        private async Task<bool> ValidateDataStage1()
        {
            string? emailError = await ValidateEmail();
            if (emailError is not null)
            {
                await GUIHelper.DisplayInfosAsync(InfoTextBlock, emailError, Brushes.Red);
                return false;
            }

            if (ConvertBoxesToDate() is null)
            {
                await GUIHelper.DisplayInfosAsync(InfoTextBlock, "The entered birthday is invalid!", Brushes.Red);
                return false;
            }

            return true;
        }

        private async Task<string?> ValidateEmail()
        {
            const string errorMsg = "The entered email is invalid!";
            string email = EmailTextBox.Text;

            try
            {
                if (email.Length == 0)
                {
                     return errorMsg;
                }

                MailAddress addr = new(email);
                return email == addr.Address && await Helper.DomainHasMxRecordAsync(email) 
                    ? null 
                    : errorMsg;
            }
            catch (FormatException)
            {
                return errorMsg;
            }
        }

        private bool ValidateDataStage2()
        {
            if (UsernameTextBox.Text.Length == 0)
            {
                const string errorMsg = "Usernme is invalid!";
                _ = GUIHelper.DisplayInfosAsync(InfoTextBlock, errorMsg, Brushes.Red);
                return false;
            }

            return true;
        }

        private DateOnly? ConvertBoxesToDate()
        {
            if (DayBox.SelectedItem is ComboBoxItem dayItem 
                && MonthBox.SelectedItem is ComboBoxItem monthItem && YearBox.SelectedItem is ComboBoxItem yearItem)
            {
                if (dayItem.Content is null || monthItem.Content is null || yearItem.Content is null)
                {
                    return null;
                }

                string date = $"{dayItem.Content}-{monthItem.Content}-{yearItem.Content}";
                return DateOnly.Parse(date);
            }

            return null;
        }

        private void HandleAccCreationError(APIResponse<ulong?> aPIResponse)
        {
            foreach (APIFieldError aPIFieldError in aPIResponse.FieldErrors)
            {
                if (aPIFieldError.Field == nameof(User.Username))
                {
                    FirstStage.Visibility = Visibility.Collapsed;
                    SecondStage.Visibility = Visibility.Visible;
                }
                else if (aPIFieldError.Field == nameof(User.Email))
                {
                    SecondStage.Visibility = Visibility.Collapsed;
                    FirstStage.Visibility = Visibility.Visible;
                }

                _ = GUIHelper.DisplayInfosAsync(InfoTextBlock, aPIFieldError.Message, Brushes.Red);
            }
        }
    }
}
