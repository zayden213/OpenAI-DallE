using System;
using System.Collections;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Scripts.Constants;
using Unity.VisualScripting;

namespace Project.Scripts
{
    public class DallEGenerator : MonoBehaviour
    {
        private static int _serverCallCountSafetyIncrement;

        [Header("Info")] 
        [SerializeField] private string key;
        
        public class DallEParamsHelper
        {
            public string prompt;
            [JsonProperty("response_format")] public string responseFormat = "b64_json";
            [JsonProperty("user")] public string userId;
            [JsonProperty("n")] public int amount = 1;
            [JsonProperty("size")] public string size = "256x256";
        }

        public async UniTask GetImage(DallEParamsHelper serializableParams, Action<Texture2D> createTextureCallback)
        {
            if (string.IsNullOrEmpty(key) == true)
                throw new NullReferenceException("Could not find OpenAI API key");
            
            if (_serverCallCountSafetyIncrement < MagicNumbers.MAX_CALL_COUNT_DALLE)
            {
                _serverCallCountSafetyIncrement++;
                
                const string apiUrl = "https://api.openai.com/v1/images/generations";
                
                var jsonString = JsonConvert.SerializeObject(serializableParams, Formatting.None, 
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });

                using var www = UnityWebRequest.Post(apiUrl, "");
                www.SetRequestHeader("Authorization", "Bearer " + key);
                www.SetRequestHeader("Content-Type", "application/json");
                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonString));
                    
                await www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                    throw new InvalidConnectionException($"Result not available at {www.result}");
                
                var result = www.downloadHandler.text;

                if (JsonConvert.DeserializeObject(result) is not JObject jsonData)
                    throw new NullReferenceException("Could not get Json Data");
                
                var jsonToken = jsonData.SelectToken("data[0]['" + serializableParams.responseFormat + "']");
                var base64Image = jsonToken?.ToString();
                
                if (string.IsNullOrEmpty(base64Image) == true)
                    throw new FileNotFoundException();
                
                var data = Convert.FromBase64String(base64Image);
                createTextureCallback?.Invoke(GetTextureFromData(data));
            }
            else
            {
                throw new Exception("Called server too many times. Limit reached.");
            }
        }

        private static Texture2D GetTextureFromData(byte[] data)
        {
            var texture = new Texture2D(0, 0);
            ImageConversion.LoadImage(texture, data);
            return texture;
        }
    }
}