using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.Shared.Network;
using System.Collections.Generic;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class ClaimRecordCodecTests
    {
        [Fact]
        public void Write_Then_Read_RoundTripsCoreFields()
        {
            var original = new ClaimRecord
            {
                Id = 42,
                OwnerGuid = 0x1000,
                Name = "Test Site",
                InfoName = "Test Site",
                Description = "welcome",
                MinX = 1000, MinY = 2000, MinZ = 3000,
                MaxX = 3100, MaxY = 3800, MaxZ = 5100,
                DefaultPermissions = 0x003,
                GroupPermissions = 0,
                GuildPermissions = 0,
                TimeHour = 12,
                TimeRate = 1.5f,
                VisitorTimeHour = -1,
                VisitorTimeRate = 1f,
                Mode = 0,
                AdventureFlags = 0x3,
                SpecialPlayers = new Dictionary<ulong, uint> { [7] = 0x11F },
                SpecialNames = new Dictionary<ulong, string> { [7] = "Buddy" }
            };

            var bytes = new List<byte>();
            ClaimRecordCodec.Write(new GamePacketWriter(bytes), original);
            ClaimRecord read = ClaimRecordCodec.Read(new GamePacketReader(bytes.ToArray()));

            Assert.Equal(original.Id, read.Id);
            Assert.Equal(original.OwnerGuid, read.OwnerGuid);
            Assert.Equal(original.Name, read.Name);
            Assert.Equal(original.Description, read.Description);
            Assert.Equal(original.MinX, read.MinX);
            Assert.Equal(original.MinY, read.MinY);
            Assert.Equal(original.MinZ, read.MinZ);
            Assert.Equal(original.MaxX, read.MaxX);
            Assert.Equal(original.MaxY, read.MaxY);
            Assert.Equal(original.MaxZ, read.MaxZ);
            Assert.Equal(original.DefaultPermissions, read.DefaultPermissions);
            Assert.Equal(original.TimeHour, read.TimeHour);
            Assert.Equal(original.TimeRate, read.TimeRate);
            Assert.Equal(original.AdventureFlags, read.AdventureFlags);
            Assert.Equal(0x11Fu, read.SpecialPlayers[7]);
            Assert.Equal("Buddy", read.SpecialNames[7]);
        }

        [Fact]
        public void ReadAoiUpdate_ParsesSub4()
        {
            var claim = new ClaimRecord
            {
                Id = 9, OwnerGuid = 1, Name = "A", InfoName = "A",
                MinX = 0, MinY = 0, MinZ = 0, MaxX = 10, MaxY = 10, MaxZ = 10
            };
            var body = new List<byte>();
            var w = new GamePacketWriter(body);
            w.Write((byte)4);
            w.Write(true);
            w.Write(1u);
            ClaimRecordCodec.Write(w, claim);
            w.Write(1u);
            w.Write(99ul);

            (List<ClaimRecord> added, List<ulong> removed, bool full) =
                ClaimRecordCodec.ReadAoiUpdate(new GamePacketReader(body.ToArray()));
            Assert.True(full);
            Assert.Equal(9ul, Assert.Single(added).Id);
            Assert.Equal(99ul, Assert.Single(removed));
        }
    }
}
