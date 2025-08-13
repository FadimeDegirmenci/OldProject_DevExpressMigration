using System.Globalization;

namespace SM.MAUI.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    public class StringToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isSuccess)
                return isSuccess ? Colors.Green : Colors.Red;
            return Colors.Black;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StockQuantityToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int quantity)
            {
                if (quantity == 0) return Colors.Red;           // Stokta yok
                if (quantity <= 10) return Colors.Orange;       // Düşük stok
                if (quantity <= 50) return Colors.Blue;         // Orta stok
                return Colors.Green;                            // Yüksek stok
            }
            return Colors.Black;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // YENİ EKLENEN CONVERTER
    public class MultiplyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is decimal price && parameter != null)
                {
                    // Parameter'ı integer'a dönüştürmeye çalış
                    if (int.TryParse(parameter.ToString(), out int quantity))
                    {
                        return (price * quantity).ToString("F2");
                    }
                }

                // Eğer value double ise
                if (value is double doublePrice && parameter != null)
                {
                    if (int.TryParse(parameter.ToString(), out int quantity))
                    {
                        return ((decimal)doublePrice * quantity).ToString("F2");
                    }
                }

                return "0.00";
            }
            catch
            {
                return "0.00";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Bonus: Daha iyi bir çözüm için değer formatlama converter'ı
    public class CurrencyConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
                return decimalValue.ToString("F2") + " TL";

            if (value is double doubleValue)
                return doubleValue.ToString("F2") + " TL";

            if (value is int intValue)
                return intValue.ToString("F2") + " TL";

            return "0.00 TL";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}