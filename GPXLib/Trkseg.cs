using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
    /// <summary>
    /// A Track Segment holds a list of Track Points which are logically 
    /// connected in order. To represent a single GPS track where GPS reception was lost, 
    /// or the GPS receiver was turned off, start a new Track Segment for each 
    /// continuous span of track data. 
    /// </summary>
    public class Trkseg
    {
        #region contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Trkseg"/> class.
        /// </summary>
        public Trkseg()
        {
            TrkptList = new List<Wpt>();
        }

        #endregion contructors

        #region public properties

        /// <summary>
        /// Gets or sets the track point list.
        /// </summary>
        [XmlElement("trkpt")]
		public List<Wpt> TrkptList { get; set; }

        #endregion public properties
    }
}
