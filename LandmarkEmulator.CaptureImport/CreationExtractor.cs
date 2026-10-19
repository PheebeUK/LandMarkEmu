using System;
using System.Collections.Generic;
using System.Linq;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// Pure assembly of claims + props + cells into a package assessment (no IO beyond what callers pass).
    /// </summary>
    public static class CreationExtractor
    {
        public sealed class CellSource
        {
            public ulong CellId { get; init; }
            public uint Version { get; init; }
            public uint FirstLayerId { get; init; }
            public uint SegmentCount { get; init; }
            public byte[] DataBlob { get; init; } = Array.Empty<byte>();
        }

        public sealed class Result
        {
            public Dictionary<ulong, ClaimRecord> Claims { get; set; } = new();
            public Dictionary<ulong, List<LightweightPropRecord>> PropsByClaim { get; set; } = new();
            public Dictionary<ulong, List<CapturedCell>> CellsByClaim { get; set; } = new();
            public List<LightweightPropRecord> UnresolvedProps { get; set; } = new();
            public CreationReport Report { get; set; } = new();
        }

        public static Result Build(
            IEnumerable<ClaimRecord> claims,
            IEnumerable<LightweightPropRecord> props,
            IEnumerable<CellSource> cells,
            PropModelIndex propIndex)
        {
            var claimMap = new Dictionary<ulong, ClaimRecord>();
            foreach (ClaimRecord c in claims ?? Array.Empty<ClaimRecord>())
                claimMap[c.Id] = c;

            var propsByClaim = claimMap.Keys.ToDictionary(id => id, _ => new List<LightweightPropRecord>());
            var unresolved = new List<LightweightPropRecord>();

            foreach (LightweightPropRecord prop in props ?? Array.Empty<LightweightPropRecord>())
            {
                if (propIndex != null && prop.PropId == 0 && prop.ModelId != 0)
                {
                    if (propIndex.TryResolve(prop.ModelId, out uint propId, out bool ambiguous))
                    {
                        prop.PropId = propId;
                        prop.PropIdAmbiguous = ambiguous;
                    }
                }

                ClaimRecord host = null;
                foreach (ClaimRecord claim in claimMap.Values)
                {
                    if (claim.Box.ContainsLocalMetres(prop.Position.X, prop.Position.Y, prop.Position.Z))
                    {
                        host = claim;
                        break;
                    }
                }

                if (host == null)
                    continue;

                if (prop.PropId == 0)
                    unresolved.Add(prop);
                else
                    propsByClaim[host.Id].Add(prop);
            }

            var cellsByClaim = claimMap.Keys.ToDictionary(id => id, _ => new List<CapturedCell>());
            foreach (CellSource cell in cells ?? Array.Empty<CellSource>())
            {
                ClaimBox cellBox = TerrainMath.CellBox(cell.CellId);
                CapturedCell captured = CellLayerExtract.ToCaptured(
                    cell.CellId, cell.Version, cell.FirstLayerId, cell.SegmentCount, cell.DataBlob);
                foreach (ClaimRecord claim in claimMap.Values)
                {
                    if (claim.Box.Intersects(cellBox))
                        cellsByClaim[claim.Id].Add(captured);
                }
            }

            var report = new CreationReport();
            foreach (ClaimRecord claim in claimMap.Values.OrderBy(c => c.Id))
            {
                // Unresolved props that fall in this claim (for warning counts).
                var unresolvedHere = unresolved
                    .Where(p => claim.Box.ContainsLocalMetres(p.Position.X, p.Position.Y, p.Position.Z))
                    .ToList();
                report.Claims.Add(CreationSufficiency.Assess(
                    claim,
                    cellsByClaim[claim.Id],
                    propsByClaim[claim.Id],
                    unresolvedHere));
            }

            return new Result
            {
                Claims = claimMap,
                PropsByClaim = propsByClaim,
                CellsByClaim = cellsByClaim,
                UnresolvedProps = unresolved,
                Report = report
            };
        }
    }

    /// <summary>Dry-run plan produced before WorldServer apply.</summary>
    public sealed class CreationImportPlan
    {
        public uint ZoneId { get; set; }
        public ulong OwnerGuid { get; set; }
        public List<CreationImportClaimPlan> Claims { get; set; } = new();
    }

    public sealed class CreationImportClaimPlan
    {
        public ulong CaptureClaimId { get; set; }
        public ulong NewClaimId { get; set; }
        public string Name { get; set; } = "";
        public int CellCount { get; set; }
        public int BuildLayerCellCount { get; set; }
        public int PropCount { get; set; }
        public bool Ready { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
