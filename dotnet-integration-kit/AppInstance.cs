using System;



using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jose;
using System.Net.Http;
using System.Net.Http.Headers;


namespace dotnet_integration_kit
{
    

    public class AppInstance
    {
        private AppInstance.AFConfig config;
        private string microModuleId;

        public AppInstance(AppInstance.AFConfig config, string microModuleId)
        {
            this.config = config;
            this.microModuleId = microModuleId;
        }


        private string encrypt(string data, string secret)
        {
            var obj = ((object)new
            {
                af_claim = data
            });
            var secretKey = Encoding.ASCII.GetBytes(secret);
            return Jose.JWT.Encode(JsonConvert.SerializeObject(obj), secretKey, JwsAlgorithm.HS256);
        }

        private string decrypt(string token, string secret)
        {
            dynamic obj = JsonConvert.DeserializeObject(Jose.JWT.Decode(token, Encoding.ASCII.GetBytes(secret), JwsAlgorithm.HS256));
            return JsonConvert.SerializeObject(obj.af_claim);

        }

        public void exec(string intent, object intentData, string userID, Action<object> callback)
        {
            var obj = ((object)new
            {
                intent = intent,
                data = intentData
            });
            HttpClient client = new HttpClient();


            string token = null;


            client.DefaultRequestHeaders.Add("x-uuid", userID);
            client.DefaultRequestHeaders.Add("x-module-handle", this.microModuleId);
            client.DefaultRequestHeaders.Add("x-app-key", this.config.appKey);
            System.Net.Http.HttpContent content;
            if (config.secretKey != null)
            {
                token = encrypt(JsonConvert.SerializeObject(obj), config.secretKey);
                client.DefaultRequestHeaders.Add("x-encrypted", "true");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                content = new StringContent(token, Encoding.UTF8, "text/plain");
            }
            else
            {
                token = JsonConvert.SerializeObject(obj);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                content = new StringContent(token, Encoding.UTF8, "application/json");
            }
            var respose = client.PostAsync(new Uri(this.config.repoUrl + "/executor/exec"), content).ContinueWith((requestTask) =>
            {
                HttpResponseMessage message = requestTask.Result;
                if (message.IsSuccessStatusCode)
                {
                    if (config.secretKey != null)
                    {
                        string result = message.Content.ReadAsStringAsync().Result;
                        callback(JsonConvert.DeserializeObject(decrypt(result, config.secretKey)));
                    }
                    else
                    {
                        string result = message.Content.ReadAsStringAsync().Result;
                        callback(JsonConvert.DeserializeObject(result));
                    }
                }
                else
                {
                    callback(null);
                }
            });
            respose.Wait();
        }


        public object execSync(string intent, object intentData, string userID)
        {
            var obj = ((object)new
            {
                intent = intent,
                data = intentData
            });
            HttpClient client = new HttpClient();


            string token = null;


            client.DefaultRequestHeaders.Add("x-uuid", userID);
            client.DefaultRequestHeaders.Add("x-module-handle", this.microModuleId);
            client.DefaultRequestHeaders.Add("x-app-key", this.config.appKey);
            System.Net.Http.HttpContent content;
            if (config.secretKey != null)
            {
                token = encrypt(JsonConvert.SerializeObject(obj), config.secretKey);
                client.DefaultRequestHeaders.Add("x-encrypted", "true");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));
                content = new StringContent(token, Encoding.UTF8, "text/plain");
            }
            else
            {
                token = JsonConvert.SerializeObject(obj);
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                content = new StringContent(token, Encoding.UTF8, "application/json");
            }

            HttpResponseMessage messge = client.PostAsync(new Uri(this.config.repoUrl + "/executor/exec"), content).Result;




            if (messge.IsSuccessStatusCode)
            {
                if (config.secretKey != null) {
                    string result = messge.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject(decrypt(result, config.secretKey));
                }
                else {
                    string result = messge.Content.ReadAsStringAsync().Result;
                    return JsonConvert.DeserializeObject(result);
                }
            }
            else{
                return null;
            }

        }

        public class AFConfig
        {
            public string repoUrl { get; set; }

            public string secretKey { get; set; }

            public string appKey { get; set; }

            public AFConfig(string repoUrl, string secretKey, string appKey)
            {
                this.repoUrl = repoUrl;
                this.secretKey = secretKey;
                this.appKey = appKey;
            }
        }

    }
}
