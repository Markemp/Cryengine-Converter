using System;
using System.Xml.Serialization;

namespace grendgine_collada
{
	[Serializable]
	[XmlType(Namespace="http://www.collada.org/2005/11/COLLADASchema" )]
	public enum Grendgine_Collada_Face
	{
		POSITIVE_X, 
		NEGATIVE_X, 
		POSITIVE_Y, 
		NEGATIVE_Y, 
		POSITIVE_Z, 
		NEGATIVE_Z		
		
	}
}

