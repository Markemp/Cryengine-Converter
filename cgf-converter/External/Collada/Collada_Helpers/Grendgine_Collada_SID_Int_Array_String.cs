using System;

namespace grendgine_collada
{
	public partial class Grendgine_Collada_SID_Int_Array_String
	{
		public int[] Value(){
			return Grendgine_Collada_Parse_Utils.String_To_Int(this.Value_As_String);
		}

	}
}

