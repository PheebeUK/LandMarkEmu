using LandmarkEmulator.CaptureImport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class CreationPackageTests
    {
        [Fact]
        public void Write_Then_Read_RoundTripsLayout()
        {
            string dir = Path.Combine(Path.GetTempPath(), "lmemu-creation-" + Guid.NewGuid().ToString("N"));
            try
            {
                var claim = new ClaimRecord
                {
                    Id = 77, OwnerGuid = 5, Name = "Pack", InfoName = "Pack",
                    MinX = 1, MinY = 2, MinZ = 3, MaxX = 4, MaxY = 5, MaxZ = 6,
                    DefaultPermissions = 3
                };
                var prop = new LightweightPropRecord
                {
                    Guid = 9, PropId = 11, ModelId = 22,
                    Position = new Vector4(1, 2, 3, 1), Rotation = new Vector4(0, 0, 0, 1),
                    Scale = new Vector4(1, 1, 1, 1), TintAlias = "t"
                };
                var cell = new CapturedCell
                {
                    CellId = 0x2000000000000001ul, Version = 0, HasBuildLayer = true,
                    BuildLayerData = new byte[] { 0xA1, 0xB2 }
                };
                var result = CreationExtractor.Build(
                    new[] { claim },
                    new[] { prop },
                    Array.Empty<CreationExtractor.CellSource>(),
                    PropModelIndex.FromPairs(new[] { (11u, 22u) }));
                // Force cells onto the claim for package IO (prop may not sit in box).
                result.CellsByClaim[77] = new List<CapturedCell> { cell };
                result.PropsByClaim[77] = new List<LightweightPropRecord> { prop };
                result.Report = new CreationReport
                {
                    Claims = { CreationSufficiency.Assess(claim, result.CellsByClaim[77], result.PropsByClaim[77], new List<LightweightPropRecord>()) }
                };

                var manifest = new CreationManifest
                {
                    ZoneId = 406, SourcePath = "bins", CreatedUtc = "2026-01-01T00:00:00Z",
                    ClaimCount = 1, ReadyCount = 1
                };
                CreationPackage.Write(dir, manifest, result.Report, result.Claims, result.PropsByClaim,
                    result.CellsByClaim, result.UnresolvedProps, readyOnly: false);

                Assert.True(File.Exists(Path.Combine(dir, "manifest.json")));
                Assert.True(File.Exists(Path.Combine(dir, "report.json")));
                Assert.True(File.Exists(Path.Combine(dir, "claims", "77.json")));
                Assert.True(File.Exists(Path.Combine(dir, "props", "77.jsonl")));

                (CreationManifest m, CreationReport r) = CreationPackage.ReadMeta(dir);
                Assert.Equal(406u, m.ZoneId);
                Assert.True(Assert.Single(r.Claims).Ready);

                ClaimPackageJson cj = CreationPackage.ReadClaim(dir, 77);
                Assert.Equal("Pack", cj.Name);
                Assert.Equal(11u, Assert.Single(CreationPackage.ReadProps(dir, 77)).PropId);

                var cells = CreationPackage.ReadCells(dir, 77);
                Assert.Equal(0xA1, Assert.Single(cells).Data[0]);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }
    }
}
