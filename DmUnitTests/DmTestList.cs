using DmBinaryFormatter;
using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DmUnitTests
{



    public class SerializableObjects
    {
        public static IEnumerable<object> GetObjects()
        {
            // Primitive types
            yield return byte.MinValue;
            yield return byte.MaxValue;
            yield return sbyte.MinValue;
            yield return sbyte.MaxValue;
            yield return short.MinValue;
            yield return short.MaxValue;
            yield return int.MinValue;
            yield return int.MaxValue;
            yield return uint.MinValue;
            yield return uint.MaxValue;
            yield return long.MinValue;
            yield return long.MaxValue;
            yield return ulong.MinValue;
            yield return ulong.MaxValue;
            yield return char.MinValue;
            yield return char.MaxValue;
            yield return float.MinValue;
            yield return float.MaxValue;
            yield return double.MinValue;
            yield return double.MaxValue;
            yield return decimal.MinValue;
            yield return decimal.MaxValue;
            yield return decimal.MinusOne;
            yield return true;
            yield return false;
            yield return "";
            yield return "c";
            yield return "\u4F60\u597D";
            yield return "some\0data\0with\0null\0chars";
            yield return "<>&\"\'";
            yield return " < ";
            yield return "minchar" + char.MinValue + "minchar";

            // Enum values
            yield return DayOfWeek.Monday;
            yield return DateTimeKind.Local;

            // Nullables
            yield return (int?)1;
            yield return (StructWithIntField?)new StructWithIntField() { X = 42 };

            // Other core serializable types
            // Doesn't WORK
            // ---------------------------
            // yield return IntPtr.Zero;
            // yield return UIntPtr.Zero;
            // yield return new AttributeUsageAttribute(AttributeTargets.Class);
            // ---------------------------

            yield return DateTime.Now;
            yield return DateTimeOffset.Now;
            yield return DateTimeKind.Local;
            yield return TimeSpan.FromDays(7);
            yield return new Version(1, 2, 3, 4);
            yield return new Guid("0CACAA4D-C6BD-420A-B660-2F557337CA89");
            yield return new List<int>();
            yield return new List<int>() { 1, 2, 3, 4, 5 };
            yield return new Dictionary<int, string>() { { 1, "test" }, { 2, "another test" } };
            yield return Tuple.Create(1);
            yield return Tuple.Create(1, "2");
            yield return Tuple.Create(1, "2", 3u);
            yield return Tuple.Create(1, "2", 3u, 4L);
            yield return Tuple.Create(1, "2", 3u, 4L, 5.6);
            yield return Tuple.Create(1, "2", 3u, 4L, 5.6, 7.8f);
            yield return Tuple.Create(1, "2", 3u, 4L, 5.6, 7.8f, 9m);
            yield return new KeyValuePair<int, byte>(42, 84);

            Dictionary<int, List<object>> dictionaryContainingAnObjectList = new Dictionary<int, List<object>>();
            var objectListInADictionary = new List<Object>();
            objectListInADictionary.Add(12);
            objectListInADictionary.Add("Hello world");

            dictionaryContainingAnObjectList.Add(1, objectListInADictionary);
            dictionaryContainingAnObjectList.Add(2, new List<Object>());

            yield return dictionaryContainingAnObjectList;

            // Arrays of primitive types
            yield return Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            yield return new int[] { };
            yield return new int[] { 1 };
            yield return new int[] { 1, 2, 3, 4, 5 };
            yield return new char[] { 'a', 'b', 'c', 'd', 'e' };
            yield return new string[] { };
            yield return new string[] { "hello", "world" };
            yield return new short[] { short.MaxValue };
            yield return new long[] { long.MaxValue };
            yield return new ushort[] { ushort.MaxValue };
            yield return new uint[] { uint.MaxValue };
            yield return new ulong[] { ulong.MaxValue };
            yield return new bool[] { true, false };
            yield return new double[] { 1.2 };
            yield return new float[] { 1.2f, 3.4f };

            // ISerializable
            yield return new BasicISerializableObject(1, "2");

            // Arrays of other types
            yield return new object[] { };
            yield return new ArrayList { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            yield return new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            yield return new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            yield return new Point[] { new Point(1, 2), new Point(3, 4) };
            yield return new ObjectWithArrays
            {
                IntArray = new int[0],
                StringArray = new string[] { "hello", "world" },
                ByteArray = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 },
                JaggedArray = new int[][] { new int[] { 1, 2, 3 }, new int[] { 4, 5, 6, 7 } }
            };

            yield return Enumerable.Range(0, 10000).Select(i => (object)i).ToArray();
            yield return new object[200]; // fewer than 256 nulls
            yield return new object[300]; // more than 256 nulls

            // Non-vector arrays
            yield return Array.CreateInstance(typeof(uint), new[] { 5 }, new[] { 1 });
            yield return Array.CreateInstance(typeof(int), new[] { 0, 0, 0 }, new[] { 0, 0, 0 });
            var arr = Array.CreateInstance(typeof(string), new[] { 1, 2 }, new[] { 3, 4 });
            arr.SetValue("hello", new[] { 3, 5 });
            yield return arr;

            // Various globalization types
            yield return CultureInfo.CurrentCulture;
            yield return CultureInfo.InvariantCulture;

            // Custom object
            var sealedObjectWithIntStringFields = new SealedObjectWithIntStringFields();
            yield return sealedObjectWithIntStringFields;
            yield return new SealedObjectWithIntStringFields() { Member1 = 42, Member2 = null, Member3 = "84" };

            // Custom object with fields pointing to the same object
            yield return new ObjectWithIntStringUShortUIntULongAndCustomObjectFields
            {
                Member1 = 10,
                Member2 = "hello",
                _member3 = "hello",
                Member4 = sealedObjectWithIntStringFields,
                Member4shared = sealedObjectWithIntStringFields,
                Member5 = new SealedObjectWithIntStringFields(),
                Member6 = "Hello World",
                str1 = "hello < world",
                str2 = "<",
                str3 = "< world",
                str4 = "hello < world",
                u16 = ushort.MaxValue,
                u32 = uint.MaxValue,
                u64 = ulong.MaxValue,
            };

            // Simple type without a default ctor
            var point = new Point(1, 2);
            yield return point;

            // Graph without cycles
            yield return new Tree<int>(42, null, null);
            yield return new Tree<int>(1, new Tree<int>(2, new Tree<int>(3, null, null), null), null);
            yield return new Tree<Colors>(Colors.Red, null, new Tree<Colors>(Colors.Blue, null, new Tree<Colors>(Colors.Green, null, null)));
            yield return new Tree<int>(1, new Tree<int>(2, new Tree<int>(3, null, null), new Tree<int>(4, null, null)), new Tree<int>(5, null, null));

            // Graph with cycles
            Graph<int> a = new Graph<int> { Value = 1 };
            yield return a;
            Graph<int> b = new Graph<int> { Value = 2, Links = new[] { a } };
            yield return b;
            Graph<int> c = new Graph<int> { Value = 3, Links = new[] { a, b } };
            yield return c;
            Graph<int> d = new Graph<int> { Value = 3, Links = new[] { a, b, c } };
            yield return d;

            // Structs
            yield return new EmptyStruct();
            yield return new StructWithIntField { X = 42 };
            yield return new StructWithStringFields { String1 = "hello", String2 = "world" };
            yield return new StructContainingOtherStructs { Nested1 = new StructWithStringFields { String1 = "a", String2 = "b" }, Nested2 = new StructWithStringFields { String1 = "3", String2 = "4" } };
            yield return new StructContainingArraysOfOtherStructs();
            yield return new StructContainingArraysOfOtherStructs { Nested = new StructContainingOtherStructs[0] };
            var s = new StructContainingArraysOfOtherStructs
            {
                Nested = new[]
                {
                    new StructContainingOtherStructs { Nested1 = new StructWithStringFields { String1 = "a", String2 = "b" }, Nested2 = new StructWithStringFields { String1 = "3", String2 = "4" } },
                    new StructContainingOtherStructs { Nested1 = new StructWithStringFields { String1 = "e", String2 = "f" }, Nested2 = new StructWithStringFields { String1 = "7", String2 = "8" } },
                }
            };
            yield return s;
            yield return new object[] { s, new StructContainingArraysOfOtherStructs?(s) };


        }
    }

    public sealed class SealedObjectWithIntStringFields
    {
        public int Member1;
        public string Member2;
        public string Member3;

        public override bool Equals(object obj)
        {
            var o = obj as SealedObjectWithIntStringFields;
            if (o == null) return false;
            return
                EqualityComparer<int>.Default.Equals(Member1, o.Member1) &&
                EqualityComparer<string>.Default.Equals(Member2, o.Member2) &&
                EqualityComparer<string>.Default.Equals(Member3, o.Member3);
        }

        public override int GetHashCode() => 1;
    }


    [Serializable]
    public class BasicISerializableObject : ISerializable
    {
        private NonSerializablePair<int, string> _data;

        public BasicISerializableObject(int value1, string value2)
        {
            _data = new NonSerializablePair<int, string> { Value1 = value1, Value2 = value2 };
        }

        public BasicISerializableObject(SerializationInfo info, StreamingContext context)
        {
            _data = new NonSerializablePair<int, string> { Value1 = info.GetInt32("Value1"), Value2 = info.GetString("Value2") };
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Value1", _data.Value1);
            info.AddValue("Value2", _data.Value2);
        }

        public override bool Equals(object obj)
        {
            var o = obj as BasicISerializableObject;
            if (o == null) return false;
            if (_data == null || o._data == null) return _data == o._data;
            return _data.Value1 == o._data.Value1 && _data.Value2 == o._data.Value2;
        }

        public override int GetHashCode() => 1;
    }

    public class ObjectWithIntStringUShortUIntULongAndCustomObjectFields
    {
        public int Member1;
        public string Member2;
        public string _member3;
        public SealedObjectWithIntStringFields Member4;
        public SealedObjectWithIntStringFields Member4shared;
        public SealedObjectWithIntStringFields Member5;
        public string Member6;
        public string str1;
        public string str2;
        public string str3;
        public string str4;
        public ushort u16;
        public uint u32;
        public ulong u64;

        public override bool Equals(object obj)
        {
            var o = obj as ObjectWithIntStringUShortUIntULongAndCustomObjectFields;
            if (o == null) return false;

            return
                EqualityComparer<int>.Default.Equals(Member1, o.Member1) &&
                EqualityComparer<string>.Default.Equals(Member2, o.Member2) &&
                EqualityComparer<string>.Default.Equals(_member3, o._member3) &&
                EqualityComparer<SealedObjectWithIntStringFields>.Default.Equals(Member4, o.Member4) &&
                EqualityComparer<SealedObjectWithIntStringFields>.Default.Equals(Member4shared, o.Member4shared) &&
                EqualityComparer<SealedObjectWithIntStringFields>.Default.Equals(Member5, o.Member5) &&
                EqualityComparer<string>.Default.Equals(Member6, o.Member6) &&
                EqualityComparer<string>.Default.Equals(str1, o.str1) &&
                EqualityComparer<string>.Default.Equals(str2, o.str2) &&
                EqualityComparer<string>.Default.Equals(str3, o.str3) &&
                EqualityComparer<string>.Default.Equals(str4, o.str4) &&
                EqualityComparer<ushort>.Default.Equals(u16, o.u16) &&
                EqualityComparer<uint>.Default.Equals(u16, o.u16) &&
                EqualityComparer<ulong>.Default.Equals(u64, o.u64) &&
                // make sure shared members are the same object
                ReferenceEquals(Member4, Member4shared) &&
                ReferenceEquals(o.Member4, o.Member4shared);
        }

        public override int GetHashCode() => 1;
    }

    public class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            var o = obj as Point;
            if (o == null) return false;
            return X == o.X && Y == o.Y;
        }
        public override int GetHashCode() => 1;
    }

    public class Tree<T>
    {
        public Tree(T value, Tree<T> left, Tree<T> right)
        {
            Value = value;
            Left = left;
            Right = right;
        }

        public T Value { get; }
        public Tree<T> Left { get; }
        public Tree<T> Right { get; }

        public override bool Equals(object obj)
        {
            Tree<T> o = obj as Tree<T>;
            if (o == null) return false;

            return
                EqualityComparer<T>.Default.Equals(Value, o.Value) &&
                EqualityComparer<Tree<T>>.Default.Equals(Left, o.Left) &&
                EqualityComparer<Tree<T>>.Default.Equals(Right, o.Right) &&
                // make sure the branches aren't actually the exact same object
                (Left == null || !ReferenceEquals(Left, o.Left)) &&
                (Right == null || !ReferenceEquals(Right, o.Right));
        }

        public override int GetHashCode() => 1;
    }

    public class Graph<T>
    {
        public T Value;
        public Graph<T>[] Links;

        public override bool Equals(object obj)
        {
            Graph<T> o = obj as Graph<T>;
            if (o == null) return false;

            var toExplore = new Stack<KeyValuePair<Graph<T>, Graph<T>>>();
            toExplore.Push(new KeyValuePair<Graph<T>, Graph<T>>(this, o));
            var seen1 = new HashSet<Graph<T>>();
            while (toExplore.Count > 0)
            {
                var cur = toExplore.Pop();
                if (!seen1.Add(cur.Key))
                {
                    continue;
                }

                if (!EqualityComparer<T>.Default.Equals(cur.Key.Value, cur.Value.Value))
                {
                    return false;
                }

                if (Links == null || o.Links == null)
                {
                    if (Links != o.Links) return false;
                    continue;
                }

                if (Links.Length != o.Links.Length)
                {
                    return false;
                }

                for (int i = 0; i < Links.Length; i++)
                {
                    toExplore.Push(new KeyValuePair<Graph<T>, Graph<T>>(Links[i], o.Links[i]));
                }
            }

            return true;
        }

        public override int GetHashCode() => 1;
    }

    internal sealed class ObjectWithArrays
    {
        public int[] IntArray;
        public string[] StringArray;
        public Tree<int>[] TreeArray;
        public byte[] ByteArray;
        public int[][] JaggedArray;
        public int[,] MultiDimensionalArray;

        public override bool Equals(object obj)
        {
            ObjectWithArrays o = obj as ObjectWithArrays;
            if (o == null) return false;

            return
                EqualityHelpers.ArraysAreEqual(IntArray, o.IntArray) &&
                EqualityHelpers.ArraysAreEqual(StringArray, o.StringArray) &&
                EqualityHelpers.ArraysAreEqual(TreeArray, o.TreeArray) &&
                EqualityHelpers.ArraysAreEqual(ByteArray, o.ByteArray) &&
                EqualityHelpers.ArraysAreEqual(JaggedArray, o.JaggedArray) &&
                EqualityHelpers.ArraysAreEqual(MultiDimensionalArray, o.MultiDimensionalArray);
        }

        public override int GetHashCode() => 1;
    }

    public enum Colors
    {
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple
    }

    public struct NonSerializableStruct
    {
        public int Value;
    }

    public class NonSerializableClass
    {
        public int Value;
    }

    public struct EmptyStruct { }

    public struct StructWithIntField
    {
        public int X;
    }

    public struct StructWithStringFields
    {
        public string String1;
        public string String2;
    }

    public struct StructContainingOtherStructs
    {
        public StructWithStringFields Nested1;
        public StructWithStringFields Nested2;
    }

    public struct StructContainingArraysOfOtherStructs
    {
        public StructContainingOtherStructs[] Nested;

        public override bool Equals(object obj)
        {
            if (!(obj is StructContainingArraysOfOtherStructs)) return false;
            return EqualityHelpers.ArraysAreEqual(Nested, ((StructContainingArraysOfOtherStructs)obj).Nested);
        }

        public override int GetHashCode() => 1;
    }




    internal sealed class NonSerializablePair<T1, T2>
    {
        public T1 Value1;
        public T2 Value2;
    }




    public class Version1ClassWithoutField
    {
    }


    public class Version2ClassWithoutOptionalField
    {
        public object Value;
    }



    public class ObjectWithStateAndMethod
    {
        public int State;
        public int GetState() => State;
    }



    internal static class EqualityHelpers
    {
        public static bool ArraysAreEqual<T>(T[] array1, T[] array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(array1[i], array2[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsEnumerable(Type type)
        {
            if (type.IsArray)
                return true;

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return true;

            return false;
        }

        public static bool IsDictionary(Type type)
        {
            if (typeof(IDictionary).IsAssignableFrom(type))
                return true;

            TypeInfo typeInfo = type.GetTypeInfo();

            if (typeInfo.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                return true;

            return false;
        }
        public static bool DictionariesAreEqual(IDictionary array1, IDictionary array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;

            var e1 = array1.GetEnumerator();
            var e2 = array2.GetEnumerator();

            while (e1.MoveNext())
            {
                e2.MoveNext();

                var entryE1 = e1.Entry;
                var entryE2 = e2.Entry;

                if (!EqualityComparer<object>.Default.Equals(entryE1.Key, entryE1.Key))
                    return false;


               if (IsEnumerable(entryE1.Value.GetType()))
                {
                    if (!ArraysAreEqual((IEnumerable)entryE1.Value, (IEnumerable)entryE2.Value))
                    {
                        return false;
                    }
                }
                else if (!EqualityComparer<object>.Default.Equals(entryE1.Value, entryE2.Value))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ArraysAreEqual(IEnumerable array1, IEnumerable array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;

            var e1 = array1.GetEnumerator();
            var e2 = array2.GetEnumerator();
            while (e1.MoveNext())
            {
                e2.MoveNext();

                if ((e1.GetType()) is IEnumerable)
                {
                    if (!ArraysAreEqual((IEnumerable)e1, (IEnumerable)e2))
                    {
                        return false;
                    }
                }
                else if (!EqualityComparer<object>.Default.Equals(e1.Current, e2.Current))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool ArraysAreEqual(Array array1, Array array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;
            if (array1.Rank != array2.Rank) return false;

            for (int i = 0; i < array1.Rank; i++)
            {
                if (array1.GetLength(i) != array2.GetLength(i)) return false;
            }

            var e1 = array1.GetEnumerator();
            var e2 = array2.GetEnumerator();
            while (e1.MoveNext())
            {
                e2.MoveNext();
                if (!EqualityComparer<object>.Default.Equals(e1.Current, e2.Current))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ArraysAreEqual<T>(T[][] array1, T[][] array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                T[] sub1 = array1[i], sub2 = array2[i];
                if (sub1 == null || sub2 == null && (sub1 != sub2)) return false;
                if (sub1.Length != sub2.Length) return false;
                for (int j = 0; j < sub1.Length; j++)
                {
                    if (!EqualityComparer<T>.Default.Equals(sub1[j], sub2[j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }


}


