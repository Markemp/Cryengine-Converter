﻿using CgfConverter.Renderers.USD.Attributes;
using System.Text;

namespace CgfConverter.Renderers.USD.Models;

[UsdElement("header")]
public class UsdHeader
{
    [UsdProperty("defaultPrim")]
    public string DefaultPrim { get; set; } = "root";

    [UsdProperty("doc")]
    public string Doc { get; set; } = "Generated by CgfConverter";

    [UsdProperty("metersPerUnit")]
    public int MetersPerUnit { get; set; } = 1;

    [UsdProperty("upAxis")]
    public string UpAxis { get; set; } = "Z";

    [UsdProperty("startTimeCode")]
    public int? StartTimeCode { get; set; }

    [UsdProperty("endTimeCode")]
    public int? EndTimeCode { get; set; }

    [UsdProperty("timeCodesPerSecond")]
    public int? TimeCodesPerSecond { get; set; }

    [UsdProperty("version")]
    public string Version { get; set; } = "1.0";

    public string Serialize()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"#usda {Version}");
        sb.AppendLine("(");
        sb.AppendLine($"    defaultPrim = \"{DefaultPrim}\"");
        sb.AppendLine($"    doc = \"{Doc}\"");
        sb.AppendLine($"    metersPerUnit = {MetersPerUnit}");
        sb.AppendLine($"    upAxis = \"{UpAxis}\"");
        sb.AppendLine(")");
        return sb.ToString();
    }
}
