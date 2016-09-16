using System.Collections.Generic;
using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
	/// <summary>
	/// <c>Rte</c> represents route - an ordered list of waypoints representing 
	/// a series of turn points leading to a destination.
	/// </summary>
	public class Rte
    {
        #region constructors
		/// <summary>
        /// Initializes a new instance of the <see cref="Rte"/> class.
        /// </summary>
        public Rte()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Rte"/> class.
        /// </summary>
        /// <param name="routeName">Name of the route</param>
        public Rte(string routeName)
        {
            Name = routeName;
            LinkList = new List<Link>();
            RteptList = new List<Wpt>();
        }

        #endregion constructors

        #region public properties

        /// <summary>
        /// Gets or sets the Name of the route.
        /// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the Comment of the route.
		/// </summary>
		public string Cmt { get; set; }

        /// <summary>
        /// Gets or sets the Description of the route.
        /// </summary>
		public string Desc { get; set; }

        /// <summary>
        /// Gets or sets the "Source of data" of the route.
        /// </summary>
		public string Src { get; set; }

        /// <summary>
        /// Gets or sets the LinkList.
        /// </summary>
        [XmlElement("link")]
		public List<Link> LinkList { get; set; }

        /// <summary>
        /// Gets or sets the route Number.
        /// </summary>
        [XmlElement(DataType = "nonNegativeInteger")]
		public string Number { get; set; }

        /// <summary>
        /// Gets or sets the Type (classification) of the route.
        /// </summary>
		public string Type { get; set; }

        /// <summary>
        /// Gets or sets the route point list.
        /// </summary>
        [XmlElement("rtept")]
		public List<Wpt> RteptList { get; set; }

        #endregion public properties
    }
}
