using System.Collections.Generic;
using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
	/// <summary>
	/// <c>Trk</c> represents a track - an ordered list of points describing a path. 
	/// </summary>
	public class Trk
    {
        #region constructors
		/// <summary>
        /// Initializes a new instance of the <see cref="Trk"/> class.
        /// </summary>
        public Trk()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Trk"/> class.
        /// </summary>
        /// <param name="trackName">Name of the track</param>
        public Trk(string trackName)
        {
            Name = trackName;
            LinkList = new List<Link>();
            TrksegList = new List<Trkseg>();
        }

		public void AddTrackPoint(int trackSegmentId, Wpt point)
		{
			while(TrksegList.Count <= trackSegmentId)
			{
				TrksegList.Add(new Trkseg());
			}

			var seg = TrksegList[trackSegmentId];
			seg.TrkptList.Add(point);
		}

        #endregion constructors

        #region public properties

        /// <summary>
        /// Gets or sets the Name of the route.
        /// </summary>
        [XmlElement("name")]
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
        /// Gets or sets the track segment point list.
        /// </summary>
        [XmlElement("trkseg")]
		public List<Trkseg> TrksegList { get; set; }

        #endregion public properties
    }
}
