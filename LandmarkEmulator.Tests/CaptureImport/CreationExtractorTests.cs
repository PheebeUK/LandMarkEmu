using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.WorldServer.Game.Terrain;
using System.Linq;
using System.Numerics;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class CreationExtractorTests
    {
        [Fact]
        public void Build_AssignsPropAndCellToClaim()
        {
            ulong cellId = TerrainStore.CellId(2, 100, 50, 200);
            ClaimBox cellBox = TerrainMath.CellBox(cellId);
            (float lx, float ly, float lz) = TerrainMath.ToLocal(
                (cellBox.MinX + cellBox.MaxX) / 2,
                (cellBox.MinY + cellBox.MaxY) / 2,
                (cellBox.MinZ + cellBox.MaxZ) / 2);

            var claim = new ClaimRecord
            {
                Id = 3, OwnerGuid = 1, Name = "C", InfoName = "C",
                MinX = cellBox.MinX, MinY = cellBox.MinY, MinZ = cellBox.MinZ,
                MaxX = cellBox.MaxX, MaxY = cellBox.MaxY, MaxZ = cellBox.MaxZ
            };
            var prop = new LightweightPropRecord
            {
                Guid = 1, ModelId = 50, Position = new Vector4(lx, ly, lz, 1f)
            };
            var cell = new CreationExtractor.CellSource
            {
                CellId = cellId, Version = 0, FirstLayerId = 1, SegmentCount = 1,
                DataBlob = BuildLayerBlob(new byte[] { 9, 8, 7 })
            };
            var index = PropModelIndex.FromPairs(new[] { (8u, 50u) });

            CreationExtractor.Result result = CreationExtractor.Build(new[] { claim }, new[] { prop }, new[] { cell }, index);
            Assert.True(Assert.Single(result.Report.Claims).Ready);
            Assert.Equal(8u, Assert.Single(result.PropsByClaim[3]).PropId);
            Assert.True(Assert.Single(result.CellsByClaim[3]).HasBuildLayer);
        }

        /// <summary>First layer body: u32 version, u8 flag, u32 size, data (no layer id — first segment).</summary>
        private static byte[] BuildLayerBlob(byte[] data)
        {
            var blob = new byte[9 + data.Length];
            System.BitConverter.GetBytes(1u).CopyTo(blob, 0);
            blob[4] = 0;
            System.BitConverter.GetBytes((uint)data.Length).CopyTo(blob, 5);
            data.CopyTo(blob, 9);
            return blob;
        }
    }
}
