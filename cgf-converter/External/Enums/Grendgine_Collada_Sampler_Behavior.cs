using System;
namespace grendgine_collada
{
	[Serializable]
	[XmlType(Namespace="http://www.collada.org/2005/11/COLLADASchema" )]
	public enum Grendgine_Collada_Sampler_Behavior
	{

		UNDEFINED,
		CONSTANT,
		GRADIENT, 
		CYCLE,
		OSCILLATE, 
		CYCLE_RELATIVE
	}
}

