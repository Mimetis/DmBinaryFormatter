using DmBinaryFormatter;
using System;
using System.Collections;
using Xunit;

namespace DmUnitTests
{
    public partial class DmBinaryFormatterTests
    {
        [Fact]
        public void TestCases()
        {
            DmSerializer serializer = new DmSerializer();

            foreach (var obj in SerializableObjects.GetObjects())
            {
                try
                {
                    var cliByte = serializer.Serialize(obj);
                    var ob2 = serializer.Deserialize(cliByte);

                    bool areEqual = false;

                
                    if (EqualityHelpers.IsDictionary(obj.GetType()))
                        areEqual = EqualityHelpers.DictionariesAreEqual((IDictionary)obj, (IDictionary)ob2);
                    else if (EqualityHelpers.IsEnumerable(obj.GetType()))
                        areEqual = EqualityHelpers.ArraysAreEqual((IEnumerable)obj, (IEnumerable)ob2);
                    else
                        areEqual = obj.Equals(ob2);

                    Assert.True(areEqual, $"Type: {obj.GetType()}: Instance 1 is not equal to deserialized instance.");
                }
                catch (Exception ex)
                {
                    Assert.True(false, ($"Object 1 Type : {obj.GetType()} - is not serializable. ({ex.Message})"));
                }
            }
        }

        public class ClassWithAConverter
        {

            public int ID { get; set; }
            public string LastName { get; set; }
            public IntPtr Ptr { get; set; }

            public override bool Equals(object obj)
            {
                var objC = (ClassWithAConverter)obj;

                return (this.ID == objC.ID && this.LastName == objC.LastName && this.Ptr.ToInt32() == objC.Ptr.ToInt32()); 
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

        }
        public class ClassConverterForClassWithAConverter : DmBinaryFormatter.Converters.ObjectConverter
        {
            public override object ConvertFromString(string obj)
            {
                ClassWithAConverter cc = new ClassWithAConverter();

                var array = obj.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                cc.ID = int.Parse(array[0]);
                cc.LastName = array[1].ToString();
                cc.Ptr = new IntPtr(int.Parse(array[2]));

                return cc;
            }

            public override string ConvertToString(object s)
            {
                ClassWithAConverter cc = s as ClassWithAConverter;
                if (cc == null)
                    throw new ArgumentException("Object is not of type ClassWithAConverter");


                return $"{cc.ID},{cc.LastName},{cc.Ptr.ToInt32()}";
            }
        }

        [Fact]
        public void AddingAConverter()
        {
            ClassWithAConverter cc = new ClassWithAConverter();
            cc.ID = 12;
            cc.LastName = "Pertus";
            cc.Ptr = new IntPtr(12222);


            DmSerializer serializer = new DmSerializer();

            serializer.RegisterConverter(typeof(ClassWithAConverter), new ClassConverterForClassWithAConverter());

            var b = serializer.Serialize(cc);

            var cc2 = serializer.Deserialize<ClassWithAConverter>(b);

            Assert.Equal(cc, cc2);
        }

    }

}