using LandmarkEmulator.Shared.Network;
using System;
using System.Collections.Generic;
using System.IO;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// Read/write of a ClaimsBase claim record body (same order as WorldServer <c>Claims.WriteRecord</c>).
    /// </summary>
    public static class ClaimRecordCodec
    {
        public static void Write(GamePacketWriter writer, ClaimRecord claim, ClaimRecordWriteExtras extras = null)
        {
            writer.Write(claim.Id);
            writer.Write(claim.ParentGroup);
            writer.Write(claim.OwnerGuid);
            writer.Write(claim.Name ?? "");
            writer.Write(0ul);
            writer.Write(claim.InfoName ?? claim.Name ?? "");
            writer.Write(claim.Description ?? "");
            writer.Write("");
            writer.Write(0u);
            writer.Write(claim.MinX);
            writer.Write(claim.MinY);
            writer.Write(claim.MinZ);
            writer.Write(claim.MaxX);
            writer.Write(claim.MaxY);
            writer.Write(claim.MaxZ);
            writer.Write(claim.DefaultPermissions);
            writer.Write(claim.GroupPermissions);
            writer.Write(claim.GuildPermissions);

            var players = claim.SpecialPlayers ?? new Dictionary<ulong, uint>();
            var names = claim.SpecialNames ?? new Dictionary<ulong, string>();
            writer.Write((uint)players.Count);
            foreach ((ulong guid, uint mask) in players)
            {
                writer.Write(guid);
                writer.Write(mask);
            }
            writer.Write(0u);
            writer.Write((uint)players.Count);
            foreach ((ulong guid, uint mask) in players)
            {
                writer.Write(guid);
                writer.Write(names.GetValueOrDefault(guid, ""));
                writer.Write(mask);
                writer.Write(false);
            }
            writer.Write(0u);
            writer.Write(claim.AdminTemplateGuid);
            writer.Write(false);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);

            string[] winner = claim.WinnerTexts ?? new string[5];
            for (int i = 0; i < 5; i++)
                writer.Write(i < winner.Length ? winner[i] ?? "" : "");
            writer.Write(0ul);

            if (extras?.WriteSiteGame != null)
                extras.WriteSiteGame(writer);
            else
                WriteEmptySiteGame(writer);

            writer.Write(0ul);
            writer.Write("");
            writer.Write(0u);
            writer.Write(claim.TimeHour);
            writer.Write(claim.TimeRate);
            writer.Write(claim.VisitorTimeHour);
            writer.Write(claim.VisitorTimeRate);
            writer.Write(0u);
            writer.Write(claim.Mode);
            writer.Write(0u);
            writer.Write(claim.AdventureFlags);
        }

        public static ClaimRecord Read(GamePacketReader reader)
        {
            var claim = new ClaimRecord
            {
                Id = reader.ReadULong(),
                ParentGroup = reader.ReadULong(),
                OwnerGuid = reader.ReadULong(),
                Name = reader.ReadString()
            };
            reader.ReadULong();
            claim.InfoName = reader.ReadString();
            claim.Description = reader.ReadString();
            reader.ReadString();
            reader.ReadUInt();
            claim.MinX = reader.ReadDouble();
            claim.MinY = reader.ReadDouble();
            claim.MinZ = reader.ReadDouble();
            claim.MaxX = reader.ReadDouble();
            claim.MaxY = reader.ReadDouble();
            claim.MaxZ = reader.ReadDouble();
            claim.DefaultPermissions = reader.ReadUInt();
            claim.GroupPermissions = reader.ReadUInt();
            claim.GuildPermissions = reader.ReadUInt();

            uint playerCount = reader.ReadUInt();
            for (uint i = 0; i < playerCount; i++)
            {
                ulong guid = reader.ReadULong();
                uint mask = reader.ReadUInt();
                claim.SpecialPlayers[guid] = mask;
            }
            reader.ReadUInt();
            uint namedCount = reader.ReadUInt();
            for (uint i = 0; i < namedCount; i++)
            {
                ulong guid = reader.ReadULong();
                string name = reader.ReadString();
                uint mask = reader.ReadUInt();
                reader.ReadBool();
                claim.SpecialNames[guid] = name;
                claim.SpecialPlayers[guid] = mask;
            }
            reader.ReadUInt();
            claim.AdminTemplateGuid = reader.ReadULong();
            reader.ReadBool();
            reader.ReadUInt();
            reader.ReadUInt();
            uint emptyList = reader.ReadUInt();
            for (uint i = 0; i < emptyList; i++)
            {
                // unexpected; skip nothing typed — list of unknown shape; treat as empty protocol
            }

            claim.WinnerTexts = new string[5];
            for (int i = 0; i < 5; i++)
                claim.WinnerTexts[i] = reader.ReadString();
            reader.ReadULong();

            SkipSiteGame(reader);

            reader.ReadULong();
            reader.ReadString();
            reader.ReadUInt();
            claim.TimeHour = reader.ReadInt();
            claim.TimeRate = reader.ReadSingle();
            claim.VisitorTimeHour = reader.ReadInt();
            claim.VisitorTimeRate = reader.ReadSingle();
            reader.ReadUInt();
            claim.Mode = reader.ReadUInt();
            reader.ReadUInt();
            claim.AdventureFlags = reader.ReadULong();
            return claim;
        }

        /// <summary>
        /// ClaimsBase AOIUpdate sub 4: bool full, u32 added × records, u32 removed × u64.
        /// Returns claims (last wins per id) and removed ids seen in this packet.
        /// </summary>
        public static (List<ClaimRecord> Added, List<ulong> Removed, bool Full) ReadAoiUpdate(GamePacketReader reader)
        {
            byte sub = reader.ReadByte();
            if (sub != 4)
                throw new InvalidDataException($"Expected ClaimsBase AOI sub 4, got 0x{sub:X2}");
            bool full = reader.ReadBool();
            uint addedCount = reader.ReadUInt();
            var added = new List<ClaimRecord>((int)addedCount);
            for (uint i = 0; i < addedCount; i++)
                added.Add(Read(reader));
            uint removedCount = reader.ReadUInt();
            var removed = new List<ulong>((int)removedCount);
            for (uint i = 0; i < removedCount; i++)
                removed.Add(reader.ReadULong());
            return (added, removed, full);
        }

        public static void WriteEmptySiteGame(GamePacketWriter writer)
        {
            writer.Write(0ul);
            writer.Write(0u);
            writer.Write(false);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(false);
            writer.Write(0u);
            writer.Write("");
            writer.Write(0u);
            writer.Write(0u);
        }

        private static void SkipSiteGame(GamePacketReader reader)
        {
            reader.ReadULong();
            reader.ReadUInt();
            reader.ReadBool();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadBool();
            reader.ReadUInt();
            reader.ReadString();
            reader.ReadUInt();
            reader.ReadUInt();
        }
    }
}
