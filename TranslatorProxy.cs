using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Project2_203149T
{
    public class TranslatorProxy
    {
        public async static Task<TranslateRoot> TranslateText(string textInput, string targetLanguage)
        {
			string translatorApiKey = "76adac93demsh254fbe1aad08dcdp120c9ejsncd83e805e1ca";

			string textType = "plain";
			string apiVersion = "3.0";
			string profanityAction = "NoAction";
			string translatorBaseUri = "https://microsoft-translator-text.p.rapidapi.com/translate?to%5B0%5D=";

			using (var client = new HttpClient())
            {
				var request = new HttpRequestMessage
				{
					Method = HttpMethod.Post,
					RequestUri = new Uri(translatorBaseUri + targetLanguage + "&api-version=" + apiVersion + "&profanityAction=" + profanityAction +"&textType=" + textType),
					Headers =
					{
						{ "X-RapidAPI-Key", translatorApiKey },
						{ "X-RapidAPI-Host", "microsoft-translator-text.p.rapidapi.com" },
					},
					Content = new StringContent("[\r{\r\"Text\": \"" + textInput + "\"\r}\r]")
					{
						Headers =
						{
							ContentType = new MediaTypeHeaderValue("application/json")
						}
					}
				};

				using (var response = await client.SendAsync(request))
                {
					response.EnsureSuccessStatusCode();
					var result = await response.Content.ReadAsStringAsync();
					var result2 = result.Substring(1, result.Length - 1);		// take out '[' and ']' from Json response in order to serialize properly
					var serializer = new DataContractJsonSerializer(typeof(TranslateRoot));

					using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(result2)))
                    {
						request.Dispose();
						var data = (TranslateRoot)serializer.ReadObject(ms);
						return data;
					}
				}
			}
			
		}
    }

	[DataContract]
	public class DetectedLanguage
	{
		[DataMember]
		public string language { get; set; }
		[DataMember]
		public double score { get; set; }
	}

	[DataContract]
	public class TranslateRoot
	{
		[DataMember]
		public DetectedLanguage detectedLanguage { get; set; }
		[DataMember]
		public List<Translation> translations { get; set; }
	}

	[DataContract]
	public class Translation
	{
		[DataMember]
		public string text { get; set; }
		[DataMember]
		public string to { get; set; }
	}
}
