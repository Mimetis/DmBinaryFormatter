using DmBinaryFormatter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
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

            DmSerializer serializer = new DmSerializer();

            using (FileStream fs = new FileStream("client.bin", FileMode.OpenOrCreate, FileAccess.Write))
            {
                serializer.Serialize(cli, fs);
            }

            Client deserializedClient = null;
            using (FileStream fs = new FileStream("client.bin", FileMode.Open, FileAccess.Read))
            {
                deserializedClient = serializer.Deserialize<Client>(fs);
            }
        }
    }
}
