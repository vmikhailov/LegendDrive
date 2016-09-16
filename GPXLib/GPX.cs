using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Linq;

namespace LegendDrive.Library.GPXLib
{
    /// <summary>
    /// GPX is the root element in the XML file
    /// GPX documents contain a metadata header, followed by waypoints, routes, and tracks. 
    /// You can add your own elements to the extensions section of the GPX document. 
    /// </summary>
    [XmlRoot("gpx", Namespace = "http://www.topografix.com/GPX/1/1", IsNullable = false)]
    public class GPX
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GPXLib"/> class.
        /// </summary>
        public GPX()
        {
            Version = "1.1";
            Metadata = new Metadata();
            WptList = new List<Wpt>();
            RteList = new List<Rte>();
            TrkList = new List<Trk>();
        }
        #endregion constructors

        #region public properties

        /// <summary>
        /// Gets or sets the Metadata of the file.
        /// </summary>
		public Metadata Metadata { get; set; }

        /// <summary>
        /// Gets or sets the waypoint list of the file.
        /// </summary>
        [XmlElement("wpt")]
		public List<Wpt> WptList { get; set; }

        /// <summary>
        /// Gets or sets the route list of the file.
        /// </summary>
        [XmlElement("rte")]
		public List<Rte> RteList { get; set; }

        /// <summary>
        /// Gets or sets the track list of the file.
        /// </summary>
        [XmlElement("trk")]
		public List<Trk> TrkList { get; set; }

        /// <summary>
        /// Gets or sets the version of the file.
        /// </summary>
        [XmlAttribute("version")]
		public string Version { get; set; }

        /// <summary>
        /// Gets or sets the creator of the file.
        /// </summary>
        [XmlAttribute("creator")]
		public string Creator { get; set; }

		#endregion public properties

		#region public methods
		/// <summary>
		/// This procedure add a track point to the <see cref="TrkList"/> list.
		/// The <see cref="TrkList"/>, <see cref="Trkseg"/> and the 
		/// track point list will created if there are not exists.
		/// </summary>
		/// <param name="trackName">Name of the track</param>
		/// <param name="trackSegmentId">Index of the track segment list (start at 0)</param>
		/// <param name="point">The <see cref="Wpt"/> object </param>
		public void AddTrackPoint(string trackName, int trackSegmentId, Wpt point)
		{
			var trk = TrkList.Where(x => x.Name == trackName).FirstOrDefault();
			if (trk == null)
			{
				trk = new Trk(trackName);
				TrkList.Add(trk);
			}

			trk.AddTrackPoint(trackSegmentId, point);
		}

        /// <summary>
        /// This procedure add a route point to the <see cref="RteList"/> list.
        /// The <see cref="RteList"/> and the route point list will 
        /// created if there are not exists.
        /// </summary>
        /// <param name="routeName">Name of the route</param>
        /// <param name="point">The <see cref="Wpt"/> object </param>
        public void AddRoutePoint(string routeName, Wpt point)
        {
			var rte = RteList.Where(x => x.Name == routeName).FirstOrDefault();
			rte = rte ?? new Rte(routeName);

			rte.RteptList.Add(point);
        }

		/// <summary>
		/// This procedure add a way point to the <see cref="WptList"/> list.
		/// </summary>
		/// <param name="point">The <see cref="Wpt"/> object </param>
		public void AddWayPoint(Wpt point)
		{
			WptList.Add(point);
		}

        #endregion public methods

    }
}
