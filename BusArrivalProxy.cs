using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Project2_203149T
{
    public class BusArrivalProxy
    {
        public async static Task<BusRoot> GetBusArrival(string busStopCode)
        {
            string busApiKey = "UpJOM5jsTBKK2oi2SnzsBg==";

            string busBaseUri = "http://datamall2.mytransport.sg/ltaodataservice/BusArrivalv2";

            var busUri = new Uri(busBaseUri + "?BusStopCode=" + busStopCode);

            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("User-Agent", "C# App");
                http.DefaultRequestHeaders.Add("Accept", "application/json");
                http.DefaultRequestHeaders.Add("AccountKey", busApiKey);

                using (var response = await http.GetAsync(busUri))
                {
                    var result = await response.Content.ReadAsStringAsync();

                    //  GO BANG MY FUCKING HEAD ON THIS SHIT TOO, 2 HOURS FOR THIS PIECE OF CODE?
                    //  Its optional now, lmao, just take it out whenever u want to, myles
                    var serializer = new DataContractJsonSerializer(typeof(BusRoot),
                        new DataContractJsonSerializerSettings
                        {
                            DateTimeFormat = new DateTimeFormat("yyyy-MM-dd'T'HH:mm:ssK"),
                        });

                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                    {
                        var data = (BusRoot)serializer.ReadObject(ms);
                        return data;
                    }
                }
            }

        }
    }

    [DataContract]
    public class NextBus
    {
        [DataMember]
        public string OriginCode { get; set; }
        [DataMember]
        public string DestinationCode { get; set; }
        [DataMember]
        public string EstimatedArrival { get; set; }
        [DataMember]
        public string Latitude { get; set; }
        [DataMember]
        public string Longitude { get; set; }
        [DataMember]
        public string VisitNumber { get; set; }
        [DataMember]
        public string Load { get; set; }
        [DataMember]
        public string Feature { get; set; }
        [DataMember]
        public string Type { get; set; }
    }

    [DataContract]
    public class NextBus2
    {
        [DataMember]
        public string OriginCode { get; set; }
        [DataMember]
        public string DestinationCode { get; set; }
        [DataMember]
        public string EstimatedArrival { get; set; }
        [DataMember]
        public string Latitude { get; set; }
        [DataMember]
        public string Longitude { get; set; }
        [DataMember]
        public string VisitNumber { get; set; }
        [DataMember]
        public string Load { get; set; }
        [DataMember]
        public string Feature { get; set; }
        [DataMember]
        public string Type { get; set; }
    }

    [DataContract]
    public class NextBus3
    {
        [DataMember]
        public string OriginCode { get; set; }
        [DataMember]
        public string DestinationCode { get; set; }
        [DataMember]
        public string EstimatedArrival { get; set; }
        [DataMember]
        public string Latitude { get; set; }
        [DataMember]
        public string Longitude { get; set; }
        [DataMember]
        public string VisitNumber { get; set; }
        [DataMember]
        public string Load { get; set; }
        [DataMember]
        public string Feature { get; set; }
        [DataMember]
        public string Type { get; set; }
    }

    [DataContract]
    public class BusRoot
    {
        //[JsonProperty("odata.metadata")]
        //public string OdataMetadata { get; set; }
        [DataMember]
        public string BusStopCode { get; set; }
        [DataMember]
        public List<Service> Services { get; set; }
    }

    [DataContract]
    public class Service
    {
        [DataMember]
        public string ServiceNo { get; set; }
        [DataMember]
        public string Operator { get; set; }
        [DataMember]
        public NextBus NextBus { get; set; }
        [DataMember]
        public NextBus2 NextBus2 { get; set; }
        [DataMember]
        public NextBus3 NextBus3 { get; set; }
    }
}
