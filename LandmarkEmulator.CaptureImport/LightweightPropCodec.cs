using LandmarkEmulator.Shared.Network;
using System;
using System.Numerics;

namespace LandmarkEmulator.CaptureImport
{
    /// <summary>
    /// Read/write of PlayerUpdate AddLightweightProp wire body (mirrors
    /// <c>PlayerUpdateAddLightweightProp.Write</c>).
    /// </summary>
    public static class LightweightPropCodec
    {
        public static readonly (double X, double Y, double Z) ZoneOrigin =
            (TerrainMath.ZoneOriginX, TerrainMath.ZoneOriginY, TerrainMath.ZoneOriginZ);

        public static void Write(GamePacketWriter writer, LightweightPropRecord prop)
        {
            writer.Write(prop.Guid);
            writer.WriteUIntWith2BitLength((int)prop.TransientId);
            writer.Write("");
            writer.Write(prop.Name ?? "");
            writer.Write(prop.ShowName ? prop.NameId : 0u);
            writer.Write(0u);
            writer.Write((byte)0);
            writer.Write(prop.ModelId);
            writer.Write(prop.Scale);
            writer.Write("");
            writer.Write(prop.TintAlias ?? "");
            writer.Write("");
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(prop.Position);
            writer.Write(prop.Rotation);
            writer.Write(new Vector4(0f, 0f, 0f, 0f));
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write("");
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write((byte)prop.ControlMode);
            writer.Write(0u);
            writer.Write(false);
            writer.WriteBytes(new byte[] { 0, (byte)(prop.Collidable ? 0x08 : 0), 0 });
            writer.Write((byte)0);
            writer.Write(0u);
            writer.Write(0ul);
            writer.Write(0ul);
            writer.Write(ZoneOrigin.X);
            writer.Write(ZoneOrigin.Y);
            writer.Write(ZoneOrigin.Z);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(prop.StaticAreaId);
            writer.Write(prop.AreaGuid);
            writer.Write(prop.GroupGuid);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write((byte)0);
            writer.Write(0u);
            uint[] animations = prop.SpawnAnimations ?? Array.Empty<uint>();
            writer.Write((uint)animations.Length);
            foreach (uint selection in animations)
            {
                writer.Write(selection);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
            writer.Write(prop.OwnerGuid);
            writer.Write((byte)1);
            writer.Write(0u);
            writer.Write((byte)0);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0u);
            writer.Write(0ul);
            writer.Write(prop.Enabled);
        }

        public static LightweightPropRecord Read(GamePacketReader reader)
        {
            var prop = new LightweightPropRecord
            {
                Guid = reader.ReadULong(),
                TransientId = reader.ReadUInt2BitLength()
            };
            reader.ReadString();
            prop.Name = reader.ReadString();
            prop.NameId = reader.ReadUInt();
            prop.ShowName = prop.NameId != 0;
            reader.ReadUInt();
            reader.ReadByte();
            prop.ModelId = reader.ReadUInt();
            prop.Scale = ReadVector4(reader);
            reader.ReadString();
            prop.TintAlias = reader.ReadString();
            reader.ReadString();
            reader.ReadUInt();
            reader.ReadUInt();
            prop.Position = ReadVector4(reader);
            prop.Rotation = ReadVector4(reader);
            ReadVector4(reader);
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadString();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            prop.ControlMode = (sbyte)reader.ReadByte();
            reader.ReadUInt();
            reader.ReadBool();
            reader.ReadByte();
            byte flags1 = reader.ReadByte();
            reader.ReadByte();
            prop.Collidable = (flags1 & 0x08) != 0;
            reader.ReadByte();
            reader.ReadUInt();
            reader.ReadULong();
            reader.ReadULong();
            reader.ReadDouble();
            reader.ReadDouble();
            reader.ReadDouble();
            reader.ReadUInt();
            reader.ReadUInt();
            prop.StaticAreaId = reader.ReadUInt();
            prop.AreaGuid = reader.ReadULong();
            prop.GroupGuid = reader.ReadULong();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadByte();
            reader.ReadUInt();
            uint animCount = reader.ReadUInt();
            if (animCount > 0 && animCount < 256)
            {
                prop.SpawnAnimations = new uint[animCount];
                for (uint i = 0; i < animCount; i++)
                {
                    prop.SpawnAnimations[i] = reader.ReadUInt();
                    reader.ReadByte();
                    reader.ReadByte();
                }
            }
            prop.OwnerGuid = reader.ReadULong();
            reader.ReadByte();
            reader.ReadUInt();
            reader.ReadByte();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadUInt();
            reader.ReadULong();
            prop.Enabled = reader.ReadBool();
            return prop;
        }

        private static Vector4 ReadVector4(GamePacketReader reader)
            => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
    }
}
