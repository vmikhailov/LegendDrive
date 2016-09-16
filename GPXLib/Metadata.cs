using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
    /// <summary>
    /// Information about the GPX file, author, and copyright restrictions 
    /// goes in the metadata section. Providing rich, meaningful information 
    /// about your GPX files allows others to search for and use your GPS data. 
    /// </summary>
    public class Metadata
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Metadata"/> class.
        /// </summary>
        public Metadata()
        {
            LinkList= new List<Link>();
        }

        #endregion constructors

        #region public properties

        /// <summary>
        /// Gets or sets Name for the file.
        /// </summary>
		public string Name { get; set; } 

        /// <summary>
        /// Gets or sets the description for the file.
        /// </summary>
		public string Desc { get; set; }

        /// <summary>
        /// Gets or sets the Author from the file.
        /// </summary>
		//public Person Author { get; set; }

        /// <summary>
        /// Gets or sets the Copyright for the file.
        /// </summary>
		//public Copyright Copyright { get; set; } 

        /// <summary>
        /// Gets or sets the LinkList.
        /// </summary>
        [XmlElement("link")]
		public List<Link> LinkList { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp for the file.
        /// </summary>
		public DateTime Time { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Timestamp is enabled.
        /// </summary>
        [XmlIgnore]
		public bool TimeSpecified { get; set; } 

        /// <summary>
        /// Gets or sets the Keywords.
        /// </summary>
		public string Keywords { get; set; }

		/// <summary>
        /// Gets or sets the Keywords.
        /// </summary>
		//public Bounds Bounds { get; set; }

        #endregion public properties
    }
}
