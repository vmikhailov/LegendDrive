using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
	/// <summary>
	/// <c>Wpt</c> represents a waypoint, point of interest, or named feature on a map.
	/// </summary>
	public class Wpt
	{
		#region constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Wpt"/> class.
		/// </summary>
		public Wpt()
		{
			LinkList = new List<Link>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Wpt"/> class.
		/// </summary>
		/// <param name="lat">Latitude of waypoint</param>
		/// <param name="lon">Longitude of waypoint</param>
		public Wpt(decimal lat, decimal lon) : this()
		{
			Lat = lat;
			Lon = lon;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Wpt"/> class.
		/// This "private" constructor is for hiding the public constructor for 
		/// serialize / deserialize
		/// </summary>
		/// <param name="dummy">Dummy parameter</param>
		private Wpt(int dummy)
		{
		}

		#endregion constructors

		#region public properties

		/// <summary>
		/// Gets or sets Elevation (in meters) of the point.
		/// </summary>
		[XmlElement("ele")]
		public decimal Ele { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Elevation is enabled.
		/// </summary>
		[XmlIgnoreAttribute]
		public bool EleSpecified { get; set; }

		/// <summary>
		/// Gets or sets the timestamp.
		/// </summary>
		[XmlElement("time")]
		public DateTime Time { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Time is enabled.
		/// </summary>
		[XmlIgnoreAttribute]
		public bool TimeSpecified { get; set; }

		/// <summary>
		/// Gets or sets the Magnetic variation (in degrees) at the point
		/// </summary>
		public decimal Magvar { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Magnetic variation is enabled.
		/// </summary>
		[XmlIgnore]
		public bool MagvarSpecified { get; set; }

		/// <summary>
		/// Gets or sets the Height
		/// </summary>
		public decimal Geoidheight { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Height is enabled.
		/// </summary>
		[XmlIgnore]
		public bool GeoidheightSpecified { get; set; }

		/// <summary>
		/// Gets or sets the Name of the waypoint.
		/// </summary>
		[XmlElement("name")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the comment of a waypoint.
		/// </summary>
		public string Cmt { get; set; }

		/// <summary>
		/// Gets or sets the Description of a waypoint.
		/// </summary>
		public string Desc { get; set; }

		/// <summary>
		/// Gets or sets "Source of data" for a waypoint
		/// </summary>
		public string Src { get; set; }

		/// <summary>
		/// Gets or sets the LinkList.
		/// </summary>
		[XmlElement("link")]
		public List<Link> LinkList { get; set; }

		/// <summary>
		/// Gets or sets the Symbol name
		/// </summary>
		public string Sym { get; set; }

		/// <summary>
		/// Gets or sets the Type (classification) of a waypoint
		/// </summary>
		public string Type{ get; set; }

		/// <summary>
		/// Gets or sets the Fix.
		/// </summary>
		public Fix Fix { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Fix is enabled.
		/// </summary>
		[XmlIgnore]
		public bool FixSpecified { get; set; }

		/// <summary>
		/// Gets or sets the number of satellites.
		/// </summary>
		[XmlElement(DataType = "nonNegativeInteger")]
		public string Sat { get; set; }

		/// <summary>
		/// Gets or sets the Horizontal dilution of precision.
		/// </summary>
		public decimal Hdop { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Horizontal dilution is enabled.
		/// </summary>
		[XmlIgnore]
		public bool HdopSpecified { get; set; }

		/// <summary>
		/// Gets or sets Vertical dilution of precision.
		/// </summary>
		public decimal Vdop { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Vertical dilution is enabled.
		/// </summary>
		[XmlIgnore]
		public bool VdopSpecified { get; set; }

		/// <summary>
		/// Gets or sets the Position dilution of precision.
		/// </summary>
		public decimal Pdop { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Position dilution is enabled.
		/// </summary>
		[XmlIgnore]
		public bool PdopSpecified { get; set; }

		/// <summary>
		/// Gets or sets the Number of seconds since last DGPS update.
		/// </summary>
		public decimal Ageofdgpsdata { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the Number of seconds is enabled.
		/// </summary>
		[XmlIgnore]
		public bool AgeofdgpsdataSpecified { get; set; }

		/// <summary>
		/// Gets or sets the ID of DGPS station used in differential correction.
		/// </summary>
		[XmlElement(DataType = "integer")]
		public string Dgpsid { get; set; }

		/// <summary>
		/// Gets or sets the Latitude of a waypoint
		/// </summary>
		[XmlAttribute("lat")]
		public decimal Lat { get; set; }

		/// <summary>
		/// Gets or sets the Longitude of a waypoint
		/// </summary>
		[XmlAttribute("lon")]
		public decimal Lon { get; set; }

		#endregion public properties
	}
}
