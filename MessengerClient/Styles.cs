using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MessengerClient
{
    public static class Styles
    {
        public static Style CreateWindowInteractionBtnStyle()
        {
            Style style = new(typeof(Button));

            style.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
            style.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a6a6a6"))));
            style.Setters.Add(new Setter(Control.FontSizeProperty, 18.0));
            style.Setters.Add(new Setter(Control.HorizontalAlignmentProperty, HorizontalAlignment.Right));

            ControlTemplate template = new(typeof(Button));

            FrameworkElementFactory contentPresenterFactory = new(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);

            contentPresenterFactory.SetBinding(ContentPresenter.MarginProperty, new Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });

            template.VisualTree = contentPresenterFactory;

            Trigger mouseOverTrigger = new()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = true
            };

            Trigger mouseOutTrigger = new()
            {
                Property = UIElement.IsMouseOverProperty,
                Value = false
            };

            mouseOverTrigger.Setters.Add(new Setter(Control.OpacityProperty, 0.5));
            mouseOverTrigger.Setters.Add(new Setter(Control.CursorProperty, Cursors.Hand));

            mouseOutTrigger.Setters.Add(new Setter(Control.OpacityProperty, 0));
            mouseOutTrigger.Setters.Add(new Setter(Control.CursorProperty, Cursors.Arrow));

            template.Triggers.Add(mouseOverTrigger);

            style.Setters.Add(new Setter(Control.TemplateProperty, template));

            return style;
        }
    }
}
