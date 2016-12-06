# DmBinaryFormatter
A pretty little binary serializer working with .net standard 1.6

Hopefully, we will have the real BinaryFormatter (from .netcore) in .net standard 2.0

Here is a straightforward sample :

    public class Address
    {
        public String City { get; set; }
        public string PostalCode { get; set; }
    }

    public class Client
    {
        private string privateHack = "private hacking property";
        public string publicHack = "Public hacking property";

        // using an enum
        private DayOfWeek day = DayOfWeek.Monday;

        // using a dictionary
        public Dictionary<int, DayOfWeek> dayDictionary { get; set; } = new Dictionary<int, DayOfWeek>();

        // Nullable type
        public int? Id { get; set; }

        public Guid? ClientId { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public DateTime? Birthday { get; set; }
        public bool? IsAvailable { get; set; }
        public double? Money { get; set; }

        // Other kind of object
        public Address Address { get; set; }
    }

And the serialization :

        // ------------------------------------
        // Serialize
        // ------------------------------------

        MemoryStream ms = new MemoryStream();
        serializer.Serialize(cli, ms);

        // ------------------------------------
        // Deserialize
        // ------------------------------------
        // get the bytes array for debug purpose
        var clientByte = ms.ToArray();

        Console.WriteLine($"array length : {clientByte.Length}");

        // Of course you can deserialize the byte array, 
        // var deserializedClient = serializer.Deserialize<Client>(clientByte);

        // But obviously, you should prefer the Stream method
        ms.Position = 0;

        // If you want to see the debug graph, uncomment this line
        serializer.DebugWriter = Console.Out;
      
        var deserializedClient = serializer.Deserialize<Client>(ms);

        Console.WriteLine($"Client1 : {cli.FirstName} - Client2 : {deserializedClient.FirstName} ");

        Console.ReadLine();

## Register a custom converter

You can register a custom converter if your object is not compatible with the serializer, by default.

Here is a straightfoward sample:

        /// <summary>
        /// This class contains a IntPtr. DmSerializer can't serialize an IntPtr
        /// </summary>
        public class ClassWithAConverter
        {
            public int ID { get; set; }
            public string LastName { get; set; }
            public IntPtr Ptr { get; set; }

            /// <summary>
            /// Just for the test equality
            /// </summary>
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

Here is the Converter, inhereting from ObjectConverter :

        /// <summary>
        /// This class inherits from the ObjectConverter abstract class.
        /// You have to implement ConvertFromString and ConvertToString methods
        /// </summary>
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

And here is the test case :

            ClassWithAConverter cc = new ClassWithAConverter();
            cc.ID = 12;
            cc.LastName = "Pertus";
            cc.Ptr = new IntPtr(12222);

            DmSerializer serializer = new DmSerializer();
            serializer.RegisterConverter(typeof(ClassWithAConverter), new ClassConverterForClassWithAConverter());
            var b = serializer.Serialize(cc);
            var cc2 = serializer.Deserialize<ClassWithAConverter>(b);

            Assert.Equal(cc, cc2);


## Knwon Issues

This version doesn't work when you have a cyclable class.  
Something like that :

    public class ClientCyclable
    {
        public int? Id { get; set; }
        public String FirstName { get; set; }

        public ClientCyclable Cyclable { get; set; }
    }

    ClientCyclable cyclable = new ClientCyclable();
    cyclable.Id = 1;
    cyclable.FirstName = "Sebastien";
    cyclable.Cyclable = cyclable;
    
