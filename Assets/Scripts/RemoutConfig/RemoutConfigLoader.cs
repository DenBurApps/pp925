using Flagsmith;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace RemoutConfig
{
    public class RemoutConfigLoader : MonoBehaviour
    {
        [SerializeField] private string _key;
        [SerializeField] private ConfigData _allConfigData;
        private bool _showTerms = true;
        private static FlagsmithClient _flagsmithClient;

        public Action<bool> ConfigLoadEnded;

        private void Start()
        {
#if UNITY_IOS
        if (!PlayerPrefs.HasKey("Onboarding"))
        {
        Device.RequestStoreReview();
        }
#endif
            //Перед получением данных из конфига инициализируйте свою систему сохранений, чтоб не было null при сохранении ссылки
            _flagsmithClient = new(_key);
            StartLoading();
        }


        private void StartLoading()
        {
            string HtmlText = GetHtmlFromUri("http://google.com");

            Debug.Log("Google result: " + HtmlText);

            if (HtmlText != "")
            {
                if(_key != "")
                {
                    LoadRemoutConfig();
                }
                else
                {
                    Debug.Log("Missing Flagsmith key");
                    LoadScene();
                }
            }

            else
            {
                Debug.Log("No internet");
                LoadScene();
            }
        }

        public void LoadRemoutConfig()
        {
            _ = StartAsync();
        }

        async System.Threading.Tasks.Task StartAsync()
        {
            var flags = await _flagsmithClient.GetEnvironmentFlags();
            if (flags == null)
            {
                Debug.Log("flags null");
                LoadScene();
                return;
            }
            string values = await flags.GetFeatureValue("config");
            if (values == null || values == "")
            {
                Debug.Log("values null");
                LoadScene();
                return;
            }
            Debug.Log("Loaded");
            ProcessJsonResponse(values);
        }

        private void ProcessJsonResponse(string jsonResponse)
        {
            ConfigData config = JsonConvert.DeserializeObject<ConfigData>(jsonResponse);
            _allConfigData.link = config.link;
            _allConfigData.usePrivacy = config.usePrivacy;

            Debug.Log("link's value from Config: " + _allConfigData.link);
            Debug.Log("useprivacy's value from Config: " + _allConfigData.usePrivacy);

            _showTerms = _allConfigData.usePrivacy;
            //Место для сохранения ссылки (Сохранять в плеер префс нельзя поскольку сброс данных приложения обнуляет эту информацию)

            //var reg = SaveSystem.LoadData<RegistrationSaveData>();
            //reg.Link = _allConfigData.link;
            //SaveSystem.SaveData(reg);
            LoadScene();

        }

        private void LoadScene()
        {
            ConfigLoadEnded?.Invoke(_showTerms);
        }

        public string GetHtmlFromUri(string resource)
        {
            string html = string.Empty;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(resource);
            try
            {
                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                {
                    bool isSuccess = (int)resp.StatusCode < 299 && (int)resp.StatusCode >= 200;
                    if (isSuccess)
                    {
                        using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                        {
                            //We are limiting the array to 80 so we don't have
                            //to parse the entire html document feel free to 
                            //adjust (probably stay under 300)
                            char[] cs = new char[80];
                            reader.Read(cs, 0, cs.Length);
                            foreach (char ch in cs)
                            {
                                html += ch;
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
            return html;
        }

    }

    [Serializable]
    public class ConfigData
    {
        public string link;
        public bool usePrivacy;
    }
}
