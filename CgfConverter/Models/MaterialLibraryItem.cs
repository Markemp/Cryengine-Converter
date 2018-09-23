using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CgfConverter.Models
{
    public class MaterialLibraryItem
    {
        /// <summary>
        /// Unique identifier for this material.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Name of the material file this material was found.
        /// </summary>
        public string MaterialFileSource { get; set; }

        /// <summary>
        /// A material from MaterialFileSource
        /// </summary>
        public CryEngine_Core.Material Material { get; set; }
    }
}
