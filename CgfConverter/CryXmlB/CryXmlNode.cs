using System;

namespace HoloXPLOR.DataForge
{
    public class CryXmlNode
    {
        public Int32 NodeID { get; set; }
        public Int32 NodeNameOffset { get; set; }
        public Int32 ItemType { get; set; }
        public Int16 AttributeCount { get; set; }
        public Int16 ChildCount { get; set; }
        public Int32 ParentNodeID { get; set; }
        public Int32 FirstAttributeIndex { get; set; }
        public Int32 FirstChildIndex { get; set; }
        public Int32 Reserved { get; set; }
    }
}
