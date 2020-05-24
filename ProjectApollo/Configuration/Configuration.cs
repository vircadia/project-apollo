//   Copyright 2020 Vircadia
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http;
using System.Net;

namespace Project_Apollo
{
/// <summary>
/// This is old code that is kept around until I figure out what this does,
/// Once the functionality is duplicated, this file will be deleted.
/// </summary>
    public class XXConfiguration
    {
        [JsonProperty(PropertyName = "Default ICE Server")]
        public string DefaultIceServerAddress;
        public static XXConfiguration LoadConfig()
        {
            if (File.Exists("config.json"))
            {
                return JsonConvert.DeserializeObject<XXConfiguration>(File.ReadAllText("config.json"));
            }else
            {
                try
                {

                    XXConfiguration cfg = new XXConfiguration();
                    HttpWebRequest hwr = HttpWebRequest.CreateHttp("https://api.ipify.org");
                    while (hwr.HaveResponse) { }
                    HttpWebResponse hwresp = (HttpWebResponse)hwr.GetResponse();
                    
                    Stream resp = hwresp.GetResponseStream();
                    StreamReader sr = new StreamReader(resp);

                    cfg.DefaultIceServerAddress = sr.ReadToEnd();

                    hwresp.Close();
                    
                    File.WriteAllText("config.json", JsonConvert.SerializeObject(cfg, Formatting.Indented));
                    return cfg;
                }
                catch(Exception e)
                {
                    throw new Exception();
                }
            }
        }

        [JsonProperty(PropertyName = "Default Port")]
        public int Port=9400;
    }
}
