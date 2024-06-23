using System.Text;

namespace Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder CleanNumbers(this StringBuilder sb)
    {
        sb.Replace("0.0000000", "0");
        sb.Replace("-0.0000000", "0");
        sb.Replace("1.0000000", "1");
        sb.Replace("-1.0000000", "-1");

        return sb;
    }

    public static StringBuilder AppendIndent(this StringBuilder sb, int indentLevel)
        => sb.Append(new string(' ', indentLevel * 4));
}
