using Dash.Engine;
using Dash.Net;
using System;
using System.Collections.Generic;
using System.IO;

namespace AceOfSpades.IO
{
    public class WorldObjectDescription
    {
        enum PrimitiveType : byte
        {
            None,

            Byte,
            SByte,

            Char,
            Boolean,

            Int16,
            UInt16,

            Int32,
            UInt32,

            Int64,
            UInt64,

            Single,
            Double,

            ByteFlag
        }

        public string Tag;

        Dictionary<string, object> fields;

        public WorldObjectDescription()
        {
            Tag = "";
            fields = new Dictionary<string, object>();
        }

        public WorldObjectDescription(string tag)
        {
            Tag = tag;
            fields = new Dictionary<string, object>();
        }

        public void AddField<T>(string name, T value)
            where T : struct
        {
            fields.Add(name, value);
        }

        public void AddField(string name, Vector3 vec3)
        {
            fields.Add(name + ".X", vec3.X);
            fields.Add(name + ".Y", vec3.Y);
            fields.Add(name + ".Z", vec3.Z);
        }

        public Vector3 GetVector3(string name)
        {
            float x = GetField<float>(name + ".X") ?? 0;
            float y = GetField<float>(name + ".Y") ?? 0;
            float z = GetField<float>(name + ".Z") ?? 0;

            return new Vector3(x, y, z);
        }

        public T? GetField<T>(string name)
            where T : struct
        {
            object v;
            if (fields.TryGetValue(name, out v))
                return (T)v;
            else
                return null;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Tag);
            writer.Write((ushort)fields.Count);
            foreach (KeyValuePair<string, object> pair in fields)
            {
                writer.Write(pair.Key);
                PrimitiveType type = GetPrimitiveType(pair.Value.GetType());
                writer.Write((byte)type);
                writer.Write((dynamic)pair.Value);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            Tag = reader.ReadString();
            int numFields = reader.ReadUInt16();
            for (int i = 0; i < numFields; i++)
            {
                string key = reader.ReadString();
                PrimitiveType type = (PrimitiveType)reader.ReadByte();
                object value = ReadPrimitive(reader, type);

                fields.Add(key, value);
            }
        }

        static object ReadPrimitive(BinaryReader reader, PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Byte:
                    return reader.ReadByte();
                case PrimitiveType.Char:
                    return reader.ReadChar();
                case PrimitiveType.Boolean:
                    return reader.ReadBoolean();
                case PrimitiveType.Int16:
                case PrimitiveType.SByte: // SByte is written as a short
                    return reader.ReadInt16();
                case PrimitiveType.UInt16:
                    return reader.ReadUInt16();
                case PrimitiveType.Int32:
                    return reader.ReadInt32();
                case PrimitiveType.UInt32:
                    return reader.ReadUInt32();
                case PrimitiveType.Int64:
                    return reader.ReadInt64();
                case PrimitiveType.UInt64:
                    return reader.ReadUInt64();
                case PrimitiveType.Single:
                    return reader.ReadSingle();
                case PrimitiveType.Double:
                    return reader.ReadDouble();
                case PrimitiveType.ByteFlag:
                    return new ByteFlag(reader.ReadByte());
                default:
                    throw new Exception("WorldFile primitive type '" + type + "' is not supported!");
            }
        }

        static PrimitiveType GetPrimitiveType(Type type)
        {
            if (type == typeof(byte)) return PrimitiveType.Byte;
            else if (type == typeof(sbyte)) return PrimitiveType.SByte;
            else if (type == typeof(char)) return PrimitiveType.Char;
            else if (type == typeof(bool)) return PrimitiveType.Boolean;
            else if (type == typeof(short)) return PrimitiveType.Int16;
            else if (type == typeof(ushort)) return PrimitiveType.UInt16;
            else if (type == typeof(int)) return PrimitiveType.Int32;
            else if (type == typeof(uint)) return PrimitiveType.UInt32;
            else if (type == typeof(long)) return PrimitiveType.Int64;
            else if (type == typeof(ulong)) return PrimitiveType.UInt64;
            else if (type == typeof(float)) return PrimitiveType.Single;
            else if (type == typeof(double)) return PrimitiveType.Double;
            else if (type == typeof(ByteFlag)) return PrimitiveType.ByteFlag;
            else throw new Exception("WorldFile primitive type '" + type.FullName + "' is not supported!");
        }
    }
}
