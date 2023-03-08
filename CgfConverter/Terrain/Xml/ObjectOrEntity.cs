using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace CgfConverter.Terrain.Xml;

public class ObjectOrEntity
{
    [XmlIgnore] public List<string> AllAttachedModelPaths = new();
    
    [XmlAttribute(AttributeName = "Id")] public string? Id;
    
    [XmlAttribute(AttributeName = "Name")] public string? Name;

    [XmlAttribute(AttributeName = "EntityClass")]
    public string? EntityClass;

    [XmlAttribute(AttributeName = "EntityId")]
    public string? EntityId;

    [XmlAttribute(AttributeName = "EntityGuid")]
    public string? EntityGuid;

    [XmlAttribute(AttributeName = "CastShadow")]
    public string? CastShadow;

    [XmlAttribute(AttributeName = "Layer")]
    public string? Layer;

    [XmlAttribute(AttributeName = "Material")]
    public string? Material;

    [XmlAttribute(AttributeName = "ParentId")]
    public string? ParentId;

    [XmlAttribute(AttributeName = "Pos")] public string? Pos;

    [XmlAttribute(AttributeName = "LodRatio")]
    public string? LodRatio;

    [XmlAttribute(AttributeName = "ViewDistRatio")]
    public string? ViewDistRatio;

    [XmlAttribute(AttributeName = "Geometry")]
    public string? Geometry;

    [XmlAttribute(AttributeName = "Rotate")]
    public string? Rotate;

    [XmlAttribute(AttributeName = "Scale")]
    public string? Scale;

    [XmlAttribute(AttributeName = "HiddenInGame")]
    public string? HiddenInGame;

    [XmlAttribute(AttributeName = "FOV")] public string? FOV;

    [XmlAttribute(AttributeName = "AmplitudeA")]
    public string? AmplitudeA;

    [XmlAttribute(AttributeName = "AmplitudeAMult")]
    public string? AmplitudeAMult;

    [XmlAttribute(AttributeName = "FrequencyA")]
    public string? FrequencyA;

    [XmlAttribute(AttributeName = "FrequencyAMult")]
    public string? FrequencyAMult;

    [XmlAttribute(AttributeName = "NoiseAAmpMult")]
    public string? NoiseAAmpMult;

    [XmlAttribute(AttributeName = "NoiseAFreqMult")]
    public string? NoiseAFreqMult;

    [XmlAttribute(AttributeName = "TimeOffsetA")]
    public string? TimeOffsetA;

    [XmlAttribute(AttributeName = "AmplitudeB")]
    public string? AmplitudeB;

    [XmlAttribute(AttributeName = "AmplitudeBMult")]
    public string? AmplitudeBMult;

    [XmlAttribute(AttributeName = "FrequencyB")]
    public string? FrequencyB;

    [XmlAttribute(AttributeName = "FrequencyBMult")]
    public string? FrequencyBMult;

    [XmlAttribute(AttributeName = "NoiseBAmpMult")]
    public string? NoiseBAmpMult;

    [XmlAttribute(AttributeName = "NoiseBFreqMult")]
    public string? NoiseBFreqMult;

    [XmlAttribute(AttributeName = "TimeOffsetB")]
    public string? TimeOffsetB;

    [XmlAttribute(AttributeName = "Type")] public string? Type;

    [XmlAttribute(AttributeName = "Height")]
    public string? Height;

    [XmlAttribute(AttributeName = "DisplayFilled")]
    public string? DisplayFilled;

    [XmlAttribute(AttributeName = "CullDistRatio")]
    public string? CullDistRatio;

    [XmlAttribute(AttributeName = "UseInIndoors")]
    public string? UseInIndoors;

    [XmlAttribute(AttributeName = "DoubleSide")]
    public string? DoubleSide;

    [XmlAttribute(AttributeName = "Width")]
    public string? Width;

    [XmlAttribute(AttributeName = "BorderWidth")]
    public string? BorderWidth;

    [XmlAttribute(AttributeName = "VegetationMaskWidth")]
    public string? VegetationMaskWidth;

    [XmlAttribute(AttributeName = "StepSize")]
    public string? StepSize;

    [XmlAttribute(AttributeName = "TileLength")]
    public string? TileLength;

    [XmlAttribute(AttributeName = "SortPriority")]
    public string? SortPriority;

    [XmlAttribute(AttributeName = "IgnoreTerrainHoles")]
    public string? IgnoreTerrainHoles;

    [XmlAttribute(AttributeName = "AnchorType")]
    public string? AnchorType;

    [XmlAttribute(AttributeName = "AmbientColor")]
    public string? AmbientColor;

    [XmlAttribute(AttributeName = "AffectedBySun")]
    public string? AffectedBySun;

    [XmlAttribute(AttributeName = "IgnoreSkyColor")]
    public string? IgnoreSkyColor;

    [XmlAttribute(AttributeName = "IgnoreGI")]
    public string? IgnoreGI;

    [XmlAttribute(AttributeName = "SkyOnly")]
    public string? SkyOnly;

    [XmlAttribute(AttributeName = "OceanIsVisible")]
    public string? OceanIsVisible;

    [XmlAttribute(AttributeName = "UseDeepness")]
    public string? UseDeepness;

    [XmlElement(ElementName = "Properties")]
    public EntityProperties? Properties;

    public Vector3? PosValue
    {
        get => string.IsNullOrWhiteSpace(Pos)
            ? null
            : new Vector3(Pos.Split(",").Select(x => float.Parse(x.Trim())).ToArray());
        set => Pos = value == null ? null : $"{value.Value.X},{value.Value.Y},{value.Value.Z}";
    }

    public Vector3? ScaleValue
    {
        get => string.IsNullOrWhiteSpace(Scale)
            ? null
            : new Vector3(Scale.Split(",").Select(x => float.Parse(x.Trim())).ToArray());
        set => Scale = value == null ? null : $"{value.Value.X},{value.Value.Y},{value.Value.Z}";
    }

    public Quaternion? RotateValue
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Rotate))
                return null;
            var parts = Rotate.Split(",").Select(x => float.Parse(x.Trim())).ToArray();
            return new Quaternion(parts[0], parts[1], parts[2], parts[3]);
        }
        set => Rotate = value == null ? null : $"{value.Value.X},{value.Value.Y},{value.Value.Z},{value.Value.W}";
    }

    public int? IdValue
    {
        get => string.IsNullOrWhiteSpace(Id) || Id == "0" ? null : int.Parse(Id);
        set => Id = value.ToString();
    }
    
    public int? EntityIdValue
    {
        get => string.IsNullOrWhiteSpace(EntityId) || EntityId == "0" ? null : int.Parse(EntityId);
        set => EntityId = value.ToString();
    }

    public int? ParentIdValue
    {
        get => string.IsNullOrWhiteSpace(ParentId) || ParentId == "0" ? null : int.Parse(ParentId);
        set => ParentId = value.ToString();
    }
}