using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using CgfConverter;
namespace grendgine_collada
{

    [Serializable]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType = true)]
    // MODIFIED from original.  Using the new 1.5 schema instead of the old 1.4.1
    [System.Xml.Serialization.XmlRootAttribute(ElementName = "COLLADA", Namespace = "https://www.khronos.org/files/collada_schema_1_5", IsNullable = false)]
    //[System.Xml.Serialization.XmlRootAttribute(ElementName = "COLLADA", Namespace = "http://www.khronos.org/files/collada_schema_1_4", IsNullable = false)]
    public partial class Grendgine_Collada
    {
        [XmlAttribute("version")]
        public string Collada_Version;

        [XmlElement(ElementName = "asset")]
        public Grendgine_Collada_Asset Asset;

        #region Core Elements

        [XmlElement(ElementName = "library_animation_clips")]
        public Grendgine_Collada_Library_Animation_Clips Library_Animation_Clips;

        [XmlElement(ElementName = "library_animations")]
        public Grendgine_Collada_Library_Animations Library_Animations;

        [XmlElement(ElementName = "library_cameras")]
        public Grendgine_Collada_Library_Cameras Library_Cameras;

        [XmlElement(ElementName = "library_controllers")]
        public Grendgine_Collada_Library_Controllers Library_Controllers;

        [XmlElement(ElementName = "library_formulas")]
        public Grendgine_Collada_Library_Formulas Library_Formulas;

        [XmlElement(ElementName = "library_geometries")]
        public Grendgine_Collada_Library_Geometries Library_Geometries;

        [XmlElement(ElementName = "library_lights")]
        public Grendgine_Collada_Library_Lights Library_Lights;

        [XmlElement(ElementName = "library_nodes")]
        public Grendgine_Collada_Library_Nodes Library_Nodes;

        [XmlElement(ElementName = "library_visual_scenes")]
        public Grendgine_Collada_Library_Visual_Scenes Library_Visual_Scene;

        #endregion

        #region Physics Elements

        [XmlElement(ElementName = "library_force_fields")]
        public Grendgine_Collada_Library_Force_Fields Library_Force_Fields;

        [XmlElement(ElementName = "library_physics_materials")]
        public Grendgine_Collada_Library_Physics_Materials Library_Physics_Materials;

        [XmlElement(ElementName = "library_physics_models")]
        public Grendgine_Collada_Library_Physics_Models Library_Physics_Models;

        [XmlElement(ElementName = "library_physics_scenes")]
        public Grendgine_Collada_Library_Physics_Scenes Library_Physics_Scenes;

        #endregion

        #region FX Elements

        [XmlElement(ElementName = "library_effects")]
        public Grendgine_Collada_Library_Effects Library_Effects;

        [XmlElement(ElementName = "library_materials")]
        public Grendgine_Collada_Library_Materials Library_Materials;

        [XmlElement(ElementName = "library_images")]
        public Grendgine_Collada_Library_Images Library_Images;

        #endregion

        #region Kinematics

        [XmlElement(ElementName = "library_articulated_systems")]
        public Grendgine_Collada_Library_Articulated_Systems Library_Articulated_Systems;

        [XmlElement(ElementName = "library_joints")]
        public Grendgine_Collada_Library_Joints Library_Joints;

        [XmlElement(ElementName = "library_kinematics_models")]
        public Grendgine_Collada_Library_Kinematics_Models Library_Kinematics_Models;

        [XmlElement(ElementName = "library_kinematics_scenes")]
        public Grendgine_Collada_Library_Kinematics_Scene Library_Kinematics_Scene;

        [XmlElement(ElementName = "scene")]
        public Grendgine_Collada_Scene Scene;

        [XmlElement(ElementName = "extra")]
        public Grendgine_Collada_Extra[] Extra;

        #endregion
		
        public Grendgine_Collada()
        {
            Collada_Version = "1.5";
            Asset = new Grendgine_Collada_Asset();
            Asset.Title = "Test Engine 1";
            Library_Visual_Scene = new Grendgine_Collada_Library_Visual_Scenes();
        }

        public static Grendgine_Collada Grendgine_Load_File(string file_name)
        {
            try
            {
                Grendgine_Collada col_scenes = null;

                XmlSerializer sr = new XmlSerializer(typeof(Grendgine_Collada));
                TextReader tr = new StreamReader(file_name);
                col_scenes = (Grendgine_Collada)(sr.Deserialize(tr));

                tr.Close();

                return col_scenes;
            }
            catch (Exception ex)
            {
                Utils.Log(LogLevelEnum.Error, ex.ToString());
                Console.ReadLine();
                return null;
            }			
		}

        //public async static void Grendgine_Save_File(Grendgine_Collada data)
        //{
        //    FileSavePicker savePicker = new FileSavePicker();
        //    savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        //    savePicker.FileTypeChoices.Add("DAE", new List<string>() { ".dae" });
        //    savePicker.FileTypeChoices.Add("Xml", new List<string>() { ".xml" });
        //    savePicker.SuggestedFileName = "Test";
        //    StorageFile file = await savePicker.PickSaveFileAsync();
        //    if (file != null)
        //    {
        //        IRandomAccessStream sessionRandomAccess = await file.OpenAsync(FileAccessMode.ReadWrite);
        //        IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
        //        try
        //        {
        //            XmlSerializer serializer = new XmlSerializer(typeof(Grendgine_Collada));
        //            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), data);
        //        }
        //        catch (System.InvalidOperationException e)
        //        {
        //            Debug.WriteLine(e.InnerException);
        //        }
        //        sessionRandomAccess.Dispose();
        //        await sessionOutputStream.FlushAsync();
        //        sessionOutputStream.Dispose();
        //    }
        //}
    }
}