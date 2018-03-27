using System;



using Newtonsoft.Json;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jose;

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
            return Jose.JWT.Decode(token, Encoding.ASCII.GetBytes(secret), JwsAlgorithm.HS256);

        }

        public void exec(string intent, object intentData, string userID, Action<string> callback)
        {
            var obj = ((object)new
            {
                intent = intent,
                data = intentData
            });
            Console.WriteLine(this.config.repoUrl + "/executor/exec");
            RestClient restClient = new RestClient(this.config.repoUrl + "/executor/exec");
            RestRequest restRequest = new RestRequest(Method.POST);
            string token = null;
            if (config.secretKey != null)
            {
                token = encrypt(JsonConvert.SerializeObject(obj), config.secretKey);
                restRequest.AddParameter("text/plain", (object)token, ParameterType.RequestBody);
            }
            else
            {
                token = JsonConvert.SerializeObject(obj);
                restRequest.AddParameter("application/json", (object)token, ParameterType.RequestBody);
            }
            restRequest.AddHeader("x-uuid", userID);
            restRequest.AddHeader("x-module-handle", this.microModuleId);
            restRequest.AddHeader("x-app-key", this.config.appKey);

            restClient.ExecuteAsync(restRequest, response => {
                callback(response.Content);
            });
        }

        public string execSync(string intent, JsonToken intentData, string userID, Action<string> callback)
        {
            var obj = ((object)new
            {
                intent = intent,
                data = intentData
            });
            Console.WriteLine(this.config.repoUrl + "/executor/exec");
            RestClient restClient = new RestClient(this.config.repoUrl + "/executor/exec");
            RestRequest restRequest = new RestRequest(Method.POST);
            string token = null;
            if (config.secretKey != null)
            {
                token = encrypt(JsonConvert.SerializeObject(obj), config.secretKey);
                restRequest.AddParameter("text/plain", (object)token, ParameterType.RequestBody);
            }
            else
            {
                token = JsonConvert.SerializeObject(obj);
                restRequest.AddParameter("application/json", (object)token, ParameterType.RequestBody);
            }
            restRequest.AddHeader("x-uuid", userID);
            restRequest.AddHeader("x-module-handle", this.microModuleId);
            restRequest.AddHeader("x-app-key", this.config.appKey);

            var response = restClient.Execute(restRequest);
            return response.Content;
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
