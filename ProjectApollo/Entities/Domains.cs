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
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using RandomNameGeneratorLibrary;

namespace Project_Apollo.Entities
{
    public class Domains : EntityStorage
    {
        private static readonly string _logHeader = "[Domains]";

        private static readonly object domainLock = new object();
        private static Domains _instance;
        public static Domains Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (domainLock)
                    {
                        // race condition check
                        if (_instance == null)
                        {
                            _instance = new Domains();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        // List of all known domains
        private readonly Dictionary<string, DomainEntity> ActiveDomains = new Dictionary<string, DomainEntity>();


        public Domains() : base(DomainEntity.DomainEntityTypeName)
        {
        }

        public void Init()
        {
            // Fill my list of domains
            lock (domainLock)
            {
                foreach (DomainEntity anEntity in AllEntities<DomainEntity>()) {
                    ActiveDomains.Add(anEntity.DomainID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} DomainEntities",
                            _logHeader, ActiveDomains.Count.ToString());
            }
        }

        /// <summary>
        /// Find and return a DomainEntity based on the DomainID.
        /// </summary>
        /// <param name="pDomainID"></param>
        /// <param name="oDomain">DomainObject found</param>
        /// <returns></returns>
        public bool TryGetDomainWithID(string pDomainID, out DomainEntity oDomain)
        {
            return ActiveDomains.TryGetValue(pDomainID, out oDomain);
        }

        public void AddDomain(string pDomainID, DomainEntity pDomainEntity)
        {
            lock (domainLock)
            {
                ActiveDomains.Add(pDomainID, pDomainEntity);
                pDomainEntity.Updated();
            }
        }
    }

    /// <summary>
    /// Variables and operations on a domain
    /// </summary>
    public class DomainEntity : EntityMem
    {
        public static readonly string DomainEntityTypeName = "Domain";

        public string DomainID;     // globally unique domain identifier
        public string PlaceName;    // place name
        public string IceServerAddr;// IP address of ICE server being used by this domain
        public string API_Key;      // Access key if a temp domain
        public string Public_Key;   // DomainServers's public key
        public string Protocol;     // Protocol version
        public string Version;      // DomainServer's build version (like "K3")
        public bool Restricted;     // 'true' if restricted to users with accounts
        public int TotalUsers;      // number of users
        public int Anon;            // number of anonymous users
        public int LoggedIn;        // regular users logged in
        public string NetworkingMode;   // 'full' or ?

        // admin stuff
        public string IPAddrOfFirstContact;     // IP address that registered this domain
        public DateTime WhenDomainEntryCreated; // What the variable name says
        public DateTime TimeOfLastHeartbeat;    // time of last heartbeat 

        public DomainEntity() : base(Domains.Instance)
        {
            WhenDomainEntryCreated = DateTime.UtcNow;
        }

        // EntityMem.EntityType()
        public override string EntityType()
        {
            return DomainEntity.DomainEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return DomainID;
        }

        /// <summary>
        /// Return the ICE server currently being used by this domain.
        /// The domain does a PUT of the ICE server being used or, if not
        /// set from there, use the default configured for this grid.
        /// Note that a domain server will PUT the address "0.0.0.0" if
        /// it does not yet have an ICE server set. Return default in that case.
        /// </summary>
        /// <returns>string giving IPv4 or IPV6 address of ICE server</returns>
        public string GetIceServerAddr()
        {
            string iceServerAddr = IceServerAddr;
            if (string.IsNullOrEmpty(iceServerAddr) || iceServerAddr == "0.0.0.0")
            {
                iceServerAddr = Context.Params.P<string>("DefaultIceServer");
            }
            return iceServerAddr;
        }
    }

    // ====================================================================
    // After this is all old code that will be thrown away when all
    //   the interesting logic insights have been extracted.
    public class DomainMemory // <--- This is to be used to keep only recently accessed domains in memory!
        // Anything accessed within... the last 2 hours should stay in cached memory, after that it should be unloaded
    {
        #region Base Definitions
        private static readonly object _lck = new object();
        public struct MemoryItem
        {
            private DomainEntity _obj;
            public DateTime LastAccessed;
            public DomainEntity Obj
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
                        DomainEntity obj1 = JsonConvert.DeserializeObject<DomainEntity>(File.ReadAllText($"domains/{ID}.json"));
                        mi1.Obj = obj1;
                        Itms.Add(obj1.DomainID, mi1);
                        return;
                    }
                }
            }

            MemoryItem mi = new MemoryItem();
            DomainEntity obj = new DomainEntity
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
                DomainEntity obj = mi.Obj;
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
                DomainEntity obj = mi.Obj;
                if (obj.API_Key != APIKey) return false;
                obj.IceServerAddr = IP;
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
                DomainEntity obj = mi.Obj;
                if(obj.IceServerAddr == IP)
                {
                    // RA obj.Public_Key = Key;
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
