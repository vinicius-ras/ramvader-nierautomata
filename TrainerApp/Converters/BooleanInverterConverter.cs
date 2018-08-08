using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrainerApp.Converters
{
	/// <summary>Converter used to invert a boolean value on a WPF binding.</summary>
	[ValueConversion( typeof( bool ), typeof( bool ) )]
	public class BooleanInverterConverter : IValueConverter
	{
		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null || value.GetType() != typeof( bool ) )
				return DependencyProperty.UnsetValue;

			return !( (bool) value );
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null || value.GetType() != typeof( bool ) )
				return DependencyProperty.UnsetValue;

			return !( (bool) value );
		}
		#endregion
	}
}
