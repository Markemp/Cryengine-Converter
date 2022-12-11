using System.Text;

namespace CgfConverter.Renderers.Usd.UsdCore;

public class UsdMetadata
{
    /* As of Blender 3.4
	 *  #usda 1.0
		(
			doc = "Blender v3.4.0"
			endTimeCode = 0
			metersPerUnit = 1
			startTimeCode = 0
			timeCodesPerSecond = 24
			upAxis = "Z"
		)
	 */
    public char UpAxis;
	public int? StartTimeCode;
	public int? EndTimeCode;
	public string? Doc;
	public int? TimeCodesPerSecond;

	public UsdMetadata(char upAxis = 'Y')
	{
		UpAxis = upAxis;
	}

	public StringBuilder Generate(StringBuilder sb)
	{
		sb.Append($"#usda 1.0\n(\n\t{UpAxis.ToString()}\n)\n");
		
		return sb;
	}
}
