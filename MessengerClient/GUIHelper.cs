using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace MessengerClient
{
    internal static class GUIHelper
    {
        private static CancellationTokenSource? _errorCts;
        public static void SetBasicWindowUI(Window window, Grid parentGrid)
        {
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            WindowChrome windowChrome = new()
            {
                CaptionHeight = 0,
                CornerRadius = new CornerRadius(42),
                GlassFrameThickness = new Thickness(0),
                ResizeBorderThickness = new Thickness(3),
                UseAeroCaptionButtons = false,
            };
            window.SetValue(WindowChrome.WindowChromeProperty, windowChrome);

            StackPanel stackPanel = CreateWindowinteractionBtns(parentGrid);
            CreateMoveGrid(window, parentGrid, stackPanel);
            CreateBorder(parentGrid);
        }

        private static StackPanel CreateWindowinteractionBtns(Grid parentGrid)
        {
            Thickness thickness = new(0);
            const byte fontSize = 18;
            const byte height = 30;
            const byte width = 30;

            Style style = Styles.CreateWindowInteractionBtnStyle();

            StackPanel stackPanel = new()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Orientation = Orientation.Horizontal,
                Background = Brushes.Transparent,
                Width = width * 3,
                Height = height,
            };

            Button closeBtn = new()
            {
                BorderThickness = thickness,
                FontSize = fontSize - 2,
                Padding = new(0, 3.5, 7.5, 0),
                Height = height,
                Width = width,
                Content = "X",
                Style = style,
            };

            Button maximizeBtn = new()
            {
                BorderThickness = thickness,
                FontSize = fontSize,
                Height = height,
                Width = width,
                Content = "🗖",
                Style = style,
            };

            Button minimizeBtn = new()
            {
                BorderThickness = thickness,
                FontSize = fontSize - 6,
                FontWeight = FontWeights.Bold,
                Padding = new(0, 3, 0, 0),
                Height = height,
                Width = width,
                Content = "―",
                Style = style,
            };

            closeBtn.Click += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window.GetWindow((Button)sender).Close();
                });
            };

            maximizeBtn.Click += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window window = Window.GetWindow((Button)sender);
                    switch (window.WindowState)
                    {
                        case WindowState.Maximized:
                            window.WindowState = WindowState.Normal;
                            break;
                        case WindowState.Normal
                            when window.MaxHeight != window.Height && window.MaxWidth != window.Width:
                            window.WindowState = WindowState.Maximized;
                            break;
                    }
                });
            };

            minimizeBtn.Click += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Window.GetWindow((Button)sender).WindowState = WindowState.Minimized;
                });
            };

            stackPanel.Children.Add(minimizeBtn);
            stackPanel.Children.Add(maximizeBtn);
            stackPanel.Children.Add(closeBtn);

            parentGrid.Children.Add(stackPanel);
            return stackPanel;
        }

        public static void ChangeTextBlockPos(params TextBoxWithDescription[] textBoxWithDescriptions)
        {
            foreach (TextBoxWithDescription textBoxWithDescription in textBoxWithDescriptions)
            {
                Thickness margin = textBoxWithDescription.TextBlock.Margin;
                textBoxWithDescription.TextBox.GotFocus += (sender, args) 
                    => textBoxWithDescription.TextBlock.Margin = new Thickness(0, 0, margin.Right, margin.Bottom + textBoxWithDescription.OffsetY);

                textBoxWithDescription.TextBox.LostFocus += (sender, args) 
                    => textBoxWithDescription.TextBlock.Margin = margin;
            }
        }

        public static async Task DisplayInfosAsync(
            TextBlock infoTextBlock,
            string msg,
            Brush color,
            ushort showTime = 3000)
        {
            _errorCts?.Cancel();
            _errorCts = new CancellationTokenSource();
            CancellationToken token = _errorCts.Token;

            infoTextBlock.Foreground = color;
            infoTextBlock.Text = msg;
            infoTextBlock.Visibility = Visibility.Visible;

            try
            {
                await Task.Delay(showTime, token);
                infoTextBlock.Visibility = Visibility.Hidden;
            }
            catch (TaskCanceledException) { }
        }

        public static bool ExceedsLineWidth(string line, double maxWidth, TextBox tb)
        {
            FormattedText formattedText = new(
                line,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch),
                tb.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(tb).PixelsPerDip);

            return formattedText.Width > maxWidth;
        }

        private static void CreateMoveGrid(Window window, Grid parentGrid, StackPanel stackPanel)
        {
            Grid moveGrid = new()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = window.Width - stackPanel.Width,
                Background = Brushes.Transparent,
                Height = stackPanel.Height,
            };

            window.SizeChanged += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    moveGrid.Width = args.NewSize.Width - stackPanel.Width;
                });
            };

            moveGrid.PreviewMouseLeftButtonDown += (sender, args) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.DragMove();
                });
            };

            parentGrid.Children.Add(moveGrid);
        }

        private static void CreateBorder(Grid parentGrid)
        {
            Border border = new()
            {
                CornerRadius = new CornerRadius(20),
                BorderBrush = HexToBrush("#2e2d2e"),
                BorderThickness = new Thickness(2)
            };

            parentGrid.Children.Add(border);
        }

        public static SolidColorBrush HexToBrush(string value)
        {
            object brush = new BrushConverter().ConvertFromString(value)
                ?? throw new ArgumentException($"{nameof(value)} was an invalid HEX-Color");

            return (SolidColorBrush)brush;
        }

        public static TWindowToOpen SwitchWindows<TWindowToClose, TWindowToOpen>(TWindowToOpen windowToOpen, ushort switchDelay = 300)
            where TWindowToClose : Window where TWindowToOpen : Window
        {
            _ = Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                windowToOpen.Show();

                await Task.Delay(switchDelay);

                foreach (Window window in Application.Current.Windows)
                {
                    if (typeof(TWindowToClose).IsAssignableFrom(window.GetType()))
                    {
                        window.Close();
                        break;
                    }
                }
            });

            return windowToOpen;
        }

        public static TWindowToOpen SwitchWindows<TWindowToClose, TWindowToOpen>(ushort switchDelay = 300)
            where TWindowToClose : Window where TWindowToOpen : Window, new()
            => SwitchWindows<TWindowToClose, TWindowToOpen>(new TWindowToOpen(), switchDelay);

        public static void CloseAllWindowsExcept<TWindowToKeep>(TWindowToKeep windowToKeep)
            where TWindowToKeep : Window
        {
            _ = Application.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (windowToKeep != window)
                    {
                        window.Close();
                    }
                }
            });
        }

        public static void RestrictPasting(TextBox textBox, byte maxInputLength)
        {
            CommandManager.AddPreviewExecutedHandler(textBox, (sender, args) =>
            {
                if (args.Command == ApplicationCommands.Paste)
                {
                    string? clipboardText = Clipboard.GetText();

                    if (!string.IsNullOrEmpty(clipboardText))
                    {
                        string newText = string.Concat(textBox.Text.AsSpan(0, textBox.SelectionStart), clipboardText, textBox.Text.AsSpan(textBox.SelectionStart + textBox.SelectionLength));

                        if (newText.Length > maxInputLength)
                        {
                            args.Handled = true;
                        }
                    }
                }
            });
        }
    }
}
