using UnityEngine;

/// <summary>Decorative world entity (trees, bushes, flowers, bridge planks...). No gameplay effect.</summary>
public class Scenery : WorldEntity
{
    void Awake() { kind = EntityKind.Scenery; }
}
