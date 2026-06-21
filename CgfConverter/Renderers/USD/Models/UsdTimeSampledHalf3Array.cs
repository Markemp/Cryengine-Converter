using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents time-sampled half3 array for skeletal animation scales.
/// USD format: half3[] scales.timeSamples = { 0: [(x, y, z), ...], 1: [...], ... }
/// Note: USD SkelAnimation requires scales to be half3[] (16-bit precision).
/// </summary>
public class UsdTimeSampledHalf3Array : UsdAttribute
{
    /// <summary>
    /// Time samples mapping frame number to arrays of Vector3 for all joints.
    /// </summary>
    public SortedDictionary<float, List<Vector3>> TimeSamples { get; set; }

    public UsdTimeSampledHalf3Array(string name, SortedDictionary<float, List<Vector3>> timeSamples)
        : base(name, false)
    {
        TimeSamples = timeSamples;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);
        sb.AppendLine($"half3[] {Name}.timeSamples = {{");

        foreach (var kvp in TimeSamples)
        {
            sb.AppendIndent(indentLevel + 1);
            sb.Append(kvp.Key.ToString("0", CultureInfo.InvariantCulture));
            sb.Append(": [");

            bool first = true;
            foreach (var v in kvp.Value)
            {
                if (!first) sb.Append(", ");
                first = false;
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "({0:0.######}, {1:0.######}, {2:0.######})",
                    v.X, v.Y, v.Z);
            }

            sb.AppendLine("],");
        }

        sb.AppendIndent(indentLevel);
        sb.Append("}");

        return sb.ToString();
    }
}
