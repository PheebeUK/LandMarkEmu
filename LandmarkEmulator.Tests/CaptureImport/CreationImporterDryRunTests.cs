using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.WorldServer.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class CreationImporterDryRunTests
    {
        [Fact]
        public void Plan_CountsReadyClaimsWithoutWriting()
        {
            string dir = Path.Combine(Path.GetTempPath(), "lmemu-import-" + Guid.NewGuid().ToString("N"));
            try
            {
                var claim = new ClaimRecord
                {
                    Id = 50, OwnerGuid = 1, Name = "Dry", InfoName = "Dry",
                    MinX = 0, MinY = 0, MinZ = 0, MaxX = 10, MaxY = 10, MaxZ = 10
                };
                var prop = new LightweightPropRecord
                {
                    Guid = 1, PropId = 3, ModelId = 4,
                    Position = new Vector4(1, 1, 1, 1), Rotation = new Vector4(0, 0, 0, 1),
                    Scale = new Vector4(1, 1, 1, 1)
                };
                var cells = new List<CapturedCell>
                {
                    new() { CellId = 1, HasBuildLayer = true, BuildLayerData = new byte[] { 1, 2, 3 } }
                };
                var claims = new Dictionary<ulong, ClaimRecord> { [50] = claim };
                var props = new Dictionary<ulong, List<LightweightPropRecord>> { [50] = new() { prop } };
                var cellMap = new Dictionary<ulong, List<CapturedCell>> { [50] = cells };
                var report = new CreationReport
                {
                    Claims = { CreationSufficiency.Assess(claim, cells, props[50], new List<LightweightPropRecord>()) }
                };
                CreationPackage.Write(dir, new CreationManifest { ZoneId = 406, ClaimCount = 1, ReadyCount = 1 },
                    report, claims, props, cellMap, new List<LightweightPropRecord>(), readyOnly: false);

                CreationImportPlan plan = CreationImporter.Plan(dir, ownerGuid: 99);
                CreationImportClaimPlan entry = Assert.Single(plan.Claims);
                Assert.True(entry.Ready);
                Assert.Equal(50ul, entry.CaptureClaimId);
                Assert.Equal(1, entry.PropCount);
                Assert.Equal(1, entry.BuildLayerCellCount);
                Assert.Equal(406u, plan.ZoneId);

                CreationImportPlan dry = CreationImporter.Apply(dir, 99, "Tester", dryRun: true);
                Assert.Equal(0ul, Assert.Single(dry.Claims).NewClaimId);
            }
            finally
            {
                if (Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
        }
    }
}
