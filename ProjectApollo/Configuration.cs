using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net;

namespace Project_Apollo
{
    public class Configuration
    {
        [JsonProperty(PropertyName = "Default ICE Server")]
        public string DefaultIceServerAddress;
        public static Configuration LoadConfig()
        {
            if (File.Exists("config.json"))
            {
                return JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            }else
            {
                try
                {

                    Configuration cfg = new Configuration();
                    HttpWebRequest hwr = HttpWebRequest.CreateHttp("https://api.ipify.org");
                    while (hwr.HaveResponse) { }
                    HttpWebResponse hwresp = (HttpWebResponse)hwr.GetResponse();
                    
                    Stream resp = hwresp.GetResponseStream();
                    StreamReader sr = new StreamReader(resp);

                    cfg.DefaultIceServerAddress = sr.ReadToEnd();

                    hwresp.Close();
                    
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(cfg, Formatting.Indented));
                    return cfg;
                } catch(Exception e)
                {
                    throw new Exception();
                }
            }
        }

        [JsonProperty(PropertyName = "Default Port")]
        public int Port=9400;
    }
}
