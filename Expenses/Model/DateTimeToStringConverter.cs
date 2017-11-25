using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Expenses.Model
{
    public class DateTimeToStringConverter:IValueConverter
    {
        private string format = "dd.MM.yyyy HH:mm:ss";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) // from date to string
        {
            DateTime dateValue = (DateTime) value;
            return dateValue.ToString(format);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) // from sting to datetime
        {
           string dateString = value.ToString();

            var res = DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);
            return res ? dateTime : DateTime.Now;
        }
    }
}
