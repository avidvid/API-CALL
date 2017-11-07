using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using Newtonsoft.Json;
using System.Linq;
using System;

namespace APICall
{
    //#######################################################################  
    //This is Program serves Senior tdevelopment position test for vAuto
    //http://vautointerview.azurewebsites.net/
    //Avid Narimani
    //11/07/2017
    //#######################################################################  
    class Program
    {
        static HttpClient client = new HttpClient();

        static void Main()
        {
            RunAsync().Wait();
        }

        //GetDataset From vAuto AIP
        static async Task<Datasets> GetDataset()
        {
            Datasets Dataset = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress + "datasetId");
                if (response.IsSuccessStatusCode)
                    Dataset = await response.Content.ReadAsAsync<Datasets>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Dataset;

        }

        //Get All Vehiles From vAuto AIP
        static async Task<Vehicles> GetVehiles(Datasets Dataset)
        {
            Vehicles Vehicles = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress + Dataset.datasetId + "/vehicles");
                if (response.IsSuccessStatusCode)
                    Vehicles = await response.Content.ReadAsAsync<Vehicles>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Vehicles;
        }

        //GetVehile From vAuto AIP by id
        static async Task<Vehicle> GetVehile(Datasets Dataset, int VehicleID)
        {
            Vehicle Vehicle = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress + Dataset.datasetId + "/Vehicles/" + VehicleID.ToString());
                if (response.IsSuccessStatusCode)
                    Vehicle = await response.Content.ReadAsAsync<Vehicle>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Vehicle;
        }

        //GetDealer From vAuto AIP by id
        static async Task<Dealer> GetDealer(Datasets Dataset, int DealerID)
        {
            Dealer Dealer = null;
            try
            {
                HttpResponseMessage response = await client.GetAsync(client.BaseAddress + Dataset.datasetId + "/dealers/" + DealerID.ToString());
                if (response.IsSuccessStatusCode)
                    Dealer = await response.Content.ReadAsAsync<Dealer>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Dealer;
        }

        //PostAnswer to vAuto AIP
        static async Task<Answer> PostAnswer(Datasets Dataset, FinalAnswer FA)
        {
            Answer Answer = null;
            try
            {
                HttpResponseMessage response = await client.PostAsJsonAsync(client.BaseAddress + Dataset.datasetId + "/answer", FA);
                if (response.IsSuccessStatusCode)
                    Answer = await response.Content.ReadAsAsync<Answer>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Answer;
        }

        static async Task RunAsync()
        {
            string vAutoAPI = ConfigurationSettings.AppSettings["vAutoAPI"];
            client.BaseAddress = new Uri(vAutoAPI);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                Datasets Dataset = await GetDataset();
                Console.WriteLine("Dataset = "+ Dataset.datasetId);
                if (Dataset == null)
                    throw new ArgumentNullException("Dataset");

                Vehicles Vehicles = await GetVehiles(Dataset);
                if (Vehicles == null)
                    throw new ArgumentNullException("Vehicles");

                Console.WriteLine(Vehicles.vehicleIDs.Count + " Vehiles have  been identified!");

                List<int> DealerIDs = new List<int>();
                List<Vehicle> vAutoVehicles = new List<Vehicle>();
                List<Dealer> vAutoDealers = new List<Dealer>();

                //30 Sec Version
                //#################################################
                //foreach (var Vehicleid in Vehicles.vehicleIDs)
                //{
                //    //Console.WriteLine(Vehicleid.ToString());
                //    Vehicle Vehicle = await GetVehile(Dataset, Vehicleid);
                //    vAutoVehicles.Add(Vehicle);
                //    if (DealerIDs.IndexOf(Vehicle.dealerId) < 0)
                //    {
                //        DealerIDs.Add(Vehicle.dealerId);
                //        //Console.WriteLine(Vehicle.dealerId.ToString());
                //        Dealer Dealer = await GetDealer(Dataset, Vehicle.dealerId);
                //        vAutoDealers.Add(Dealer);
                //        Console.WriteLine("Dealer done");
                //    }
                //    Console.WriteLine("Vehicle done");
                //}
                //#################################################
                var VehicleTasks = new List<Task<Vehicle>>();
                var DealerTasks = new List<Task<Dealer>>();
                foreach (var Vehicleid in Vehicles.vehicleIDs)
                {
                    VehicleTasks.Add(GetVehile(Dataset, Vehicleid));
                }
                // asynchronously wait until all Vehicle requests are complete
                try
                {
                    await Task.WhenAll(VehicleTasks.ToArray());
                    Console.WriteLine("Vehicles done..");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                foreach (var Task in VehicleTasks)
                {
                    Vehicle Vehicle = await Task;
                    vAutoVehicles.Add(Vehicle);
                    if (DealerIDs.IndexOf(Vehicle.dealerId) < 0)
                    {
                        DealerIDs.Add(Vehicle.dealerId);
                        DealerTasks.Add(GetDealer(Dataset, Vehicle.dealerId));
                    }
                }
                // asynchronously wait until all Dealer requests are complete
                try
                {
                    await Task.WhenAll(DealerTasks.ToArray());
                    Console.WriteLine("Dealers done..");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                foreach (var Task in DealerTasks)
                {
                    Dealer Dealer = await Task;
                    vAutoDealers.Add(Dealer);
                }

                //Wraping up all the requested data in the final model
                FinalAnswer FA = new FinalAnswer();
                FA.dealers = vAutoDealers
                        .Select(m => new MyDealer
                        {
                            name = m.name,
                            dealerId = m.dealerId,
                            vehicles = (from v in vAutoVehicles
                                        where v.dealerId == m.dealerId
                                        select new MyVehicle()
                                        {
                                            vehicleId = v.vehicleId,
                                            year = v.year,
                                            make = v.make,
                                            model = v.model
                                        }
                                       ).ToList()
                        }).ToList();

                Answer Answer = await PostAnswer(Dataset, FA);

                Console.WriteLine(JsonConvert.SerializeObject(Answer, Formatting.Indented));
                if (!Answer.success)
                    Console.WriteLine("OHHHH NOOO !!! I promise it worked the last time I tried it, please contact Avid !!!!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            // Keep the console window open in debug mode.
            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.ReadLine();
        }
    }
}
