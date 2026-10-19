using System.Collections.Generic;
using System.Linq;

namespace LandmarkEmulator.CaptureImport
{
    public static class CreationSufficiency
    {
        public const string WarnNoBuildLayer = "no_build_layer";
        public const string WarnUnresolvedModel = "unresolved_model";
        public const string WarnAmbiguousModel = "ambiguous_model";
        public const string WarnPartialCells = "partial_cells";

        public static ClaimAssessment Assess(
            ClaimRecord claim,
            IReadOnlyList<CapturedCell> cells,
            IReadOnlyList<LightweightPropRecord> resolvedProps,
            IReadOnlyList<LightweightPropRecord> unresolvedProps)
        {
            var warnings = new List<string>();
            bool decoded = claim != null && claim.Id != 0
                && (claim.MaxX > claim.MinX || claim.MaxY > claim.MinY || claim.MaxZ > claim.MinZ);
            int cellCount = cells?.Count ?? 0;
            int withBuild = cells?.Count(c => c.HasBuildLayer) ?? 0;
            if (cellCount > 0 && withBuild == 0)
                warnings.Add(WarnNoBuildLayer);
            if (cellCount > 0 && withBuild > 0 && withBuild < cellCount)
                warnings.Add(WarnPartialCells);

            int propResolved = resolvedProps?.Count ?? 0;
            int propUnresolved = unresolvedProps?.Count ?? 0;
            if (propUnresolved > 0)
                warnings.Add(WarnUnresolvedModel);
            if (resolvedProps != null && resolvedProps.Any(p => p.PropIdAmbiguous))
                warnings.Add(WarnAmbiguousModel);

            bool hasVoxels = cellCount > 0;
            bool hasProps = propResolved > 0;
            bool ready = decoded && (hasVoxels || hasProps);
            bool empty = decoded && !hasVoxels && !hasProps;

            return new ClaimAssessment
            {
                ClaimId = claim?.Id ?? 0,
                Decoded = decoded,
                HasVoxels = hasVoxels,
                HasProps = hasProps,
                Ready = ready,
                Empty = empty,
                CellCount = cellCount,
                BuildLayerCellCount = withBuild,
                ResolvedPropCount = propResolved,
                UnresolvedPropCount = propUnresolved,
                Warnings = warnings
            };
        }
    }

    public sealed class ClaimAssessment
    {
        public ulong ClaimId { get; set; }
        public bool Decoded { get; set; }
        public bool HasVoxels { get; set; }
        public bool HasProps { get; set; }
        public bool Ready { get; set; }
        public bool Empty { get; set; }
        public int CellCount { get; set; }
        public int BuildLayerCellCount { get; set; }
        public int ResolvedPropCount { get; set; }
        public int UnresolvedPropCount { get; set; }
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>One CellsUpdate cell clipped to a claim for packaging.</summary>
    public sealed class CapturedCell
    {
        public ulong CellId { get; set; }
        public uint Version { get; set; }
        public bool HasBuildLayer { get; set; }
        /// <summary>Layer-1 payload bytes (version+flag+size already stripped to size bytes only), or empty.</summary>
        public byte[] BuildLayerData { get; set; } = System.Array.Empty<byte>();
    }
}
