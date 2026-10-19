using System;
using System.Collections.Generic;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// Walks a CellsUpdate cell's layered blob (same layout as Parser <c>TerrainCel2Packer.CellRecord</c>)
    /// to pull out the build layer (id 1).
    /// </summary>
    public static class CellLayerExtract
    {
        public const uint BuildLayerId = 1;

        public sealed class Layer
        {
            public uint Id { get; init; }
            public uint Version { get; init; }
            public byte Flag { get; init; }
            public byte[] Data { get; init; } = Array.Empty<byte>();
        }

        /// <summary>
        /// <paramref name="firstLayerId"/> plus <paramref name="dataBlob"/> as stored on a CellRecord:
        /// first layer has no id in the blob (only version, flag, size, data); further layers are
        /// {u32 id, u32 version, u8 flag, u32 size, data}.
        /// </summary>
        public static List<Layer> ParseLayers(uint firstLayerId, uint segmentCount, byte[] dataBlob)
        {
            var layers = new List<Layer>();
            if (segmentCount == 0 || dataBlob == null || dataBlob.Length == 0)
                return layers;

            int pos = 0;
            for (uint i = 0; i < segmentCount; i++)
            {
                uint layerId;
                if (i == 0)
                    layerId = firstLayerId;
                else
                {
                    if (pos + 4 > dataBlob.Length)
                        break;
                    layerId = BitConverter.ToUInt32(dataBlob, pos);
                    pos += 4;
                }
                if (pos + 9 > dataBlob.Length)
                    break;
                uint version = BitConverter.ToUInt32(dataBlob, pos);
                byte flag = dataBlob[pos + 4];
                uint size = BitConverter.ToUInt32(dataBlob, pos + 5);
                pos += 9;
                if (size > 16 * 1024 * 1024 || pos + (int)size > dataBlob.Length)
                    break;
                byte[] data = new byte[size];
                Buffer.BlockCopy(dataBlob, pos, data, 0, (int)size);
                pos += (int)size;
                layers.Add(new Layer { Id = layerId, Version = version, Flag = flag, Data = data });
            }
            return layers;
        }

        public static CapturedCell ToCaptured(ulong cellId, uint version, uint firstLayerId, uint segmentCount, byte[] dataBlob)
        {
            List<Layer> layers = ParseLayers(firstLayerId, segmentCount, dataBlob);
            Layer build = layers.Find(l => l.Id == BuildLayerId);
            return new CapturedCell
            {
                CellId = cellId,
                Version = version,
                HasBuildLayer = build != null && build.Data.Length > 0,
                BuildLayerData = build?.Data ?? Array.Empty<byte>()
            };
        }
    }
}
