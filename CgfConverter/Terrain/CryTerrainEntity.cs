using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CgfConverter.Terrain.Xml;

namespace CgfConverter.Terrain;

public class CryTerrainEntity
{
    public readonly int Id;
    public readonly ObjectOrEntity Underlying;
    public readonly ImmutableList<CryTerrainEntity> Children;

    public CryTerrainEntity(int id, ObjectOrEntity underlying, IList<ObjectOrEntity> objectOrEntities)
    {
        Id = id;
        Underlying = underlying;
        Children = objectOrEntities
            .Where(x => (x.ParentIdValue ?? 0) == Id && int.TryParse(x.EntityId, out _))
            .Select(x => new CryTerrainEntity(x.EntityIdValue!.Value, x, objectOrEntities))
            .ToImmutableList();
    }
}