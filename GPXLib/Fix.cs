using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
	/// <summary>
	/// Types of GPS fix. none means GPS had no fix. To signify 
	/// "the fix info is unknown, leave out fixType entirely. <c>pps</c> = military signal used 
	/// </summary>
	public enum Fix
	{
		/// <summary>
		/// Represents none.
		/// </summary>
		none,

		/// <summary>
		/// Represents a 2d item.
		/// </summary>
		[XmlEnum("2d")]
		Item2d,

		/// <summary>
		/// Represents a 3d item.
		/// </summary>
		[XmlEnum("3d")]
		Item3d,

		/// <summary>
		/// Represents a <c>dgps</c> item.
		/// </summary>
		dgps,

		/// <summary>
		/// Represents a <c>pps</c> item.
		/// </summary>
		pps,
	}
}
