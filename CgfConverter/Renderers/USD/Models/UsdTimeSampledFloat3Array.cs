using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents time-sampled float3 array for skeletal animation translations.
/// USD format: float3[] translations.timeSamples = { 0: [(x, y, z), ...], 0.033: [...], ... }
/// </summary>
public class UsdTimeSampledFloat3Array : UsdAttribute
{
    /// <summary>
    /// Time samples mapping frame time (in seconds) to arrays of Vector3 for all joints.
    /// </summary>
    public SortedDictionary<float, List<Vector3>> TimeSamples { get; set; }

    public UsdTimeSampledFloat3Array(string name, SortedDictionary<float, List<Vector3>> timeSamples)
        : base(name, false)
    {
        TimeSamples = timeSamples;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);
        sb.AppendLine($"float3[] {Name}.timeSamples = {{");

        foreach (var kvp in TimeSamples)
        {
            sb.AppendIndent(indentLevel + 1);
            sb.Append(kvp.Key.ToString("G", CultureInfo.InvariantCulture));
            sb.Append(": [");

            bool first = true;
            foreach (var v in kvp.Value)
            {
                if (!first) sb.Append(", ");
                first = false;
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "({0:G}, {1:G}, {2:G})",
                    v.X, v.Y, v.Z);
            }

            sb.AppendLine("],");
        }

        sb.AppendIndent(indentLevel);
        sb.Append("}");

        return sb.ToString();
    }
}
