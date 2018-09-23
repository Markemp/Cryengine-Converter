using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace grendgine_collada
{
	[Serializable]
	[XmlType(AnonymousType=true)]
	public partial class Grendgine_Collada_New_Param
	{
		[XmlAttribute("sid")]
		public string sID;
		
		[XmlElement(ElementName = "semantic")]
		public string Semantic;				
		
		[XmlElement(ElementName = "modifier")]
		public string Modifier;				
		
		[XmlElement("annotate")]
		public Grendgine_Collada_Annotate[] Annotate;
	
		/// <summary>
		/// The element is the type and the element text is the value or space delimited list of values
		/// </summary>
		[XmlAnyElement]
		public XmlElement[] Data;

        /// <summary>
        /// Surface is for 1.4.1 compatibility
        /// </summary>
        // ggerber 1.4.1 elements.  Surface and Sampler2D are single elements for textures.
        [XmlElement("surface")]
        public Grendgine_Collada_Surface Surface;

        /// <summary>
        /// Sampler2D is for 1.4.1 compatibility
        /// </summary>
        [XmlElement("sampler2D")]
        public Grendgine_Collada_Sampler2D Sampler2D;
	}
}

