using System.Xml.Serialization;

namespace LegendDrive.Library.GPXLib
{
	/// <summary>
	/// A link to an external resource (Web page, digital photo, video clip, etc) 
	/// with additional information. 
	/// </summary>
	public class Link
    {
        #region public properties

        /// <summary>
        /// Gets or sets Text from link.
        /// </summary>
		public string Text { get; set; }

        /// <summary>
        /// Gets or sets Type from link.
        /// </summary>
		public string Type { get; set; }

        /// <summary>
        /// Gets or sets <c>Href</c> from link.
        /// </summary>
		[XmlAttribute(DataType = "anyURI")]
		public string Href { get; set; }

        #endregion public properties
    }
}
