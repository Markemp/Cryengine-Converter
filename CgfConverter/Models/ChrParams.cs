namespace CgfConverter.Models
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Params
    {
        private ParamsAnimationList animationListField;

        private ParamsIK_Definition iK_DefinitionField;

        /// <remarks/>
        public ParamsAnimationList AnimationList
        {
            get
            {
                return this.animationListField;
            }
            set
            {
                this.animationListField = value;
            }
        }

        /// <remarks/>
        public ParamsIK_Definition IK_Definition
        {
            get
            {
                return this.iK_DefinitionField;
            }
            set
            {
                this.iK_DefinitionField = value;
            }
        }



    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsAnimationList
    {

        private object[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Animation", typeof(ParamsAnimationListAnimation))]
        [System.Xml.Serialization.XmlElementAttribute("Comment", typeof(ParamsAnimationListComment))]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsAnimationListAnimation
    {

        private string nameField;

        private string pathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsAnimationListComment
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsIK_Definition
    {

        private ParamsIK_DefinitionIK[] limbIK_DefinitionField;

        private ParamsIK_DefinitionFeetLock_Definition feetLock_DefinitionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("IK", IsNullable = false)]
        public ParamsIK_DefinitionIK[] LimbIK_Definition
        {
            get
            {
                return this.limbIK_DefinitionField;
            }
            set
            {
                this.limbIK_DefinitionField = value;
            }
        }

        /// <remarks/>
        public ParamsIK_DefinitionFeetLock_Definition FeetLock_Definition
        {
            get
            {
                return this.feetLock_DefinitionField;
            }
            set
            {
                this.feetLock_DefinitionField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsIK_DefinitionIK
    {

        private string endEffectorField;

        private string handleField;

        private string rootField;

        private string solverField;

        private string j0Field;

        private string j1Field;

        private string j2Field;

        private string j3Field;

        private string j4Field;

        private byte fStepSizeField;

        private bool fStepSizeFieldSpecified;

        private byte fThresholdField;

        private bool fThresholdFieldSpecified;

        private byte nMaxInterationField;

        private bool nMaxInterationFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EndEffector
        {
            get
            {
                return this.endEffectorField;
            }
            set
            {
                this.endEffectorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Handle
        {
            get
            {
                return this.handleField;
            }
            set
            {
                this.handleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Root
        {
            get
            {
                return this.rootField;
            }
            set
            {
                this.rootField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Solver
        {
            get
            {
                return this.solverField;
            }
            set
            {
                this.solverField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string J0
        {
            get
            {
                return this.j0Field;
            }
            set
            {
                this.j0Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string J1
        {
            get
            {
                return this.j1Field;
            }
            set
            {
                this.j1Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string J2
        {
            get
            {
                return this.j2Field;
            }
            set
            {
                this.j2Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string J3
        {
            get
            {
                return this.j3Field;
            }
            set
            {
                this.j3Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string J4
        {
            get
            {
                return this.j4Field;
            }
            set
            {
                this.j4Field = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte fStepSize
        {
            get
            {
                return this.fStepSizeField;
            }
            set
            {
                this.fStepSizeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fStepSizeSpecified
        {
            get
            {
                return this.fStepSizeFieldSpecified;
            }
            set
            {
                this.fStepSizeFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte fThreshold
        {
            get
            {
                return this.fThresholdField;
            }
            set
            {
                this.fThresholdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool fThresholdSpecified
        {
            get
            {
                return this.fThresholdFieldSpecified;
            }
            set
            {
                this.fThresholdFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte nMaxInteration
        {
            get
            {
                return this.nMaxInterationField;
            }
            set
            {
                this.nMaxInterationField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool nMaxInterationSpecified
        {
            get
            {
                return this.nMaxInterationFieldSpecified;
            }
            set
            {
                this.nMaxInterationFieldSpecified = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsIK_DefinitionFeetLock_Definition
    {

        private ParamsIK_DefinitionFeetLock_DefinitionRIKHandle rIKHandleField;

        private ParamsIK_DefinitionFeetLock_DefinitionLIKHandle lIKHandleField;

        /// <remarks/>
        public ParamsIK_DefinitionFeetLock_DefinitionRIKHandle RIKHandle
        {
            get
            {
                return this.rIKHandleField;
            }
            set
            {
                this.rIKHandleField = value;
            }
        }

        /// <remarks/>
        public ParamsIK_DefinitionFeetLock_DefinitionLIKHandle LIKHandle
        {
            get
            {
                return this.lIKHandleField;
            }
            set
            {
                this.lIKHandleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsIK_DefinitionFeetLock_DefinitionRIKHandle
    {

        private string handleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Handle
        {
            get
            {
                return this.handleField;
            }
            set
            {
                this.handleField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ParamsIK_DefinitionFeetLock_DefinitionLIKHandle
    {

        private string handleField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Handle
        {
            get
            {
                return this.handleField;
            }
            set
            {
                this.handleField = value;
            }
        }
    }


}
