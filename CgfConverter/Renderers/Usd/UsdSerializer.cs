using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CgfConverter.Renderers.Usd;

public class UsdSerializer
{
    public static string Serialize(object obj)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.Append("<?");
        var myType = obj.GetType();
        IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

        foreach (var prop in props)
        {
            var propValue = prop.GetValue(obj, null);
            sb.AppendLine();
            sb.Append(@"    [" + prop.Name + "=" + propValue + "]");
        }

        sb.AppendLine();
        sb.Append("?>");
        return sb.ToString();
    }
}
