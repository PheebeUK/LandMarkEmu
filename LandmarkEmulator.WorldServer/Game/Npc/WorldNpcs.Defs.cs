using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using NLog;

namespace LandmarkEmulator.WorldServer.Game.Npc
{
    public static partial class WorldNpcs
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        /// <summary>One world NPC definition (Resources/Npcs/*.json).</summary>
        public sealed class NpcDef
        {
            public string Id { get; set; } = "";
            public string DisplayName { get; set; } = "NPC";
            /// <summary>Spawner name fragment for Creatures.SpawnStory kind resolution (e.g. Kobold).</summary>
            public string SpawnerName { get; set; } = "Spawner: Kobold";
            /// <summary>0 = home continent at load time.</summary>
            public uint Zone { get; set; }
            public float[] Home { get; set; } = { 20f, 0f, 20f };
            public float Yaw { get; set; }
            public float HailRange { get; set; } = 14f;
            public float[][] Waypoints { get; set; } = Array.Empty<float[]>();
            public float DwellSeconds { get; set; } = 4f;
            /// <summary>Custom achievement id (Achievements.NpcQuests), 0 = dialogue only.</summary>
            public uint QuestAchievementId { get; set; }
            public uint QuestObjectiveId { get; set; }
            public uint QuestRequired { get; set; } = 3;
            /// <summary>NPC id whose turned_in flag must be set before this NPC's full dialogue unlocks.</summary>
            public string RequiresNpcTurnedIn { get; set; }
            public Dictionary<string, LineDef> Lines { get; set; } = new();

            public Vector3 HomePosition()
            {
                float x = Home.Length > 0 ? Home[0] : 0f;
                float y = Home.Length > 1 ? Home[1] : 0f;
                float z = Home.Length > 2 ? Home[2] : 0f;
                return new Vector3(x, y, z);
            }

            public IEnumerable<Vector3> WaypointPositions()
            {
                foreach (float[] w in Waypoints ?? Array.Empty<float[]>())
                    if (w != null && w.Length >= 3)
                        yield return new Vector3(w[0], w[1], w[2]);
            }
        }

        public sealed class LineDef
        {
            public string Text { get; set; } = "";
            /// <summary>DisplayPropText mode: 3 chat, 4 bubble, 5 bubble+chat.</summary>
            public uint Mode { get; set; } = 5;
            public uint Seconds { get; set; } = 8;
            public bool Question { get; set; }
        }

        private static Dictionary<string, NpcDef> definitions = new(StringComparer.OrdinalIgnoreCase);

        private static string DefsFolder()
            => Path.Combine(AppContext.BaseDirectory, "Resources", "Npcs");

        /// <summary>Loads or reloads every JSON definition under Resources/Npcs.</summary>
        public static void LoadDefinitions()
        {
            var next = new Dictionary<string, NpcDef>(StringComparer.OrdinalIgnoreCase);
            string folder = DefsFolder();
            if (!Directory.Exists(folder))
            {
                log.Warn($"World NPCs: no definitions folder at {folder}.");
                definitions = next;
                return;
            }
            foreach (string path in Directory.EnumerateFiles(folder, "*.json"))
            {
                try
                {
                    NpcDef def = JsonSerializer.Deserialize<NpcDef>(File.ReadAllText(path), jsonOptions);
                    if (def == null || string.IsNullOrWhiteSpace(def.Id))
                    {
                        log.Warn($"World NPCs: skipped {path} (missing id).");
                        continue;
                    }
                    next[def.Id] = def;
                }
                catch (Exception e)
                {
                    log.Error(e, $"World NPCs: failed to read {path}.");
                }
            }
            definitions = next;
            log.Info($"World NPCs: {definitions.Count} definition(s) from {folder}.");
        }

        public static bool TryGetDef(string id, out NpcDef def) => definitions.TryGetValue(id ?? "", out def);

        public static IEnumerable<NpcDef> AllDefs() => definitions.Values;
    }
}
