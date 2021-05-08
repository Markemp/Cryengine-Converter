namespace CgfConverter.CryEngineCore
{
    public abstract class ChunkMtlName : Chunk  
    {
        /// <summary> Type of Material associated with this name </summary>
        public MtlNameType MatType { get; internal set; }
        /// <summary> Name of the Material </summary>
        public string Name { get; set; }
        public MtlNamePhysicsType[] PhysicsType { get; internal set; }
        /// <summary> Number of Materials in this name (Max: 66) </summary>
        public uint NumChildren { get; internal set; }
        public uint[] ChildIDs { get; internal set; }

        public override string ToString()
        {
            return $@"Chunk Type: {ChunkType}, ID: {ID:X}, Material Name: {Name}, Number of Children: {NumChildren}, Material Type: {MatType}";
        }
    }
}
