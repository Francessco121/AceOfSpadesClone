using Dash.Net;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace AceOfSpades.Net
{
    /* Serialized Snapshot 
        
        ushort: fields

        foreach field : SnapshotField
            byte: id
            byte: type
            T: value
    */

    public abstract class Snapshot
    {
        enum FieldType : byte
        {
            Array,
            NestedSnapshot,

            Bool,
            Float,
            Byte,
            ByteFlag,
            UInt16
        }

        class SnapshotField
        {
            public byte Id;
            public FieldType Type;
            public object Value;

            public SnapshotField(byte id, FieldType type, object value)
            {
                Id = id;
                Type = type;
                Value = value;
            }
        }

        OrderedDictionary fields;
        List<int> forceSendFields;

        public Snapshot()
        {
            fields = new OrderedDictionary();
            forceSendFields = new List<int>();
        }

        public void InvalidateFields(params string[] names)
        {
            object[] keys = new object[fields.Keys.Count];
            fields.Keys.CopyTo(keys, 0);

            for (int i = 0; i < fields.Count; i++)
            {
                for (int k = 0; k < names.Length; k++)
                    if (names[k].Equals(keys[i]))
                    {
                        forceSendFields.Add(i);
                        break;
                    }
            }
        }

        public void AddField<T>(string name)
            where T : struct
        {
            FieldType type = GetFieldType(typeof(T));
            fields.Add(name, default(T));
        }

        protected void AddArrayField<T>(string name, int length)
             where T : struct
        {
            if (typeof(Array).IsAssignableFrom(typeof(T)))
                throw new Exception("Snapshot.AddArrayField cannot take an Array type! Specify the type of values instead.");

            for (int i = 0; i < length; i++)
                AddField<T>(string.Format("{0}[{1}]", name, i));
        }

        public void Set<T>(string name, T value)
             where T : struct
        {
            fields[name] = value;
        }

        public T Get<T>(string name)
            where T : struct
        {
            return (T)fields[name];
        }

        public T GetArrayValue<T>(string name, int i)
            where T : struct
        {
            return (T)fields[string.Format("{0}[{1}]", name, i)];
        }

        public object this[int index]
        {
            get { return fields[index]; }
            set { fields[index] = value; }
        }

        /*
            Array
            byte index
            byte type
            object data...

            NestedSnapshot
            byte snapshotType
            byte type
            object data...

            NestedSnapshot in Array
            byte index
            byte NestedSnapshot
            byte type
            object data...
        */

        public virtual void Serialize(NetBuffer buffer, Snapshot prev)
        {
            if (prev != null && GetType() != prev.GetType())
                throw new InvalidOperationException("Cannot get delta diff of two different snapshot types.");

            // Get delta diff
            List<SnapshotField> deltaFields = new List<SnapshotField>();

            for (int i = 0; i < fields.Count; i++)
            {
                object a = fields[i];

                if (prev != null && !forceSendFields.Contains(i))
                {
                    if (prev.fields.Count > i)
                    {
                        object b = prev.fields[i];
                        if (!a.Equals(b))
                            deltaFields.Add(new SnapshotField((byte)i, GetFieldType(a.GetType()), a));
                    }
                    else
                        deltaFields.Add(new SnapshotField((byte)i, GetFieldType(a.GetType()), a));
                }
                else
                    deltaFields.Add(new SnapshotField((byte)i, GetFieldType(a.GetType()), a));
            }

            forceSendFields.Clear();

            // Write delta diff
            buffer.Write((ushort)deltaFields.Count);
            for (int i = 0; i < deltaFields.Count; i++)
            {
                SnapshotField f = deltaFields[i];
                buffer.Write(f.Id);
                buffer.Write((byte)f.Type);
                buffer.WriteDynamic(f.Value);
            }
        }

        public virtual bool Deserialize(NetInboundPacket packet)
        {
            int fieldCount = packet.ReadUInt16();
            bool readAll = true;
            for (int i = 0; i < fieldCount; i++)
            {
                byte fieldIndex = packet.ReadByte();
                byte dataType = packet.ReadByte();
                object data = ReadField(packet, dataType);

                if (fieldIndex < fields.Count)
                    fields[fieldIndex] = data;
                else
                    readAll = false;
            }

            return readAll;
        }

        FieldType GetFieldType(Type type)
        {
            if (type == typeof(bool)) return FieldType.Bool;
            else if (type == typeof(float)) return FieldType.Float;
            else if (type == typeof(byte)) return FieldType.Byte;
            else if (type == typeof(ByteFlag)) return FieldType.ByteFlag;
            else if (type == typeof(ushort)) return FieldType.UInt16;
            else throw new InvalidOperationException("Failed to get type id of type: " + type);
        }

        object ReadField(NetInboundPacket packet, byte type)
        {
            byte innerType;

            switch ((FieldType)type)
            {
                case FieldType.Bool:
                    return packet.ReadBool();
                case FieldType.Float:
                    return packet.ReadFloat();
                case FieldType.Byte:
                    return packet.ReadByte();
                case FieldType.ByteFlag:
                    return packet.ReadByteFlag();
                case FieldType.UInt16:
                    return packet.ReadUInt16();
                case FieldType.NestedSnapshot:
                    // Nested snapshot
                    byte id = packet.ReadByte();
                    innerType = packet.ReadByte();
                    return ReadField(packet, innerType);
                case FieldType.Array: 
                    // Array
                    byte index = packet.ReadByte();
                    innerType = packet.ReadByte();
                    return ReadField(packet, innerType);
                default:
                    throw new InvalidOperationException("Failed to read field of type id: " + type);
            }
        }
    }
}
