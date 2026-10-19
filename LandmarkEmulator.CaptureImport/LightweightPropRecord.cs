using System.Numerics;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>Fields of PlayerUpdate AddLightweightProp (0x26/0x62) used for creation inject.</summary>
    public sealed class LightweightPropRecord
    {
        public ulong Guid { get; set; }
        public uint TransientId { get; set; }
        public string Name { get; set; } = "";
        public uint NameId { get; set; }
        public bool ShowName { get; set; }
        public uint ModelId { get; set; }
        public Vector4 Position { get; set; }
        public Vector4 Rotation { get; set; } = new(0f, 0f, 0f, 1f);
        public Vector4 Scale { get; set; } = new(1f, 1f, 1f, 1f);
        public ulong OwnerGuid { get; set; }
        public string TintAlias { get; set; } = "";
        public bool Collidable { get; set; } = true;
        public sbyte ControlMode { get; set; }
        public bool Enabled { get; set; } = true;
        public ulong AreaGuid { get; set; }
        public uint StaticAreaId { get; set; }
        public ulong GroupGuid { get; set; }
        public uint[] SpawnAnimations { get; set; }

        /// <summary>Resolved PropDefs id (0 when unresolved).</summary>
        public uint PropId { get; set; }
        public bool PropIdAmbiguous { get; set; }
    }
}
