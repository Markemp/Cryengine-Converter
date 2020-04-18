using System.Collections.Generic;

namespace CgfConverter.Models
{
    public class MaterialLibrary
    {
        public string GameName { get; set; }

        public string BaseDirectory { get; set; }

        public List<MaterialLibraryItem> MaterialLibraryItems { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialLibrary()
        {
            this.MaterialLibraryItems = new List<MaterialLibraryItem>();
        }


    }
}
