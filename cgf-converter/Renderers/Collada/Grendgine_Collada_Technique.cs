using CgfConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace grendgine_collada
{
    #region Option 1 - Extend Grendgine_Collada_Technique class and add the bump property

    public partial class Grendgine_Collada_Technique
    {
        [XmlElement(ElementName = "bump")]
        public Grendgine_Collada_BumpMap[] Bump { get; set; }
    }

    #endregion
}
