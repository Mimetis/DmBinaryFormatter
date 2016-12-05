using DmBinaryFormatter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

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

            Debug.WriteLine($"array length : {clientByte.Length}");

            // Of course you can deserialize the byte array, 
            // var deserializedClient = serializer.Deserialize<Client>(clientByte);

            // But obviously, you should prefer the Stream method
            ms.Position = 0;

            // If you want to see the debug graph, uncomment this line
            //serializer.DebugWriter = Console.Out;

            var deserializedClient = serializer.Deserialize<Client>(ms);

            Debug.WriteLine($"Client1 : {cli.FirstName} - Client2 : {deserializedClient.FirstName} ");

        }
    }
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
}
