using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CgfConverter.Collada;

[Serializable]
[System.Diagnostics.DebuggerStepThrough()]
[System.ComponentModel.DesignerCategory("code")]
[XmlType(AnonymousType = true)]
//[System.Xml.Serialization.XmlRootAttribute(ElementName = "Collada", Namespace = "https://www.khronos.org/files/collada_schema_1_5", IsNullable = false)]
//[System.Xml.Serialization.XmlRootAttribute(ElementName = "Collada", Namespace = "http://www.khronos.org/files/collada_schema_1_4", IsNullable = false)]
[System.Xml.Serialization.XmlRootAttribute(ElementName = "COLLADA", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
public partial class ColladaDoc
{
    [XmlAttribute("version")]
    public string Collada_Version;

    [XmlElement(ElementName = "asset")]
    public ColladaAsset Asset;

    #region FX Elements
    [XmlElement(ElementName = "library_images")]
    public ColladaLibraryImages Library_Images;

    [XmlElement(ElementName = "library_effects")]
    public ColladaLibraryEffects Library_Effects;

    [XmlElement(ElementName = "library_materials")]
    public ColladaLibraryMaterials Library_Materials;

    #endregion

    #region Core Elements


    [XmlElement(ElementName = "library_animations")]
    public ColladaLibraryAnimations Library_Animations;

    [XmlElement(ElementName = "library_animation_clips")]
    public ColladaLibraryAnimationClips Library_Animation_Clips;

    [XmlElement(ElementName = "library_cameras")]
    public ColladaLibraryCameras Library_Cameras;

    [XmlElement(ElementName = "library_controllers")]
    public ColladaLibraryControllers Library_Controllers;

    [XmlElement(ElementName = "library_formulas")]
    public ColladaLibraryFormulas Library_Formulas;

    [XmlElement(ElementName = "library_geometries")]
    public ColladaLibraryGeometries Library_Geometries;

    [XmlElement(ElementName = "library_lights")]
    public ColladaLibraryLights Library_Lights;

    [XmlElement(ElementName = "library_nodes")]
    public ColladaLibraryNodes Library_Nodes;

    [XmlElement(ElementName = "library_visual_scenes")]
    public ColladaLibraryVisualScenes Library_Visual_Scene;

    #endregion

    #region Physics Elements

    [XmlElement(ElementName = "library_force_fields")]
    public ColladaLibraryForceFields Library_Force_Fields;

    [XmlElement(ElementName = "library_physics_materials")]
    public ColladaLibraryPhysicsMaterials Library_Physics_Materials;

    [XmlElement(ElementName = "library_physics_models")]
    public ColladaLibraryPhysicsModels Library_Physics_Models;

    [XmlElement(ElementName = "library_physics_scenes")]
    public ColladaLibraryPhysicsScenes Library_Physics_Scenes;

    #endregion

    #region Kinematics

    [XmlElement(ElementName = "library_articulated_systems")]
    public ColladaLibraryArticulatedSystems Library_Articulated_Systems;

    [XmlElement(ElementName = "library_joints")]
    public ColladaLibraryJoints Library_Joints;

    [XmlElement(ElementName = "library_kinematics_models")]
    public ColladaLibraryKinematicsModels Library_Kinematics_Models;

    [XmlElement(ElementName = "library_kinematics_scenes")]
    public ColladaLibraryKinematicsScene Library_Kinematics_Scene;

    [XmlElement(ElementName = "scene")]
    public ColladaScene Scene;

    [XmlElement(ElementName = "extra")]
    public ColladaExtra[] Extra;

    #endregion

    public ColladaDoc()
    {
        Collada_Version = "1.5";
        Asset = new ColladaAsset();
        Asset.Title = "Test Engine 1";
        Library_Visual_Scene = new ColladaLibraryVisualScenes();
    }

    public static ColladaDoc Grendgine_Load_File(string file_name)
    {
        try
        {
            ColladaDoc col_scenes = null;

            XmlSerializer sr = new XmlSerializer(typeof(ColladaDoc));
            TextReader tr = new StreamReader(file_name);
            col_scenes = (ColladaDoc)(sr.Deserialize(tr));

            tr.Close();

            return col_scenes;
        }
        catch (Exception ex)
        {
            Utilities.Log(LogLevelEnum.Error, ex.ToString());
            Console.ReadLine();
            return null;
        }
    }
}
