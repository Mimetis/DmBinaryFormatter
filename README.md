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
    
