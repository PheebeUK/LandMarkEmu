using System.Collections.Generic;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// Fields of a ClaimsBase AOI claim record that matter for capture extract / import
    /// (inverse of WorldServer <c>Claims.WriteRecord</c>).
    /// </summary>
    public sealed class ClaimRecord
    {
        public ulong Id { get; set; }
        public ulong ParentGroup { get; set; }
        public ulong OwnerGuid { get; set; }
        public string Name { get; set; } = "";
        public string InfoName { get; set; } = "";
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
        public Dictionary<ulong, uint> SpecialPlayers { get; set; } = new();
        public Dictionary<ulong, string> SpecialNames { get; set; } = new();
        public ulong AdminTemplateGuid { get; set; }
        public string[] WinnerTexts { get; set; } = ["", "", "", "", ""];
        public int TimeHour { get; set; } = -1;
        public float TimeRate { get; set; } = 1f;
        public int VisitorTimeHour { get; set; } = -1;
        public float VisitorTimeRate { get; set; } = 1f;
        public uint Mode { get; set; }
        public ulong AdventureFlags { get; set; }

        public ClaimBox Box => new(MinX, MinY, MinZ, MaxX, MaxY, MaxZ);
    }

    /// <summary>Optional extras written after the fixed claim fields (site-game blob).</summary>
    public sealed class ClaimRecordWriteExtras
    {
        /// <summary>
        /// Writes the InstancedGames site-game block (same layout as <c>InstancedGames.WriteInfo</c>).
        /// When null, an empty game block is written.
        /// </summary>
        public System.Action<LandmarkEmulator.Shared.Network.GamePacketWriter> WriteSiteGame { get; set; }
    }
}
