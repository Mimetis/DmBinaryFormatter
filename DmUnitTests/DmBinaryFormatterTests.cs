using DmBinaryFormatter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace DmUnitTests
{
    public class DmBinaryFormatterTests
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

                    if (obj.GetType().IsDictionary())
                        areEqual = EqualityHelpers.DictionariesAreEqual((IDictionary)obj, (IDictionary)ob2);
                    else if (obj.GetType().IsEnumerable())
                        areEqual = EqualityHelpers.ArraysAreEqual((IEnumerable)obj, (IEnumerable)ob2);
                    else
                        areEqual = obj.Equals(ob2);

                    Debug.WriteLine($"Type: {obj.GetType()}: Instance 1 is not equal to deserialized instance.");

                    Assert.True(areEqual, $"Type: {obj.GetType()}: Instance 1 is not equal to deserialized instance.");
                }
                catch (Exception ex)
                {
                    Assert.True(false, ($"Object 1 Type : {obj.GetType()} - is not serializable"));
                }
            }
        }
    }

}