using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace CognitiveServiceConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting key-phrase extraction:");

            // Need to handle file-open exceptions such as when file doesn't exist
            string textToBeExtracted = System.IO.File.ReadAllText(@"inputText.txt");
            Console.WriteLine(textToBeExtracted.Length);
            var APIEndPoint = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";
            string APIKey = System.IO.File.ReadAllText(@"KPE_key.txt");

            // make a new instance of HttpClient to make POST requests and extract key-phrases
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", APIKey);

            // make number of Document objects based on the length of text
            if (textToBeExtracted.Length < 5120000)
            {
                DocumentCollection collectionOfText = new DocumentCollection();
                collectionOfText.Documents = SplitTextIntoListOfDocuments(textToBeExtracted.Length, textToBeExtracted);

                // serialize the JSON to make the first letter to be lowercase
                var jsonSeralizerSetting = new JsonSerializerSettings();
                jsonSeralizerSetting.ContractResolver = new CamelCasePropertyNamesContractResolver();
                string jsonInputForAPI = JsonConvert.SerializeObject(collectionOfText, jsonSeralizerSetting);

                // make the Http POST request
                HttpContent jsonRequestBody = new StringContent(jsonInputForAPI);
                jsonRequestBody.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = httpClient.PostAsync(APIEndPoint, jsonRequestBody).Result;

                if (response.IsSuccessStatusCode)
                {
                    // parse the JSON
                    JObject jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result.ToString());
                    var jArray = jObject["documents"] as JArray;
                    foreach (JObject item in jArray.Children())
                    {
                        var keyPhrases = item.GetValue("keyPhrases");
                        Console.WriteLine(keyPhrases);
                    }
                    //Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("ERROR:");
                    Console.WriteLine(response.StatusCode.ToString());
                    Console.ReadKey();
                }

            }
        }

        static public Document[] SplitTextIntoListOfDocuments(int totalLengthOfString, string textToBeExtracted)
        {
            // if the total length of the document is less than 5120*101 number of characters
            // MAX NUMBER OF DOCUMENTS 1 REQUEST CAN HANDLE IS UPTO 101 or else throw error
            int numOfDocuments = totalLengthOfString / 5120;
            int remainderNumOfChars = totalLengthOfString - numOfDocuments * (5120);
            Document[] documents;
            if (remainderNumOfChars == 0)
            {
                documents = new Document[numOfDocuments];
            }
            else
            {
                documents = new Document[numOfDocuments + 1];
            }

            Console.WriteLine("number of documents: {0}", numOfDocuments.ToString());
            // use a while loop and iterate through every 5120 characters
            int i = 0;
            while (i < numOfDocuments)
            {
                documents[i] = new Document();
                documents[i].Id = Convert.ToString(i);
                documents[i].Language = "en"; // TODO: Need to pass language through language detection
                documents[i].Text = textToBeExtracted.Substring(i * 5120, 5120);
                i++;
            }

            if (remainderNumOfChars != 0)
            {
                documents[i] = new Document();
                documents[i].Id = Convert.ToString(i);
                documents[i].Language = "en"; // TODO: Need to pass language through language detection
                documents[i].Text = textToBeExtracted.Substring(i * 5120, remainderNumOfChars);
            }
            return documents;
        }

    }

}
