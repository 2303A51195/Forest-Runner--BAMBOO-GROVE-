using UnityEngine;

/// <summary>
/// Pseudo-3D lane maths. Everything in the world has a depth "z" (0 = at the player, large = far away).
/// Perspective factor F(z) shrinks objects and moves them toward the horizon.
/// </summary>
public static class LaneManager
{
    public const int LaneCount = 3;
    public const float LaneSpacing = 2.5f;
    public const float HorizonY = 3.2f;
    public const float GroundY = -5.2f;      // screen Y of the player's feet
    public const float D = 12f;              // perspective strength
    public const float SpawnZ = 85f;
    public const float PathHalfWidth = 3.9f;

    public static float F(float z) { return D / (D + z); }
    public static float ScreenY(float z) { return HorizonY + (GroundY - HorizonY) * F(z); }
    public static float LaneToX(float lane) { return (lane - 1f) * LaneSpacing; }
}
