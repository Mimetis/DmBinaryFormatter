using DmBinaryFormatter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleSample
{
    [Serializable]
    public class Address
    {
        public String City { get; set; }
        public string PostalCode { get; set; }
    }

    [Serializable]
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
    class Program
    {
        static void Main(string[] args)
        {
            Address clientAddress = new Address
            {
                City = "Toulouse",
                PostalCode = "31620"
            };
            Client cli = new Client
            {
                Address = clientAddress,
                Birthday = new DateTime(1976, 10, 23),
                ClientId = Guid.NewGuid(),
                FirstName = "Sebastien",
                LastName = "Pertus",
                IsAvailable = true,
                Money = 10,
                Id = 1,
            };

            Client deserializedClient = null;

            // ----------------------------------------------------------------------------
            // We are on .NET 4.6.2, so using version .Net Standard 1.3 (to 1.5)
            // ----------------------------------------------------------------------------
            DmSerializer serializer = new DmSerializer();

            using (FileStream fs = new FileStream("client.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                serializer.Serialize(cli, fs);
            }

            using (FileStream fs = new FileStream("client.bin", FileMode.Open, FileAccess.ReadWrite))
            {
                deserializedClient = serializer.Deserialize<Client>(fs);
            }
        }
    }
}
