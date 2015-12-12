using System;

namespace grendgine_collada
{
	public partial class Grendgine_Collada_Common_Float2_Or_Param_Type
	{
		public float[] Value(){
			return Grendgine_Collada_Parse_Utils.String_To_Float(this.Value_As_String);
		}
	}
}
