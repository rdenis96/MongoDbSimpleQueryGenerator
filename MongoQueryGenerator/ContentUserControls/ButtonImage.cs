using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MongoQueryGenerator.ContentUserControls
{
    public class ButtonImage : Image
    {
        public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(SolidColorBrush), typeof(ButtonImage));

        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register("ForegroundColor", typeof(SolidColorBrush), typeof(ButtonImage));

        public SolidColorBrush BackgroundColor
        {
            get { return (SolidColorBrush)GetValue(BackgroundColorProperty); }
            set { SetValue(BackgroundColorProperty, value); }
        }

        public SolidColorBrush ForegroundColor
        {
            get { return (SolidColorBrush)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }
    }
}