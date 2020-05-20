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
using System.IO;
using System.Text;

using Newtonsoft.Json;
using RandomNameGeneratorLibrary;

namespace Project_Apollo.Entities
{
    public class Domains : EntityStorage
    {
        private static readonly string _logHeader = "[Domains]";

        // this keeps a list of active domains. Entries age out if not used.
        private Dictionary<string, DomainObject> ActiveDomains = new Dictionary<string, DomainObject>();

        public Domains() : base()
        {

        }

        public bool TryGetDomainWithID(string pDomainID, out DomainObject oDomain)
        {
            oDomain = null;
            return false;
        }

        public void AddDomain(string pDomainID, DomainMemory.MemoryItem pMI)
        {
        }

        // Store the ICE server API key for this domain
        public bool SetIP(string pDomainID, string pRequestorAddr, string pIceServerAPIKey)
        {
            return false;
        }
    }

    /// <summary>
    /// Variables and operations on a domain
    /// </summary>
    public class DomainObject : EntityMem
    {
        public DomainObject() : base()
        {
        }
        public string DomainID;
        public string PlaceName;
        public string IPAddr;
        public string API_Key;
        public string Public_Key;
        public string Protocol;
        public string Version;
        public bool Restricted;
        public int TotalUsers;
        public int Anon;
        public int LoggedIn;
        public string NetworkingMode;

        // The name to index this entity with
        public override string EntityType()
        {
            return "Domain";
        }
        public override string StorageName()
        {
            return DomainID;
        }
        public bool SetPublicKey(string pRemoteUser, byte[] pPublicKey)
        {
            return false;
        }
        public bool SetApiKey(string pRemoteUser, byte[] pPublicKey)
        {
            return false;
        }
    }

    public class DomainMemory // <--- This is to be used to keep only recently accessed domains in memory!
        // Anything accessed within... the last 2 hours should stay in cached memory, after that it should be unloaded
    {
        #region Base Definitions
        private static readonly object _lck = new object();
        public struct MemoryItem
        {
            private DomainObject _obj;
            public DateTime LastAccessed;
            public DomainObject Obj
            {
                get
                {
                    LastAccessed = DateTime.Now;
                    return _obj;
                }
                set
                {
                    _obj = value;
                    LastAccessed = DateTime.Now;
                    SaveItem();
                }
            }

            public void SaveItem()
            {
                lock (_lck)
                {
                    if (!Directory.Exists("domains")) Directory.CreateDirectory("domains");
                    File.WriteAllText("domains/" + Obj.DomainID + ".json", JsonConvert.SerializeObject(Obj, Formatting.Indented));
                }
            }
            
        }


        public Dictionary<string, MemoryItem> Itms = new Dictionary<string, MemoryItem>();
        private void Scan()
        {
            List<string> ToRemove = new List<string>();

            foreach (KeyValuePair<string, MemoryItem> kvp in Itms)
            {
                if (kvp.Value.LastAccessed.AddHours(2) < DateTime.Now)
                {
                    ToRemove.Add(kvp.Key);
                }
            }
            foreach (string rem in ToRemove)
            {
                Itms.Remove(rem);
            }
        }
        public void LoadDomain(string ID, string api_key="")
        {
            if (Directory.Exists("domains"))
            {
                if (File.Exists($"domains/{ID}.json"))
                {
                    lock (_lck)
                    {

                        MemoryItem mi1 = new MemoryItem();
                        DomainObject obj1 = JsonConvert.DeserializeObject<DomainObject>(File.ReadAllText($"domains/{ID}.json"));
                        mi1.Obj = obj1;
                        Itms.Add(obj1.DomainID, mi1);
                        return;
                    }
                }
            }

            MemoryItem mi = new MemoryItem();
            DomainObject obj = new DomainObject
            {
                DomainID = ID
            };
            PlaceNameGenerator pngl = new PlaceNameGenerator();
            obj.PlaceName = pngl.GenerateRandomPlaceName()+"-"+pngl.GenerateRandomPlaceName()+"-"+(new Random()).Next(500, 8000).ToString(); //<-- Does this need to be changed in the future??
            // obj.IPAddr = Session.Instance.CFG.DefaultIceServerAddress;
            obj.API_Key = api_key;
            mi.Obj = obj;

            Itms.Add(obj.DomainID, mi);
        }
        #endregion

        #region Set Info
        public bool SetPlaceName(string DomainID, string PlaceName, string APIKey="")
        {
            Scan();

            if (Itms.ContainsKey(DomainID))
            {
                MemoryItem mi = Itms[DomainID];
                DomainObject obj = mi.Obj;
                if (obj.API_Key != APIKey) return false;
                obj.PlaceName = PlaceName;
                mi.Obj = obj;
                Itms[DomainID] = mi;
                return true;
            }
            else
            {
                // Load to memory
                LoadDomain(DomainID);
                return SetPlaceName(DomainID, PlaceName);
                
            }
        }


        public bool SetIP(string DomainID, string IP, string APIKey="")
        {
            Scan();

            if (Itms.ContainsKey(DomainID))
            {
                MemoryItem mi = Itms[DomainID];
                DomainObject obj = mi.Obj;
                if (obj.API_Key != APIKey) return false;
                obj.IPAddr = IP;
                mi.Obj = obj;
                Itms[DomainID] = mi;
                return true;
            }
            else
            {
                LoadDomain(DomainID);
                return SetIP(DomainID, IP);
            }
        }

        public bool SetPublicKey(string IP, string Key, string ID)
        {
            Scan();

            if (Itms.ContainsKey(ID))
            {
                MemoryItem mi = Itms[ID];
                DomainObject obj = mi.Obj;
                if(obj.IPAddr == IP)
                {
                    obj.Public_Key = Key;
                    mi.Obj = obj;
                    Itms[ID] = mi;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LoadDomain(ID);
                return SetPublicKey(IP, Key, ID);
            }
        }

        #endregion
    }
}
