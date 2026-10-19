namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// Cell / zone geometry mirrored from EnvServer <c>TerrainStore</c> so CaptureImport stays free of that project.
    /// </summary>
    public static class TerrainMath
    {
        public const double ZoneOriginX = 25584000;
        public const double ZoneOriginY = 20000;
        public const double ZoneOriginZ = 25564500;

        public static double CellSize(uint level) => 40.0 * (1u << (int)level);

        public static (uint Level, uint X, uint Y, uint Z) DecodeCell(ulong id)
            => ((uint)(id >> 60), (uint)((id >> 36) & 0x3FFFF), (uint)(((id >> 18) & 0x3FFFF) >> 6), (uint)(id & 0x3FFFF));

        public static (double X, double Y, double Z) CellOrigin(ulong id)
        {
            (uint level, uint x, uint y, uint z) = DecodeCell(id);
            double size = CellSize(level);
            return (x * size, y * size, z * size);
        }

        /// <summary>Global dm AABB of a terrain cell.</summary>
        public static ClaimBox CellBox(ulong id)
        {
            (uint level, _, _, _) = DecodeCell(id);
            double size = CellSize(level);
            (double ox, double oy, double oz) = CellOrigin(id);
            return new ClaimBox(ox, oy, oz, ox + size, oy + size, oz + size);
        }

        public static (double X, double Y, double Z) ToGlobal(float localX, float localY, float localZ)
            => (ZoneOriginX + localX * 10.0, ZoneOriginY + localY * 10.0, ZoneOriginZ + localZ * 10.0);

        public static (float X, float Y, float Z) ToLocal(double globalX, double globalY, double globalZ)
            => ((float)((globalX - ZoneOriginX) / 10.0), (float)((globalY - ZoneOriginY) / 10.0),
                (float)((globalZ - ZoneOriginZ) / 10.0));
    }
}
