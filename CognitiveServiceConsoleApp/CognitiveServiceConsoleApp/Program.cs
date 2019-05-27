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
            string textToBeExtracted = System.IO.File.ReadAllText(@"C:\Users\Joyli\Desktop\inputText.txt");
            var APIEndPoint = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";
            string APIKey = System.IO.File.ReadAllText(@"C:\Users\Joyli\Desktop\KPE_key.txt");

            // make a new instance of HttpClient to make POST requests and extract key-phrases
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", APIKey);

            // make number of Document objects based on the length of text
            if (textToBeExtracted.Length < 5120000)
            {
                Document documentOfText = new Document();
                documentOfText.Id = "1";
                documentOfText.Language = "en";
                documentOfText.Text = textToBeExtracted;

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
                    Console.WriteLine(response.StatusCode.ToString());
                    Console.ReadKey();
                }
              
            }
        }

        static public Document[] SplitTextIntoListOfDocuments(int totalLengthOfString, string textToBeExtracted)
        {
            // if the total length of the document is less than 5120*1000 number of characters
            // then make a total of 1000 instances of Documents
            if (totalLengthOfString < 5120000)
            {
                int numOfDocuments = totalLengthOfString / 5120;
                int remainderNumOfChars = totalLengthOfString - numOfDocuments * (5120);
                Document[] documents = new Document[numOfDocuments + 1];
                for (int j = 0; j<numOfDocuments + 1; j++)
                {
                    documents[j] = new Document();
                    documents[j].Id = j.ToString();
                    documents[j].Language = "en";
                    documents[j].Text = ""; // THINK OF WAYS TO FIX THE "MISSING INPUT ELEMENT BUG"
                }
                // use a while loop and iterate through every 5120 characters
                int i = 0;
                while (i <numOfDocuments)
                {
                    documents[i].Id = Convert.ToString(i);
                    documents[i].Language = "en"; // TODO: Need to pass language through language detection
                    if ((i+1)*5120 < textToBeExtracted.Length)
                    {
                        documents[i].Text = textToBeExtracted.Substring(i * 5120, 5120);
                    }
                    i++;
                }
                
                if (remainderNumOfChars != 0)
                {
                    documents[i].Id = Convert.ToString(i);
                    documents[i].Language = "en"; // TODO: Need to pass language through language detection
                    documents[i].Text = textToBeExtracted.Substring(i * 5120, remainderNumOfChars);
                }
                return documents;
            }
            return new Document[5];
        }
    }

}
