using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace BeamForming
{
    [ValueConversion(typeof(double), typeof(double))]
    class MultiplyConverter : MarkupExtension, IValueConverter
    {
        public double K { get; set; } = 1;

        public MultiplyConverter() { }

        public MultiplyConverter(double k) => K = k;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToDouble(value, culture) * K;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => System.Convert.ToDouble(value, culture) / K;
    }
}
