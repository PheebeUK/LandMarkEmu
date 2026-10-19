namespace LandmarkEmulator.CaptureImport
{
    /// <summary>Axis-aligned claim / cell box in global world units (decimetres).</summary>
    public readonly struct ClaimBox
    {
        public ClaimBox(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        public double MinX { get; }
        public double MinY { get; }
        public double MinZ { get; }
        public double MaxX { get; }
        public double MaxY { get; }
        public double MaxZ { get; }

        public bool Contains(double x, double y, double z)
            => x >= MinX && x <= MaxX && y >= MinY && y <= MaxY && z >= MinZ && z <= MaxZ;

        public bool Intersects(ClaimBox other)
            => MinX <= other.MaxX && MaxX >= other.MinX
            && MinY <= other.MaxY && MaxY >= other.MinY
            && MinZ <= other.MaxZ && MaxZ >= other.MinZ;

        /// <summary>True when a prop at client-local metres falls inside this global claim box.</summary>
        public bool ContainsLocalMetres(float localX, float localY, float localZ)
        {
            (double gx, double gy, double gz) = TerrainMath.ToGlobal(localX, localY, localZ);
            return Contains(gx, gy, gz);
        }
    }
}
