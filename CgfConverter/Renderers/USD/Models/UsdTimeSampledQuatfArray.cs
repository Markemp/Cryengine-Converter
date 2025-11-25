using CgfConverter.Renderers.USD.Attributes;
using Extensions;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

/// <summary>
/// Represents time-sampled quaternion array for skeletal animation rotations.
/// USD format: quatf[] rotations.timeSamples = { 0: [(w, x, y, z), ...], 0.033: [...], ... }
/// Note: USD quaternions are stored as (real, i, j, k) = (w, x, y, z)
/// </summary>
public class UsdTimeSampledQuatfArray : UsdAttribute
{
    /// <summary>
    /// Time samples mapping frame time (in seconds) to arrays of quaternions for all joints.
    /// </summary>
    public SortedDictionary<float, List<Quaternion>> TimeSamples { get; set; }

    public UsdTimeSampledQuatfArray(string name, SortedDictionary<float, List<Quaternion>> timeSamples)
        : base(name, false)
    {
        TimeSamples = timeSamples;
    }

    public override string Serialize(int indentLevel)
    {
        var sb = new StringBuilder();
        sb.AppendIndent(indentLevel);
        sb.AppendLine($"quatf[] {Name}.timeSamples = {{");

        foreach (var kvp in TimeSamples)
        {
            sb.AppendIndent(indentLevel + 1);
            sb.Append(kvp.Key.ToString("G", CultureInfo.InvariantCulture));
            sb.Append(": [");

            bool first = true;
            foreach (var q in kvp.Value)
            {
                if (!first) sb.Append(", ");
                first = false;
                // USD quaternion format: (real, i, j, k) = (w, x, y, z)
                sb.AppendFormat(CultureInfo.InvariantCulture,
                    "({0:G}, {1:G}, {2:G}, {3:G})",
                    q.W, q.X, q.Y, q.Z);
            }

            sb.AppendLine("],");
        }

        sb.AppendIndent(indentLevel);
        sb.Append("}");

        return sb.ToString();
    }
}
