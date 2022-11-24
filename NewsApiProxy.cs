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
    public class NewsApiProxy
    {
        public async static Task<NewsRoot> GetNews()
        {
            //string newsApiKey = "";
            string newsApiKey = "add2204719234cbf87d564682aa3c7ba";
            //string newsApiKey = "ac6fadff50149efb4dc7b59ae07044d";

            string countrySource = "sg";
            string newsType = "top-headlines";
            string newsBaseUri = "https://newsapi.org/v2/";
            
            var newsUri = new Uri(newsBaseUri + newsType + "?country=" + countrySource + "&apiKey=" + newsApiKey);
            
            using (var http = new HttpClient())
            {
                // FUCKING IMPORTANT CODE BELOW, IVE BEEN BANGING MY HEAD FOR 12HRS STRAIGHT ON THIS SHIT FFS,
                // STUPID CODE WHEN WILL THIS HELL END? HTTP MY DISTINCTION UP MY ASS PLS, GIVE ME MY GRADE AND FUCK OFF ALRDY
                // THIS ASSIGNMENT = GIVE ME A STICK AND GO KILL GLOBAL WARMING LIKE WTF???? I ALRDY LEARN A LOT MORE FROM
                // ONLINE INSTEAD OF THE ACTUAL LESSONS AND STILL EXPECT US TO HAND IN QUALITY WORK, JUST WHY THE FUCK SO
                // DEMANDING ONE SIA WALAO
                // CANT EVEN TEACH US EVERYHTING ABOUT API, HTTP, AND I NEED GO FIGURE OUT THIS IMPT PART ON MY OWN 
                // GIVE ME A WALL SO THAT I CAN GO BANG AND BANG BANGBANGABANABGGANABGNABGNGNAG MY HEAD TILL I DIE PLEASEPLSPLS
                http.DefaultRequestHeaders.Add("User-Agent", "C# App"); // <---- fuck u http, this 1 liner is a killer

                using (var response = await http.GetAsync(newsUri))
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var serializer = new DataContractJsonSerializer(typeof(NewsRoot));

                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result)))
                    {
                        var data = (NewsRoot)serializer.ReadObject(ms);
                        return data;
                    } 
                }
            }

        }
    }

    [DataContract]
    public class Article
    {
        [DataMember]
        public Source source { get; set; }
        [DataMember]
        public string author { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public string url { get; set; }
        [DataMember]
        public string urlToImage { get; set; }
        //[DataMember]
        //public DateTime publishedAt { get; set; }
        [DataMember]
        public string content { get; set; }
    }

    [DataContract]
    public class NewsRoot
    {
        [DataMember]
        public string status { get; set; }
        [DataMember]
        public int totalResults { get; set; }
        [DataMember]
        public List<Article> articles { get; set; }
        [DataMember]
        public string code { get; set; }
        [DataMember]
        public string message { get; set; }
    }

    [DataContract]
    public class Source
    {
        [DataMember]
        public string id { get; set; }
        [DataMember]
        public string name { get; set; }
    }
}
