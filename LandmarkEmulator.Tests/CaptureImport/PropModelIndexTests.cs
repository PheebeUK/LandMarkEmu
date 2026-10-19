using LandmarkEmulator.CaptureImport;
using LandmarkEmulator.Shared.Network;
using System.Collections.Generic;
using Xunit;

namespace LandmarkEmulator.Tests.CaptureImport
{
    public class PropModelIndexTests
    {
        [Fact]
        public void FromPairs_ResolvesUniqueAndAmbiguous()
        {
            var index = PropModelIndex.FromPairs(new[]
            {
                (10u, 100u),
                (20u, 200u),
                (5u, 200u)
            });

            Assert.True(index.TryResolve(100, out uint a, out bool ambA));
            Assert.Equal(10u, a);
            Assert.False(ambA);

            Assert.True(index.TryResolve(200, out uint b, out bool ambB));
            Assert.Equal(5u, b);
            Assert.True(ambB);

            Assert.False(index.TryResolve(999, out _, out _));
        }

        [Fact]
        public void FromBytes_ReadsPropBaseShapedDump()
        {
            var body = new List<byte>();
            var w = new GamePacketWriter(body);
            w.Write((byte)0xED);
            w.Write((byte)0x00);
            w.Write((byte)1);
            w.Write(0ul);
            w.Write((byte)0);
            w.Write(2u);
            WriteProp(w, "a.adr", 11, 111);
            WriteProp(w, "b.adr", 22, 222);

            var index = PropModelIndex.FromBytes(body.ToArray());
            Assert.Equal(2, index.Count);
            Assert.True(index.TryResolve(111, out uint p, out _));
            Assert.Equal(11u, p);
            Assert.Equal(222u, index.ModelOf(22));
        }

        private static void WriteProp(GamePacketWriter w, string model, uint propId, uint modelId)
        {
            w.Write(model);
            w.Write(propId);
            w.Write("internal");
            w.Write("desc");
            w.Write(1f);
            w.Write(1f);
            w.Write(0u);
            w.Write(0u);
            w.Write(0u);
            w.Write(modelId);
        }
    }
}
