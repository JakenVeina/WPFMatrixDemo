using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace WPFMatrixDemo
{
    /// <summary>
    /// Provides methods for conversion and validation between <see cref="Int32"/> values and <see cref="String"/> values.
    /// </summary>
    public class Int32ToStringValueConverterAndValidationRule : ValidationRule, IValueConverter
    {
        /**********************************************************************/
        #region Properties

        /// <summary>
        /// The minimum allowable integer value of an <see cref="Int32"/> or <see cref="String"/> being converted or validated.
        /// Defaults to <see cref="Int32.MinValue"/>.
        /// </summary>
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (value > _maximum)
                    throw new ArgumentException($"Cannot be greater than {nameof(Maximum)} ({_maximum})", nameof(Minimum));

                _minimum = value;
            }
        }
        private int _minimum = int.MinValue;

        /// <summary>
        /// The maximum allowable integer value of an <see cref="Int32"/> or <see cref="String"/> being converted or validated.
        /// Defaults to <see cref="Int32.MaxValue"/>.
        /// </summary>
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (value < _minimum)
                    throw new ArgumentException($"Cannot be less than {nameof(Minimum)} ({_minimum})", nameof(Maximum));

                _maximum = value;
            }
        }
        private int _maximum = int.MaxValue;

        #endregion Properties

        /**********************************************************************/
        #region IValueConverter

        /// <summary>
        /// See <see cref="IValueConverter.Convert"/>.
        /// Performs <see cref="Int32"/> to <see cref="String"/> and <see cref="String"/> to <see cref="Int32"/> conversions.
        /// See <see cref="Int32.ToString(IFormatProvider)"/> and <see cref="Int32.TryParse(string, NumberStyles, IFormatProvider, out int)"/> for more details.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws if targetType is null.</exception>
        /// <exception cref="ArgumentException">
        /// Throws if a <see cref="String"/> to <see cref="Int32"/> conversion is requested
        /// and the string representation of value is not a valid integer.</exception>
        /// <exception cref="ArgumentException">
        /// Throws if the <see cref="Int32"/> value of the conversion is greater than <see cref="Minimum"/> or less than <see cref="Maximum"/>.
        /// and the string representation of value is not a valid integer.</exception>
        /// <exception cref="NotSupportedException">
        /// Throws if the requested conversion (as defined by value and targetType)
        /// is not a <see cref="Int32"/> to <see cref="String"/> or <see cref="String"/> to <see cref="Int32"/> conversion.
        /// </exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int intResult;

            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            if (culture == null)
                culture = CultureInfo.CurrentUICulture;

            if ((value is int) && targetType.IsAssignableFrom(typeof(string)))
            {
                intResult = (int)value;

                if ((intResult < Minimum) || (intResult > Maximum))
                    throw new ArgumentOutOfRangeException(nameof(value), value, MakeOutOfRangeErrorMessage(Minimum, Maximum));

                return ((int)value).ToString(culture);
            }
            else if (targetType.IsAssignableFrom(typeof(int)))
            {
                var str = (value as string) ?? value?.ToString();

                if (!int.TryParse(str, NumberStyles.Any, culture, out intResult))
                    throw new ArgumentException(MakeConversionErrorMessage(str), nameof(value));

                if ((intResult < Minimum) || (intResult > Maximum))
                    throw new ArgumentOutOfRangeException(nameof(value), value, MakeOutOfRangeErrorMessage(Minimum, Maximum));

                return intResult;
            }

            throw new NotSupportedException($"Cannot convert {value?.GetType().Name ?? "null"} to {targetType.Name}");
        }

        /// <summary>
        /// Same as <see cref="Convert"/>.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Convert(value, targetType, parameter, culture);

        #endregion IValueConverter

        /**********************************************************************/
        #region ValidationRule Overrides

        /// <summary>
        /// See <see cref="ValidationRule.Validate(object, CultureInfo)"/> and <see cref="Convert"/>.
        /// </summary>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            int intResult;

            if (value is int)
            {
                intResult = (int)value;

                if ((intResult < Minimum) || (intResult > Maximum))
                    return new ValidationResult(false, MakeOutOfRangeErrorMessage(Minimum, Maximum));

                return new ValidationResult(true, null);
            }

            var str = (value as string) ?? value?.ToString();
            if (cultureInfo == null)
                cultureInfo = CultureInfo.CurrentUICulture;

            if (!int.TryParse(str, NumberStyles.Any, cultureInfo, out intResult))
                return new ValidationResult(false, MakeConversionErrorMessage(str));

            if ((intResult < Minimum) || (intResult > Maximum))
                return new ValidationResult(false, MakeOutOfRangeErrorMessage(Minimum, Maximum));

            return new ValidationResult(true, null);
        }

        #endregion ValidationRule Overrides

        /**********************************************************************/
        #region Private Methods

        private static string MakeConversionErrorMessage(string invalidString)
            => $"\"{invalidString ?? "null"}\" is not a valid integer";

        private static string MakeOutOfRangeErrorMessage(int min, int max)
            => $"Must be within the range of {min} and {max}";

        #endregion Private Methods
    }
}
