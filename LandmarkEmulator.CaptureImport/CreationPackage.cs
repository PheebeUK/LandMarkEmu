using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LandmarkEmulator.CaptureImport
{
    public sealed class CreationManifest
    {
        public string ToolVersion { get; set; } = "1";
        public uint ZoneId { get; set; }
        public string SourcePath { get; set; } = "";
        public string CreatedUtc { get; set; } = "";
        public int ClaimCount { get; set; }
        public int ReadyCount { get; set; }
    }

    public sealed class CreationReport
    {
        public List<ClaimAssessment> Claims { get; set; } = new();
    }

    public sealed class ClaimPackageJson
    {
        public ulong Id { get; set; }
        public ulong OwnerGuid { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public double MinX { get; set; }
        public double MinY { get; set; }
        public double MinZ { get; set; }
        public double MaxX { get; set; }
        public double MaxY { get; set; }
        public double MaxZ { get; set; }
        public uint DefaultPermissions { get; set; }
        public uint GroupPermissions { get; set; }
        public uint GuildPermissions { get; set; }
        public int TimeHour { get; set; }
        public float TimeRate { get; set; }
        public int VisitorTimeHour { get; set; }
        public float VisitorTimeRate { get; set; }
        public uint Mode { get; set; }
        public ulong AdventureFlags { get; set; }
        public Dictionary<ulong, uint> SpecialPlayers { get; set; } = new();
    }

    public sealed class PropPackageJson
    {
        public ulong CaptureGuid { get; set; }
        public uint PropId { get; set; }
        public uint ModelId { get; set; }
        public uint NameId { get; set; }
        public string Name { get; set; } = "";
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }
        public float Scale { get; set; } = 1f;
        public string TintAlias { get; set; } = "";
        public ulong OwnerGuid { get; set; }
        public bool Enabled { get; set; } = true;
        public bool PropIdAmbiguous { get; set; }
    }

    public sealed class CellIndexEntry
    {
        public ulong CellId { get; set; }
        public uint Version { get; set; }
        public bool HasBuildLayer { get; set; }
        public string File { get; set; } = "";
    }

    /// <summary>Read/write the offline creation package directory layout.</summary>
    public static class CreationPackage
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static readonly JsonSerializerOptions JsonlOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Write(
            string packageDir,
            CreationManifest manifest,
            CreationReport report,
            IReadOnlyDictionary<ulong, ClaimRecord> claims,
            IReadOnlyDictionary<ulong, List<LightweightPropRecord>> propsByClaim,
            IReadOnlyDictionary<ulong, List<CapturedCell>> cellsByClaim,
            IReadOnlyList<LightweightPropRecord> unresolvedProps,
            bool readyOnly)
        {
            Directory.CreateDirectory(packageDir);
            Directory.CreateDirectory(Path.Combine(packageDir, "claims"));
            Directory.CreateDirectory(Path.Combine(packageDir, "props"));
            Directory.CreateDirectory(Path.Combine(packageDir, "cells"));

            var assessments = report.Claims.ToDictionary(c => c.ClaimId);
            foreach ((ulong claimId, ClaimRecord claim) in claims)
            {
                if (!assessments.TryGetValue(claimId, out ClaimAssessment assessment))
                    continue;
                if (readyOnly && !assessment.Ready)
                    continue;

                WriteJson(Path.Combine(packageDir, "claims", $"{claimId}.json"), ToClaimJson(claim));

                List<LightweightPropRecord> props = propsByClaim.GetValueOrDefault(claimId) ?? new List<LightweightPropRecord>();
                string propsPath = Path.Combine(packageDir, "props", $"{claimId}.jsonl");
                File.WriteAllLines(propsPath, props.Select(p => JsonSerializer.Serialize(ToPropJson(p), JsonlOptions)));

                string cellDir = Path.Combine(packageDir, "cells", claimId.ToString());
                Directory.CreateDirectory(cellDir);
                List<CapturedCell> cells = cellsByClaim.GetValueOrDefault(claimId) ?? new List<CapturedCell>();
                var index = new List<CellIndexEntry>();
                foreach (CapturedCell cell in cells)
                {
                    string file = $"{cell.CellId:X16}.bin";
                    File.WriteAllBytes(Path.Combine(cellDir, file), cell.BuildLayerData ?? Array.Empty<byte>());
                    index.Add(new CellIndexEntry
                    {
                        CellId = cell.CellId,
                        Version = cell.Version,
                        HasBuildLayer = cell.HasBuildLayer,
                        File = file
                    });
                }
                WriteJson(Path.Combine(cellDir, "index.json"), index);
            }

            WriteJson(Path.Combine(packageDir, "manifest.json"), manifest);
            WriteJson(Path.Combine(packageDir, "report.json"), report);
            string unresolvedPath = Path.Combine(packageDir, "unresolved-props.jsonl");
            File.WriteAllLines(unresolvedPath,
                (unresolvedProps ?? Array.Empty<LightweightPropRecord>()).Select(p => JsonSerializer.Serialize(ToPropJson(p), JsonlOptions)));
        }

        public static (CreationManifest Manifest, CreationReport Report) ReadMeta(string packageDir)
        {
            var manifest = ReadJson<CreationManifest>(Path.Combine(packageDir, "manifest.json"));
            var report = ReadJson<CreationReport>(Path.Combine(packageDir, "report.json"));
            return (manifest, report);
        }

        public static ClaimPackageJson ReadClaim(string packageDir, ulong claimId)
            => ReadJson<ClaimPackageJson>(Path.Combine(packageDir, "claims", $"{claimId}.json"));

        public static List<PropPackageJson> ReadProps(string packageDir, ulong claimId)
        {
            string path = Path.Combine(packageDir, "props", $"{claimId}.jsonl");
            if (!File.Exists(path))
                return new List<PropPackageJson>();
            return File.ReadAllLines(path)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => JsonSerializer.Deserialize<PropPackageJson>(l, JsonlOptions))
                .Where(p => p != null)
                .Cast<PropPackageJson>()
                .ToList();
        }

        public static List<(CellIndexEntry Entry, byte[] Data)> ReadCells(string packageDir, ulong claimId)
        {
            string cellDir = Path.Combine(packageDir, "cells", claimId.ToString());
            string indexPath = Path.Combine(cellDir, "index.json");
            if (!File.Exists(indexPath))
                return new List<(CellIndexEntry, byte[])>();
            var index = ReadJson<List<CellIndexEntry>>(indexPath) ?? new List<CellIndexEntry>();
            var result = new List<(CellIndexEntry, byte[])>();
            foreach (CellIndexEntry entry in index)
            {
                string file = Path.Combine(cellDir, entry.File);
                byte[] data = File.Exists(file) ? File.ReadAllBytes(file) : Array.Empty<byte>();
                result.Add((entry, data));
            }
            return result;
        }

        private static ClaimPackageJson ToClaimJson(ClaimRecord c) => new()
        {
            Id = c.Id,
            OwnerGuid = c.OwnerGuid,
            Name = c.Name,
            Description = c.Description,
            MinX = c.MinX,
            MinY = c.MinY,
            MinZ = c.MinZ,
            MaxX = c.MaxX,
            MaxY = c.MaxY,
            MaxZ = c.MaxZ,
            DefaultPermissions = c.DefaultPermissions,
            GroupPermissions = c.GroupPermissions,
            GuildPermissions = c.GuildPermissions,
            TimeHour = c.TimeHour,
            TimeRate = c.TimeRate,
            VisitorTimeHour = c.VisitorTimeHour,
            VisitorTimeRate = c.VisitorTimeRate,
            Mode = c.Mode,
            AdventureFlags = c.AdventureFlags,
            SpecialPlayers = c.SpecialPlayers ?? new Dictionary<ulong, uint>()
        };

        private static PropPackageJson ToPropJson(LightweightPropRecord p) => new()
        {
            CaptureGuid = p.Guid,
            PropId = p.PropId,
            ModelId = p.ModelId,
            NameId = p.NameId,
            Name = p.Name,
            PosX = p.Position.X,
            PosY = p.Position.Y,
            PosZ = p.Position.Z,
            RotX = p.Rotation.X,
            RotY = p.Rotation.Y,
            RotZ = p.Rotation.Z,
            RotW = p.Rotation.W,
            Scale = p.Scale.X,
            TintAlias = p.TintAlias,
            OwnerGuid = p.OwnerGuid,
            Enabled = p.Enabled,
            PropIdAmbiguous = p.PropIdAmbiguous
        };

        private static void WriteJson<T>(string path, T value)
            => File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions));

        private static T ReadJson<T>(string path)
            => JsonSerializer.Deserialize<T>(File.ReadAllText(path), JsonOptions);
    }
}
