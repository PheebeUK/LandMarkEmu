using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.Shared.Network;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class LightweightPropCodecTests
    {
        [Fact]
        public void Write_Then_Read_RoundTripsInjectFields()
        {
            var original = new LightweightPropRecord
            {
                Guid = 0xABCD,
                TransientId = 12,
                Name = "Campfire",
                NameId = 100,
                ShowName = true,
                ModelId = 2,
                Position = new Vector4(10f, 20f, 30f, 1f),
                Rotation = new Vector4(0f, 0.5f, 0f, 0.866f),
                Scale = new Vector4(1.2f, 1.2f, 1.2f, 1f),
                OwnerGuid = 55,
                TintAlias = "GlobalTint_Coral",
                Collidable = true,
                ControlMode = 0,
                Enabled = true,
                StaticAreaId = 0,
                AreaGuid = 0,
                GroupGuid = 0
            };

            var bytes = new List<byte>();
            LightweightPropCodec.Write(new GamePacketWriter(bytes), original);
            LightweightPropRecord read = LightweightPropCodec.Read(new GamePacketReader(bytes.ToArray()));

            Assert.Equal(original.Guid, read.Guid);
            Assert.Equal(original.TransientId, read.TransientId);
            Assert.Equal(original.Name, read.Name);
            Assert.Equal(original.NameId, read.NameId);
            Assert.Equal(original.ModelId, read.ModelId);
            Assert.Equal(original.Position, read.Position);
            Assert.Equal(original.Rotation, read.Rotation);
            Assert.Equal(original.Scale, read.Scale);
            Assert.Equal(original.OwnerGuid, read.OwnerGuid);
            Assert.Equal(original.TintAlias, read.TintAlias);
            Assert.True(read.Collidable);
            Assert.True(read.Enabled);
        }
    }
}
