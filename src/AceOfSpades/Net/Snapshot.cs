using Dash.Net;
using System;
using System.Collections.Generic;

/* Snapshot.cs
 * Ethan Lafrenais
*/

namespace AceOfSpades.Net
{
    public abstract partial class Snapshot : IDisposable
    {
        class TempSnapshot : Snapshot
        {
            public TempSnapshot(SnapshotSystem snapshotSystem) 
                : base(snapshotSystem, null, true, true)
            { }

            public override string GetUniqueId() { throw new NotImplementedException(); }
        }

        public Snapshot Parent { get; private set; }

        public ushort Id { get; private set; }
        public NetConnection OtherConnection { get; }

        public IEnumerable<StaticSnapshotField> StaticFields
        {
            get { return staticFields.Values; }
        }

        public IEnumerable<DynamicSnapshotField> DynamicFields
        {
            get { return dynamicFields.Values; }
        }

        public bool IsReady { get; private set; }
        public bool IsAppOwner { get; private set; }

        protected readonly SnapshotSystem snapshotSystem;
        Dictionary<ushort, StaticSnapshotField> staticFields;
        Dictionary<object, DynamicSnapshotField> dynamicFields;
        ushort currentFieldId;

        bool dontAllocateId;
        bool dontAwait;

        public Snapshot(SnapshotSystem snapshotSystem, NetConnection otherConnection, 
            bool dontAllocateId = false, bool dontAwait = false)
        {
            this.snapshotSystem = snapshotSystem;
            this.dontAllocateId = dontAllocateId;
            this.dontAwait      = dontAwait;

            OtherConnection = otherConnection;

            staticFields  = new Dictionary<ushort, StaticSnapshotField>();
            dynamicFields = new Dictionary<object, DynamicSnapshotField>();
        }

        protected void Setup()
        {
            if (IsReady)
                throw new Exception("Cannot setup snapshot twice!");

            if (!dontAllocateId)
            {
                Id = snapshotSystem.Allocate(this);
                IsReady = true;
            }
            else if (!dontAwait)
                snapshotSystem.AwaitAllocation(this);

            IsAppOwner = !dontAllocateId;
        }

        public void SetId(ushort id)
        {
            if (IsReady)
                throw new InvalidOperationException("Snapshot already has an allocated id!");

            Id = id;
            IsReady = true;
        }

        public abstract string GetUniqueId();

        protected StaticSnapshotField AddPrimitiveField<T>(T defaultValue = default(T))
            where T : struct
        {
            if (IsReady)
                throw new InvalidOperationException("Cannot add static snapshot field after initialization!");

            ushort id = currentFieldId++;
            SnapshotPrimitiveType primType = GetPrimitiveType(defaultValue.GetType());
            StaticSnapshotField field = new StaticSnapshotField(this, id, SnapshotFieldType.Primitive, 
                defaultValue, primType);

            staticFields.Add(id, field);
            return field;
        }

        protected DynamicSnapshotField AddPrimitiveField<T, K>(K dynamicKey, T defaultValue = default(T))
            where T : struct
            where K : struct
        {
            SnapshotPrimitiveType primType = GetPrimitiveType(defaultValue.GetType());
            DynamicSnapshotField field = new DynamicSnapshotField(this, dynamicKey, SnapshotFieldType.Primitive, 
                defaultValue, GetPrimitiveType(typeof(K)), primType);

            dynamicFields.Add(dynamicKey, field);
            return field;
        }

        protected StaticSnapshotField AddTrigger()
        {
            if (IsReady)
                throw new InvalidOperationException("Cannot add static snapshot field after initialization!");

            ushort id = currentFieldId++;
            StaticSnapshotField field = new StaticSnapshotField(this, id, SnapshotFieldType.Trigger, 
                new Trigger());

            staticFields.Add(id, field);
            return field;
        }

        protected DynamicSnapshotField AddTrigger<K>(K dynamicKey)
            where K : struct
        {
            DynamicSnapshotField field = new DynamicSnapshotField(this, dynamicKey, SnapshotFieldType.Trigger, 
                new Trigger(), GetPrimitiveType(typeof(K)));

            dynamicFields.Add(dynamicKey, field);
            return field;
        }

        protected StaticSnapshotField AddNestedField(Snapshot child)
        {
            if (IsReady)
                throw new InvalidOperationException("Cannot add static snapshot field after initialization!");

            ushort id = currentFieldId++;
            StaticSnapshotField field = new StaticSnapshotField(this, id, SnapshotFieldType.Snapshot, child);
            child.Parent = this;

            staticFields.Add(id, field);
            return field;
        }

        protected DynamicSnapshotField AddNestedField<K>(K dynamicKey, Snapshot child)
            where K : struct
        {
            DynamicSnapshotField field = new DynamicSnapshotField(this, dynamicKey, SnapshotFieldType.Snapshot, 
                child, GetPrimitiveType(typeof(K)));
            child.Parent = this;

            dynamicFields.Add(dynamicKey, field);
            return field;
        }

        protected StaticSnapshotField AddCustomField(CustomSnapshot child)
        {
            if (IsReady)
                throw new InvalidOperationException("Cannot add static snapshot field after initialization!");

            ushort id = currentFieldId++;
            StaticSnapshotField field = new StaticSnapshotField(this, id, SnapshotFieldType.Custom, child);

            staticFields.Add(id, field);
            return field;
        }

        protected DynamicSnapshotField AddCustomField<K>(K dynamicKey, CustomSnapshot child)
            where K : struct
        {
            DynamicSnapshotField field = new DynamicSnapshotField(this, dynamicKey, SnapshotFieldType.Custom, 
                child, GetPrimitiveType(typeof(K)));

            dynamicFields.Add(dynamicKey, field);
            return field;
        }

        protected bool RemoveDynamicField(DynamicSnapshotField field)
        {
            if (field.Type == SnapshotFieldType.Snapshot)
            {
                // Make sure we dispose of a delta supporting snapshots
                Snapshot snapshot = (Snapshot)field.Value;
                snapshot.Dispose();
            }

            return dynamicFields.Remove(field.Id);
        }

        public virtual void Serialize(NetBuffer buffer)
        {
            DeltaSnapshot prev = GetLastValidSnapshot();

            // Temporarily write each static field to get accurate count
            int writtenStaticFields = 0;
            NetBuffer tempStaticBuffer = new NetBuffer();
            foreach (KeyValuePair<ushort, StaticSnapshotField> pair in staticFields)
                if (SerializeStaticField(pair.Value, tempStaticBuffer, prev))
                    writtenStaticFields++;

            // Temporarily write each dynamic field to get accurate count
            int writtenDynamicFields = 0;
            NetBuffer tempDynamicBuffer = new NetBuffer();
            foreach (KeyValuePair<object, DynamicSnapshotField> pair in dynamicFields)
                if (SerializeDynamicField(pair.Value, tempDynamicBuffer, prev))
                    writtenDynamicFields++;

            // Write number of fields
            buffer.Write((ushort)writtenStaticFields);
            buffer.Write((ushort)writtenDynamicFields);
            // Write delta id
            buffer.Write(currentDeltaId);
            // Write fields
            buffer.WriteBytes(tempStaticBuffer.Data, 0, tempStaticBuffer.Length);
            buffer.WriteBytes(tempDynamicBuffer.Data, 0, tempDynamicBuffer.Length);

            // Add a copy of what we just wrote
            // to the delta states
            if (IsDeltaCompressing)
                AddAsDeltaSnapshot();
        }

        bool SerializeStaticField(StaticSnapshotField field, NetBuffer buffer, DeltaSnapshot prev)
        {
            // Attempt to delta compress
            if (prev != null && !field.NeverCompress)
            {
                if (field.Type == SnapshotFieldType.Primitive)
                {
                    // If the field in the last acknowledged snapshot is the same as
                    // what were sending, skip it
                    object prevField;
                    if (prev.StaticFields.TryGetValue(field.Id, out prevField))
                        if (field.Value.Equals(prevField))
                            return false;
                }
                else if (field.Type == SnapshotFieldType.Trigger)
                {
                    // If the trigger was never activated, dont send.
                    // Receiving end assumes that the trigger is zero when not received.
                    Trigger ct = (Trigger)field.Value;
                    if (ct.Iterations == 0)
                        return false;
                }
            }

            // Write field id
            buffer.Write(field.Id);

            return SerializeField(field, buffer, prev);
        }

        bool SerializeDynamicField(DynamicSnapshotField field, NetBuffer buffer, DeltaSnapshot prev)
        {
            // Attempt to delta compress
            if (prev != null && !field.NeverCompress)
            {
                if (field.Type == SnapshotFieldType.Primitive)
                {
                    // If the field in the last acknowledged snapshot is the same as
                    // what were sending, skip it
                    object prevField;
                    if (prev.DynamicFields.TryGetValue(field.Id, out prevField))
                        if (field.Value.Equals(prevField))
                            return false;
                }
                else if (field.Type == SnapshotFieldType.Trigger)
                {
                    // If the trigger was never activated, dont send.
                    // Receiving end assumes that the trigger is zero when not received.
                    Trigger ct = (Trigger)field.Value;
                    if (ct.Iterations == 0)
                        return false;
                }
            }

            // Write field id
            buffer.Write((byte)field.IdPrimitiveType);
            buffer.WriteDynamic(field.Id);

            return SerializeField(field, buffer, prev);
        }

        bool SerializeField(SnapshotField field, NetBuffer buffer, DeltaSnapshot prev)
        {
            // Write field type
            buffer.Write((byte)field.Type);

            // Write the field data
            if (field.Type == SnapshotFieldType.Primitive)
            {
                // Write primitive
                buffer.Write((byte)field.PrimitiveType);
                buffer.WriteDynamic(field.Value);
            }
            else if (field.Type == SnapshotFieldType.Trigger)
            {
                Trigger t = (Trigger)field.Value;

                // Write trigger
                buffer.Write(t.Iterations);
                // Reset trigger
                t.LastIterations = t.Iterations;
                t.Iterations = 0;
            }
            else if (field.Type == SnapshotFieldType.Snapshot)
            {
                // Write nested snapshot
                Snapshot ns = (Snapshot)field.Value;
                ns.Serialize(buffer);
            }
            else if (field.Type == SnapshotFieldType.Custom)
            {
                // Write custom snapshot
                CustomSnapshot cs = (CustomSnapshot)field.Value;
                cs.Serialize(buffer);
            }

            return true;
        }

        public virtual void Deserialize(NetBuffer buffer)
        {
            // Reset all triggers
            foreach (SnapshotField field in staticFields.Values)
                if (field.Type == SnapshotFieldType.Trigger)
                {
                    Trigger t = (Trigger)field.Value;
                    t.LastIterations = t.Iterations;
                    t.Iterations = 0;
                }

            foreach (SnapshotField field in dynamicFields.Values)
                if (field.Type == SnapshotFieldType.Trigger)
                {
                    Trigger t = (Trigger)field.Value;
                    t.LastIterations = t.Iterations;
                    t.Iterations = 0;
                }

            // Read number of fields
            ushort numStaticFields = buffer.ReadUInt16();
            ushort numDynamicFields = buffer.ReadUInt16();

            // Read delta id
            byte deltaId = buffer.ReadByte();

            if (IsDeltaCompressing)
                AcknowledgedDeltaIds.Add(deltaId);

            // Read static fields
            for (int i = 0; i < numStaticFields; i++)
            {
                // Read field id
                ushort fieldId = buffer.ReadUInt16();

                // Try and find the field (it's ok if we can't)
                StaticSnapshotField field;
                staticFields.TryGetValue(fieldId, out field);

                // Read the field
                ReadField(buffer, field);
            }

            // Read dynamic fields
            for (int i = 0; i < numDynamicFields; i++)
            {
                // Read field id
                SnapshotPrimitiveType idPrimType = (SnapshotPrimitiveType)buffer.ReadByte();
                object fieldId = ReadPrimitive(buffer, idPrimType);

                // Try and find the field (it's ok if we can't)
                DynamicSnapshotField field;
                dynamicFields.TryGetValue(fieldId, out field);

                // Read the field
                ReadField(buffer, field);
            }
        }

        void ReadField(NetBuffer buffer, SnapshotField field)
        {
            // Read field type
            SnapshotFieldType fieldType = (SnapshotFieldType)buffer.ReadByte();

            // Read field data
            if (fieldType == SnapshotFieldType.Primitive)
            {
                // Read primitive
                SnapshotPrimitiveType primType = (SnapshotPrimitiveType)buffer.ReadByte();
                object v = ReadPrimitive(buffer, primType);

                if (field != null)
                    field.Value = v;
            }
            else if (fieldType == SnapshotFieldType.Trigger)
            {
                // Read trigger
                byte it = buffer.ReadByte();

                if (field != null)
                {
                    Trigger t = (Trigger)field.Value;
                    t.Iterations = it;
                }
            }
            else if (fieldType == SnapshotFieldType.Snapshot)
            {
                // Read nested snapshot
                Snapshot ns;
                if (field != null)
                    ns = (Snapshot)field.Value;
                else
                    // Make temp snapshot to just read ns
                    ns = new TempSnapshot(snapshotSystem);

                ns.Deserialize(buffer);
            }
            else if (fieldType == SnapshotFieldType.Custom)
            {
                // Read custom snapshot
                // Read snapshot size
                ushort bufferSize = buffer.ReadUInt16();

                if (field != null)
                {
                    // Read custom snapshot
                    CustomSnapshot cs = (CustomSnapshot)field.Value;
                    cs.Deserialize(buffer);
                }
                else
                    // Skip buffer
                    buffer.Position += bufferSize;
            }
        }

        static object ReadPrimitive(NetBuffer buffer, SnapshotPrimitiveType type)
        {
            switch (type)
            {
                case SnapshotPrimitiveType.Byte:
                    return buffer.ReadByte();
                case SnapshotPrimitiveType.Char:
                    return buffer.ReadChar();
                case SnapshotPrimitiveType.Boolean:
                    return buffer.ReadBool();
                case SnapshotPrimitiveType.Int16:
                case SnapshotPrimitiveType.SByte: // SByte is written as a short
                    return buffer.ReadInt16();
                case SnapshotPrimitiveType.UInt16:
                    return buffer.ReadUInt16();
                case SnapshotPrimitiveType.Int32:
                    return buffer.ReadInt32();
                case SnapshotPrimitiveType.UInt32:
                    return buffer.ReadUInt32();
                case SnapshotPrimitiveType.Int64:
                    return buffer.ReadInt64();
                case SnapshotPrimitiveType.UInt64:
                    return buffer.ReadUInt64();
                case SnapshotPrimitiveType.Single:
                    return buffer.ReadFloat();
                case SnapshotPrimitiveType.Double:
                    return buffer.ReadDouble();
                case SnapshotPrimitiveType.ByteFlag:
                    return buffer.ReadByteFlag();
                default:
                    throw new Exception("Snapshot primitive type '" + type + "' is not supported!");
            }
        }

        static SnapshotPrimitiveType GetPrimitiveType(Type type)
        {
            if (type == typeof(byte)) return SnapshotPrimitiveType.Byte;
            else if (type == typeof(sbyte)) return SnapshotPrimitiveType.SByte;
            else if (type == typeof(char)) return SnapshotPrimitiveType.Char;
            else if (type == typeof(bool)) return SnapshotPrimitiveType.Boolean;
            else if (type == typeof(short)) return SnapshotPrimitiveType.Int16;
            else if (type == typeof(ushort)) return SnapshotPrimitiveType.UInt16;
            else if (type == typeof(int)) return SnapshotPrimitiveType.Int32;
            else if (type == typeof(uint)) return SnapshotPrimitiveType.UInt32;
            else if (type == typeof(long)) return SnapshotPrimitiveType.Int64;
            else if (type == typeof(ulong)) return SnapshotPrimitiveType.UInt64;
            else if (type == typeof(float)) return SnapshotPrimitiveType.Single;
            else if (type == typeof(double)) return SnapshotPrimitiveType.Double;
            else if (type == typeof(ByteFlag)) return SnapshotPrimitiveType.ByteFlag;
            else throw new Exception("Snapshot primitive type '" + type.FullName + "' is not supported!");
        }
    }
}
