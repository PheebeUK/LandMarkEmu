using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.Shared.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LandmarkEmulator.Parser
{
    /// <summary>
    /// Builds a creation package from parsed zone bins (ClaimsBase AOI, AddLightweightProp, CellsUpdate).
    /// </summary>
    public static class CreationExtractRunner
    {
        public sealed class Options
        {
            public string BinsDir { get; init; }
            public string OutDir { get; init; }
            public uint ZoneId { get; init; }
            public string PropDefsPath { get; init; }
            public bool ReadyOnly { get; init; }
        }

        public sealed class Result
        {
            public int Claims { get; init; }
            public int Ready { get; init; }
            public int PropsResolved { get; init; }
            public int PropsUnresolved { get; init; }
            public int CellsAssigned { get; init; }
            public string PackageDir { get; init; }
        }

        public static Result Run(Options options)
        {
            if (!Directory.Exists(options.BinsDir))
                throw new DirectoryNotFoundException(options.BinsDir);
            if (options.ZoneId == 0)
                throw new ArgumentException("--zone is required (ClaimsBase records do not carry zone id)");

            PropModelIndex propIndex = LoadPropIndex(options.PropDefsPath);
            List<ClaimRecord> claims = LoadClaims(options.BinsDir);
            List<LightweightPropRecord> props = LoadProps(options.BinsDir);
            List<CreationExtractor.CellSource> cells = LoadCells(options.BinsDir);

            CreationExtractor.Result built = CreationExtractor.Build(claims, props, cells, propIndex);
            int ready = built.Report.Claims.Count(c => c.Ready);

            var manifest = new CreationManifest
            {
                ToolVersion = "1",
                ZoneId = options.ZoneId,
                SourcePath = Path.GetFullPath(options.BinsDir),
                CreatedUtc = DateTime.UtcNow.ToString("o"),
                ClaimCount = built.Claims.Count,
                ReadyCount = ready
            };

            Directory.CreateDirectory(options.OutDir);
            CreationPackage.Write(options.OutDir, manifest, built.Report, built.Claims, built.PropsByClaim,
                built.CellsByClaim, built.UnresolvedProps, options.ReadyOnly);

            return new Result
            {
                Claims = built.Claims.Count,
                Ready = ready,
                PropsResolved = built.PropsByClaim.Values.Sum(l => l.Count),
                PropsUnresolved = built.UnresolvedProps.Count,
                CellsAssigned = built.CellsByClaim.Values.Sum(l => l.Count),
                PackageDir = Path.GetFullPath(options.OutDir)
            };
        }

        private static PropModelIndex LoadPropIndex(string propDefsPath)
        {
            string path = propDefsPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(Directory.GetCurrentDirectory(),
                    "LandmarkEmulator.WorldServer", "Resources", "PacketDumps", "145-PropBase");
                if (!File.Exists(path))
                    path = Path.Combine(AppContext.BaseDirectory, "Resources", "PacketDumps", "145-PropBase");
            }
            if (!File.Exists(path))
            {
                Console.WriteLine($"[extract-creations] WARN: PropBase dump not found ({path}); props will stay unresolved");
                return PropModelIndex.FromPairs(Array.Empty<(uint, uint)>());
            }
            Console.WriteLine($"[extract-creations] Prop defs: {path}");
            return PropModelIndex.Load(path);
        }

        private static List<ClaimRecord> LoadClaims(string binsDir)
        {
            var byId = new Dictionary<ulong, ClaimRecord>();
            foreach (string file in EnumerateBins(binsDir, "ClaimsBase", "0xF900", "0x0000F900"))
            {
                byte[] raw = File.ReadAllBytes(file);
                byte[] body = StripOpcode(raw, 0xF9, 0x00);
                if (body.Length < 2 || body[0] != 4)
                    continue;
                try
                {
                    (List<ClaimRecord> added, List<ulong> removed, _) =
                        ClaimRecordCodec.ReadAoiUpdate(new GamePacketReader(body));
                    foreach (ClaimRecord c in added)
                        byId[c.Id] = c;
                    foreach (ulong id in removed)
                        byId.Remove(id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[extract-creations] skip claim bin {Path.GetFileName(file)}: {ex.Message}");
                }
            }
            return byId.Values.ToList();
        }

        private static List<LightweightPropRecord> LoadProps(string binsDir)
        {
            var props = new List<LightweightPropRecord>();
            foreach (string file in EnumerateBins(binsDir, "AddLightweightProp", "0x260062", "0x00620026"))
            {
                byte[] raw = File.ReadAllBytes(file);
                byte[] body = StripPlayerUpdateSub(raw, 0x62);
                try
                {
                    props.Add(LightweightPropCodec.Read(new GamePacketReader(body)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[extract-creations] skip prop bin {Path.GetFileName(file)}: {ex.Message}");
                }
            }
            return props;
        }

        private static List<CreationExtractor.CellSource> LoadCells(string binsDir)
        {
            var cells = new List<CreationExtractor.CellSource>();
            foreach (var record in TerrainCel2Packer.IndexCellsFromBins(binsDir).Values)
            {
                cells.Add(new CreationExtractor.CellSource
                {
                    CellId = record.CellId,
                    Version = record.Version,
                    FirstLayerId = record.FirstLayerId,
                    SegmentCount = record.SegmentCount,
                    DataBlob = record.DataBlob ?? Array.Empty<byte>()
                });
            }
            return cells;
        }

        private static IEnumerable<string> EnumerateBins(string binsDir, params string[] needles)
        {
            return Directory.GetFiles(binsDir, "*.bin")
                .Where(f =>
                {
                    string name = Path.GetFileName(f);
                    return needles.Any(n => name.Contains(n, StringComparison.OrdinalIgnoreCase));
                })
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Bins may start with u16 opcode (LE) or already be body-only.</summary>
        private static byte[] StripOpcode(byte[] raw, byte opLo, byte opHi)
        {
            if (raw.Length >= 2 && raw[0] == opLo && raw[1] == opHi)
                return raw.AsSpan(2).ToArray();
            return raw;
        }

        /// <summary>
        /// PlayerUpdate wire: u16 family 0x26 + u8 sub, or packed 0x260062 as 3 bytes LE, or body-only sub first.
        /// </summary>
        private static byte[] StripPlayerUpdateSub(byte[] raw, byte sub)
        {
            if (raw.Length >= 3 && raw[0] == 0x26 && raw[1] == 0x00 && raw[2] == sub)
                return raw.AsSpan(3).ToArray();
            if (raw.Length >= 1 && raw[0] == sub)
                return raw.AsSpan(1).ToArray();
            // Some dumps store the whole 0x260062 as u24 LE without a separate sub.
            if (raw.Length >= 3 && raw[0] == 0x62 && raw[1] == 0x00 && raw[2] == 0x26)
                return raw.AsSpan(3).ToArray();
            return raw;
        }
    }
}
