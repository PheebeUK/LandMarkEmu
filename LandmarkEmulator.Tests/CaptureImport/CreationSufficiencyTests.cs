using LandmarkEmulator.CaptureImport;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class CreationSufficiencyTests
    {
        private static ClaimRecord Claim() => new()
        {
            Id = 1, OwnerGuid = 2, Name = "S", InfoName = "S",
            MinX = 0, MinY = 0, MinZ = 0, MaxX = 100, MaxY = 100, MaxZ = 100
        };

        [Fact]
        public void VoxelsOnly_IsReady()
        {
            var cells = new List<CapturedCell> { new() { CellId = 1, HasBuildLayer = true, BuildLayerData = new byte[] { 1 } } };
            ClaimAssessment a = CreationSufficiency.Assess(Claim(), cells, new List<LightweightPropRecord>(), new List<LightweightPropRecord>());
            Assert.True(a.Ready);
            Assert.True(a.HasVoxels);
            Assert.False(a.Empty);
        }

        [Fact]
        public void PropsOnly_IsReady()
        {
            var props = new List<LightweightPropRecord> { new() { PropId = 5, ModelId = 1 } };
            ClaimAssessment a = CreationSufficiency.Assess(Claim(), new List<CapturedCell>(), props, new List<LightweightPropRecord>());
            Assert.True(a.Ready);
            Assert.True(a.HasProps);
        }

        [Fact]
        public void Neither_IsEmpty()
        {
            ClaimAssessment a = CreationSufficiency.Assess(Claim(), new List<CapturedCell>(), new List<LightweightPropRecord>(), new List<LightweightPropRecord>());
            Assert.False(a.Ready);
            Assert.True(a.Empty);
        }

        [Fact]
        public void UnresolvedProps_Warn()
        {
            var unresolved = new List<LightweightPropRecord> { new() { ModelId = 99, Position = Vector4.Zero } };
            ClaimAssessment a = CreationSufficiency.Assess(Claim(), new List<CapturedCell>(), new List<LightweightPropRecord>(), unresolved);
            Assert.Contains(CreationSufficiency.WarnUnresolvedModel, a.Warnings);
        }

        [Fact]
        public void NoBuildLayer_Warn()
        {
            var cells = new List<CapturedCell> { new() { CellId = 1, HasBuildLayer = false } };
            ClaimAssessment a = CreationSufficiency.Assess(Claim(), cells, new List<LightweightPropRecord>(), new List<LightweightPropRecord>());
            Assert.Contains(CreationSufficiency.WarnNoBuildLayer, a.Warnings);
            Assert.True(a.Ready);
        }
    }
}
