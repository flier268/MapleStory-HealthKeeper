using MapleStory_HealthKeeper.Model;

namespace MapleStory_HealthKeeper.Converter
{
    public class KeyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                FullKeyInfo key => key.Key.ToString(),
                _ => throw new ArgumentOutOfRangeException(parameter.ToString(), "Convert type"),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}