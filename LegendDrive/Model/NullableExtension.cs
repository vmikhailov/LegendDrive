using System;
namespace LegendDrive.Model
{
	public static class NullableExtension
	{
		public static T GetValue<T>(this T? nullableValue, T dflt = default(T))
			where T: struct
		{
			return nullableValue == null || !nullableValue.HasValue ? dflt : nullableValue.Value;
		}
	}
}
