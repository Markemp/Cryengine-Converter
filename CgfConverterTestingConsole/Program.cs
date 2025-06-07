using CgfConverter;
using CgfConverter.CryEngineCore;

string startingFilePath = $@"d:\depot\sc3.24\data\objects\";
string datadir = $@"d:\depot\sc3.24\data\";

ArgsHandler argsHandler = new();

//// go through all the files in the directory recursively.  If it ends in .skin, .cgf, .cga or .chr, process it.
//foreach (string file in Directory.EnumerateFiles(startingFilePath, "*.*", SearchOption.AllDirectories))
//{
//    if (file.EndsWith(".skin") || file.EndsWith(".cgf") || file.EndsWith(".cga") || file.EndsWith(".chr"))
//    {
//        Console.WriteLine($@"Processing {file}");
//        CryEngine cryData = new(file, argsHandler.PackFileSystem);
//        cryData.ProcessCryengineFiles();

//        // See if it has an IVOTangents datastream
//        var ivoSkinMesh = (ChunkIvoSkinMesh)cryData.Models[1].ChunkMap.FirstOrDefault(x => x.Value.ChunkType == ChunkType.IvoSkin).Value;
//        if (ivoSkinMesh is null)
//        {
//            Console.WriteLine($@"{file}: Found IVOSkin datastream with no IvoSkinMesh!");
//            Console.WriteLine("Press any key to continue...");
//            Console.ReadKey();
//            continue;
//        }
//        if (ivoSkinMesh.Tangents.BytesPerElement == 16)
//        {
//            Console.WriteLine($@"{file}: Found IVOSkin datastream with 16 bytes per element!");
//            Console.WriteLine("Press any key to continue...");
//            Console.ReadKey();
//        }
//    }
//}

// Open the file at {startingFilePath\box.cgf and convert it to crydata
var file = new FileInfo(datadir + $@"objects\default\box.cgf");
CryEngine cryData = new(file.ToString(), argsHandler.PackFileSystem);

cryData.ProcessCryengineFiles();

var ivoSkinMesh = (ChunkIvoSkinMesh)cryData.Models[1].ChunkMap.FirstOrDefault(x => x.Value.ChunkType == ChunkType.IvoSkin || x.Value.ChunkType == ChunkType.IvoSkin2).Value;

Console.ReadKey();
