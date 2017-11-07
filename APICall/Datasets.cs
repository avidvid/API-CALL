using System.Collections.Generic;

//Class definitions based on the vAuto's API

namespace APICall
{
    class Datasets
    {
        public string datasetId { get; set; }
    }

    public class Vehicles
    {
        public List<int> vehicleIDs { get; set; }
    }

    public class Vehicle
    {
        public int vehicleId { get; set; }
        public int year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public int dealerId { get; set; }
    }

    public class Dealer
    {
        public int dealerId { get; set; }
        public string name { get; set; }
    }

    public class MyVehicle
    {
        public int vehicleId { get; set; }
        public int year { get; set; }
        public string make { get; set; }
        public string model { get; set; }
    }

    public class MyDealer
    {
        public int dealerId { get; set; }
        public string name { get; set; }
        public List<MyVehicle> vehicles { get; set; }
    }

    public class FinalAnswer
    {
        public List<MyDealer> dealers { get; set; } 
    }

    public class Answer
    {
        public bool success { get; set; }
        public string message { get; set; }
        public int totalMilliseconds { get; set; }
    }
}
