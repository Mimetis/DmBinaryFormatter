using DmBinaryFormatter.Serializers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace DmBinaryFormatter
{
    public class DmSerializer
    {
        public Encoding Encoding { get; set; }
        public DmBinaryReader Reader { get; private set; }
        public DmBinaryWriter Writer { get; private set; }
        public TextWriter DebugWriter { get; set; }

        private enum DmState : byte
        {
            IsNull = 0,
            IsObject = 1,
            IsReference = 2
        }

        internal int index = 0;
        private Dictionary<Object, int> fromObjects = new Dictionary<Object, int>();
        private Dictionary<int, Object> fromIndexes = new Dictionary<int, object>();

        private (DmState, int) GetState(Object obj, Type objType)
        {
            if (obj == null)
            {
                this.index++;
                return (DmState.IsNull, this.index);
            }

            if (objType.IsPrimitiveManagedType())
            {
                this.index++;
                return (DmState.IsObject, this.index);
            }

            var ptrObj = fromObjects.ContainsKey(obj);

            if (ptrObj)
                return (DmState.IsReference, fromObjects[obj]);

            this.index++;
            fromObjects.Add(obj, this.index);
            return (DmState.IsObject, this.index);

        }
        public DmSerializer()
        {
            this.Encoding = Encoding.UTF8;
        }

        public DmSerializer(Encoding encoding)
        {
            this.Encoding = encoding;
        }


        public void Serialize<T>(T obj, Stream s)
        {
            if (obj == null)
                throw new ArgumentException("Object is null");

            if (!s.CanRead || !s.CanWrite)
                throw new ArgumentException("Can't Read / write in the stram");

            this.Writer = new DmBinaryWriter(s, this.Encoding);

            // Clear references
            fromObjects.Clear();
            fromIndexes.Clear();

            index = 0;

            var objectType = obj.GetType();

            this.Serialize(obj, objectType);
        }

        public byte[] Serialize<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentException("Object is null");

            using (MemoryStream ms = new MemoryStream())
            {
                this.Serialize(obj, ms);

                return ms.ToArray();
            }
        }


        internal void Serialize(object obj, Type objType)
        {

            // If it's Object, we get the underlying type
            if (objType == typeof(Object) && obj != null)
            {
                var baseType = obj.GetType().GetBaseType();
                var s = TypeSerializer.GetSerializer(baseType);
                Serialize(obj, baseType);
                return;
            }

            var serializer = TypeSerializer.GetSerializer(objType);

            // Write object type
            this.Writer.Write(objType.AssemblyQualifiedName);

            // Check if it's not a reference
            (DmState state, int refIndex) = this.GetState(obj, objType);

            // Write state
            this.Writer.Write((byte)state);

            // Write index;
            this.Writer.Write(refIndex);

            // if not null or ref
            if (state == DmState.IsObject)
                serializer.Serialize(this, obj, objType);
        }


        public Object Deserialize(Byte[] bytes)
        {
            using (MemoryStream s = new MemoryStream(bytes))
            {
                return Deserialize(s);
            }
        }

        public object Deserialize(Stream s)
        {
            if (!s.CanRead || !s.CanWrite)
                throw new ArgumentException("Can't Read / write in the stram");

            Object obj = null;
            using (Reader = new DmBinaryReader(s, this.Encoding))
            {
                obj = this.GetObject(this.DebugWriter != null);
            }

            fromIndexes.Clear();
            fromObjects.Clear();

            return obj;
        }

        public T Deserialize<T>(Stream s)
        {
            return (T)Deserialize(s);
        }

        public T Deserialize<T>(Byte[] bytes)
        {
            return (T)Deserialize(bytes);
        }

        internal Object GetObject(bool isDebugMode = false)
        {
            Object deserializedObject = null;


            int indent = 0;

            if (this.Reader.BaseStream.Position >= this.Reader.BaseStream.Length)
                throw new IndexOutOfRangeException("stream is ended !");


            if (isDebugMode)
            {
                DebugWriter.WriteLineIndent(indent);
                DebugWriter.Write("{");
            }

            // Get Type
            var objTypeFromStream = Reader.ReadString();
            var objType = Type.GetType(objTypeFromStream);

            if (isDebugMode)
            {
                DebugWriter.Write($"[{objType.Name}]");
            }


            // Get State and Index
            var state = (DmState)Reader.ReadByte();
            var index = Reader.ReadInt32();

            if (isDebugMode)
                DebugWriter.Write($"[{state}][{index}]");

            TypeSerializer serializer = null;
            serializer = TypeSerializer.GetSerializer(objType);

            if (state == DmState.IsReference)
            {
                deserializedObject = fromIndexes[index];
            }
            else if (state != DmState.IsNull)
            {
                deserializedObject = serializer.Deserialize(this, objType, isDebugMode);

                // Dont make a reference on value type
                if (!objType.IsPrimitiveManagedType())
                    fromIndexes.Add(index, deserializedObject);
            }

            if (isDebugMode)
            {
                // cosmetic
                if (serializer.GetType() != typeof(PrimitiveSerializer))
                {
                    indent--;
                    DebugWriter.WriteLineIndent(indent);
                }
                DebugWriter.Write("}");
            }

            return deserializedObject;

        }



    }
}
