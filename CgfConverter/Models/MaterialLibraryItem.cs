using System;

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
        public CryEngineCore.Material Material { get; set; }
    }
}
