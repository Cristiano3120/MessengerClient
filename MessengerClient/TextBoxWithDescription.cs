using System.Windows.Controls;

namespace MessengerClient
{
    internal readonly record struct TextBoxWithDescription
    {
        public TextBoxWithDescription(TextBox textBox, TextBlock textBlock, sbyte offsetY = 6)
        {
            TextBox = textBox;
            TextBlock = textBlock;
            OffsetY = offsetY;
        }

        public TextBox TextBox { get; init; }
        public TextBlock TextBlock { get; init; }
        public sbyte OffsetY { get; init; } = 6;
    }
}
