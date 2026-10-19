using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.Database.Character.Model;
using LandmarkEmulator.WorldServer.Game.Terrain;
using LandmarkEmulator.WorldServer.Network;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace LandmarkEmulator.WorldServer.Game
{
    /// <summary>
    /// Applies an offline creation package (from Parser <c>extract-creations</c>) into the live world:
    /// claim row, layer-1 voxels, placed props.
    /// </summary>
    public static class CreationImporter
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static CreationImportPlan Plan(string packageDir, ulong ownerGuid, IEnumerable<ulong> claimFilter = null)
        {
            (CreationManifest manifest, CreationReport report) = CreationPackage.ReadMeta(packageDir);
            HashSet<ulong> filter = claimFilter?.ToHashSet();
            var plan = new CreationImportPlan { ZoneId = manifest.ZoneId, OwnerGuid = ownerGuid };
            foreach (ClaimAssessment a in report.Claims)
            {
                if (filter != null && filter.Count > 0 && !filter.Contains(a.ClaimId))
                    continue;
                ClaimPackageJson claim = null;
                try { claim = CreationPackage.ReadClaim(packageDir, a.ClaimId); }
                catch { /* missing payload when --ready-only skipped it */ }
                int props = 0;
                try { props = CreationPackage.ReadProps(packageDir, a.ClaimId).Count; } catch { /* */ }
                int cells = 0, build = 0;
                try
                {
                    var cellList = CreationPackage.ReadCells(packageDir, a.ClaimId);
                    cells = cellList.Count;
                    build = cellList.Count(c => c.Entry.HasBuildLayer && c.Data.Length > 0);
                }
                catch { /* */ }
                plan.Claims.Add(new CreationImportClaimPlan
                {
                    CaptureClaimId = a.ClaimId,
                    NewClaimId = 0,
                    Name = claim?.Name ?? $"claim {a.ClaimId}",
                    CellCount = cells,
                    BuildLayerCellCount = build,
                    PropCount = props,
                    Ready = a.Ready,
                    Warnings = a.Warnings ?? new List<string>()
                });
            }
            return plan;
        }

        /// <summary>
        /// Import ready claims. Returns the plan with <see cref="CreationImportClaimPlan.NewClaimId"/> filled.
        /// When <paramref name="dryRun"/> is true, nothing is written.
        /// </summary>
        public static CreationImportPlan Apply(string packageDir, ulong ownerGuid, string ownerName,
            IEnumerable<ulong> claimFilter = null, bool dryRun = false, WorldSession notify = null)
        {
            CreationImportPlan plan = Plan(packageDir, ownerGuid, claimFilter);
            if (dryRun)
            {
                log.Info($"importcreation dry-run: {plan.Claims.Count(c => c.Ready)} ready claim(s) on zone {plan.ZoneId} for owner 0x{ownerGuid:X}");
                return plan;
            }

            foreach (CreationImportClaimPlan entry in plan.Claims.Where(c => c.Ready))
            {
                ClaimPackageJson json = CreationPackage.ReadClaim(packageDir, entry.CaptureClaimId);
                ClaimModel imported = Claims.Import(new ClaimModel
                {
                    OwnerGuid = ownerGuid,
                    Name = string.IsNullOrWhiteSpace(json.Name) ? $"{ownerName}'s Claim" : json.Name,
                    Description = json.Description ?? "",
                    MinX = json.MinX,
                    MinY = json.MinY,
                    MinZ = json.MinZ,
                    MaxX = json.MaxX,
                    MaxY = json.MaxY,
                    MaxZ = json.MaxZ,
                    ZoneId = plan.ZoneId,
                    DefaultPermissions = json.DefaultPermissions,
                    GroupPermissions = json.GroupPermissions,
                    GuildPermissions = json.GuildPermissions,
                    TimeHour = json.TimeHour,
                    TimeRate = json.TimeRate,
                    VisitorTimeHour = json.VisitorTimeHour,
                    VisitorTimeRate = json.VisitorTimeRate,
                    AdventureFlags = json.AdventureFlags,
                    PlayMode = json.Mode == 2
                }, ownerName);
                entry.NewClaimId = imported.Id;

                ImportVoxels(packageDir, entry.CaptureClaimId, plan.ZoneId, ownerGuid);
                ImportProps(packageDir, entry.CaptureClaimId, plan.ZoneId, ownerGuid, notify);
                log.Info($"Imported capture claim {entry.CaptureClaimId} → site {imported.Id} ({entry.Name}): "
                    + $"{entry.BuildLayerCellCount} build cell(s), {entry.PropCount} prop(s).");
            }
            return plan;
        }

        private static void ImportVoxels(string packageDir, ulong captureClaimId, uint zone, ulong owner)
        {
            var writes = new List<(ulong Cell, List<TerrainEdits.VoxelValue> Values)>();
            foreach ((CellIndexEntry entry, byte[] data) in CreationPackage.ReadCells(packageDir, captureClaimId))
            {
                if (!entry.HasBuildLayer || data == null || data.Length == 0)
                    continue;
                try
                {
                    List<TerrainEdits.VoxelValue> values = LayerBlobToVoxels(data);
                    if (values.Count > 0)
                        writes.Add((entry.CellId, values));
                }
                catch (Exception ex)
                {
                    log.Warn(ex, $"Skipping cell 0x{entry.CellId:X16} of capture claim {captureClaimId}");
                }
            }
            if (writes.Count == 0)
                return;
            WriteResult written = EnvironmentServer.Instance.WriteCells(zone, owner, writes, null, false);
            if (written.Unavailable)
            {
                log.Warn($"Env unavailable while importing voxels for capture claim {captureClaimId}");
                return;
            }
            foreach (ulong cell in writes.Select(w => w.Cell).Distinct())
            {
                foreach (WorldSession session in PlayerManager.InZone(zone))
                    ZoneTerrain.SendManifestUpdate(session, cell);
            }
        }

        private static List<TerrainEdits.VoxelValue> LayerBlobToVoxels(byte[] packedLayer)
        {
            byte[] raw = TerrainStore.Unpack(packedLayer, 0, packedLayer.Length);
            BlockData blocks = BlockData.Deserialize(raw);
            var values = new List<TerrainEdits.VoxelValue>();
            for (int i = 0; i < BlockData.Count; i++)
            {
                if (blocks.Flags[i] == 0)
                    continue;
                values.Add(new TerrainEdits.VoxelValue(i, blocks.Flags[i], blocks.Materials[i],
                    blocks.Offsets[i * 3], blocks.Offsets[i * 3 + 1], blocks.Offsets[i * 3 + 2]));
            }
            return values;
        }

        private static void ImportProps(string packageDir, ulong captureClaimId, uint zone, ulong owner, WorldSession notify)
        {
            foreach (PropPackageJson prop in CreationPackage.ReadProps(packageDir, captureClaimId))
            {
                if (prop.PropId == 0)
                    continue;
                uint modelId = prop.ModelId != 0 ? prop.ModelId : Props.ModelOf(prop.PropId);
                if (modelId == 0)
                {
                    log.Warn($"Skipping prop def {prop.PropId}: no model");
                    continue;
                }
                Props.ImportCaptured(owner, zone, prop.PropId, modelId, prop.NameId,
                    new Vector3(prop.PosX, prop.PosY, prop.PosZ),
                    new Vector4(prop.RotX, prop.RotY, prop.RotZ, prop.RotW),
                    prop.Scale <= 0 ? 1f : prop.Scale,
                    prop.TintAlias ?? "",
                    prop.Enabled);
            }
        }
    }
}
