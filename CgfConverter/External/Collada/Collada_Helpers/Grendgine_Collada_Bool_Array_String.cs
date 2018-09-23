using System;

namespace grendgine_collada
{

	public partial class Grendgine_Collada_Bool_Array_String
	{
		public bool[] Value(){
			return Grendgine_Collada_Parse_Utils.String_To_Bool(this.Value_As_String);
		}


	}
}