using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.WorldServer.Game.Terrain;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class ClaimCellIntersectionTests
    {
        [Fact]
        public void CellBox_MatchesTerrainStoreOrigin()
        {
            ulong id = TerrainStore.CellId(2, 100, 50, 200);
            ClaimBox box = TerrainMath.CellBox(id);
            (double ox, double oy, double oz) = TerrainStore.CellOrigin(id);
            Assert.Equal(ox, box.MinX);
            Assert.Equal(oy, box.MinY);
            Assert.Equal(oz, box.MinZ);
            Assert.Equal(TerrainStore.CellSize(2), box.MaxX - box.MinX);
        }

        [Fact]
        public void Claim_Intersects_ContainingCell()
        {
            ulong id = TerrainStore.CellId(2, 10, 10, 10);
            ClaimBox cell = TerrainMath.CellBox(id);
            var claim = new ClaimBox(cell.MinX + 1, cell.MinY + 1, cell.MinZ + 1,
                cell.MinX + 10, cell.MinY + 10, cell.MinZ + 10);
            Assert.True(claim.Intersects(cell));
        }

        [Fact]
        public void Claim_DoesNotIntersect_DistantCell()
        {
            ulong id = TerrainStore.CellId(2, 10, 10, 10);
            ClaimBox cell = TerrainMath.CellBox(id);
            var claim = new ClaimBox(cell.MaxX + 1000, cell.MinY, cell.MinZ,
                cell.MaxX + 2000, cell.MaxY, cell.MaxZ);
            Assert.False(claim.Intersects(cell));
        }

        [Fact]
        public void ContainsLocalMetres_UsesZoneOrigin()
        {
            var claim = new ClaimBox(
                TerrainMath.ZoneOriginX, TerrainMath.ZoneOriginY, TerrainMath.ZoneOriginZ,
                TerrainMath.ZoneOriginX + 100, TerrainMath.ZoneOriginY + 100, TerrainMath.ZoneOriginZ + 100);
            Assert.True(claim.ContainsLocalMetres(0f, 0f, 0f));
            Assert.False(claim.ContainsLocalMetres(50f, 0f, 0f));
        }
    }
}
