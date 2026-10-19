using LandmarkEmulator.Shared.Network;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// PropId ↔ ModelId index built from a PropBase dump (same layout as <c>Props.LoadDefinitions</c>).
    /// </summary>
    public sealed class PropModelIndex
    {
        private readonly Dictionary<uint, uint> propToModel = new();
        private readonly Dictionary<uint, List<uint>> modelToProps = new();

        public int Count => propToModel.Count;

        public static PropModelIndex Load(string dumpPath)
        {
            if (!File.Exists(dumpPath))
                throw new FileNotFoundException(dumpPath);
            return FromBytes(File.ReadAllBytes(dumpPath));
        }

        /// <summary>
        /// PropBase dump body: u8 sub, u64, u8, u32 count, then per prop
        /// {string modelFile, u32 propId, string, string, f32, f32, u32, u32, u32, u32 modelId}.
        /// Optional leading opcode bytes <c>ED 00</c> are skipped when present.
        /// </summary>
        public static PropModelIndex FromBytes(byte[] dump)
        {
            var reader = new GamePacketReader(dump);
            if (dump.Length >= 2 && dump[0] == 0xED && dump[1] == 0x00)
                reader.ReadBytes(2);
            // sub + u64 + u8
            reader.ReadByte();
            reader.ReadULong();
            reader.ReadByte();

            uint count = reader.ReadUInt();
            var index = new PropModelIndex();
            for (uint i = 0; i < count; i++)
            {
                reader.ReadString();
                uint propId = reader.ReadUInt();
                reader.ReadString();
                reader.ReadString();
                reader.ReadSingle();
                reader.ReadSingle();
                reader.ReadUInt();
                reader.ReadUInt();
                reader.ReadUInt();
                uint modelId = reader.ReadUInt();
                index.Add(propId, modelId);
            }
            return index;
        }

        /// <summary>Minimal index for tests: explicit prop→model pairs.</summary>
        public static PropModelIndex FromPairs(IEnumerable<(uint PropId, uint ModelId)> pairs)
        {
            var index = new PropModelIndex();
            foreach ((uint propId, uint modelId) in pairs)
                index.Add(propId, modelId);
            return index;
        }

        public void Add(uint propId, uint modelId)
        {
            if (!propToModel.ContainsKey(propId))
                propToModel[propId] = modelId;
            if (!modelToProps.TryGetValue(modelId, out List<uint> list))
                modelToProps[modelId] = list = new List<uint>();
            if (!list.Contains(propId))
                list.Add(propId);
        }

        public uint ModelOf(uint propId) => propToModel.TryGetValue(propId, out uint m) ? m : 0;

        /// <summary>
        /// Resolve ModelId → PropId. On collision picks the lowest prop id and sets <paramref name="ambiguous"/>.
        /// </summary>
        public bool TryResolve(uint modelId, out uint propId, out bool ambiguous)
        {
            ambiguous = false;
            propId = 0;
            if (!modelToProps.TryGetValue(modelId, out List<uint> list) || list.Count == 0)
                return false;
            ambiguous = list.Count > 1;
            propId = list.Min();
            return true;
        }
    }
}
