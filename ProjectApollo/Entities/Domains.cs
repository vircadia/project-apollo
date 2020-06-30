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
using System.Linq;
using System.Collections.Generic;

using Project_Apollo.Configuration;

namespace Project_Apollo.Entities
{
    public class Domains : EntityStorage
    {
        private static readonly string _logHeader = "[Domains]";

        private static readonly object _domainsLock = new object();
        private static Domains _instance;
        public static Domains Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_domainsLock)
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
            lock (_domainsLock)
            {
                foreach (DomainEntity anEntity in AllStoredEntities<DomainEntity>()) {
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
            if (pDomainID != null)
            {
                lock (_domainsLock)
                {
                    return ActiveDomains.TryGetValue(pDomainID, out oDomain);
                }
            }
            oDomain = null;
            return false;
        }

        /// <summary>
        /// Find and return a DomainEntity based on an APIKey
        /// </summary>
        /// <param name="pAPIKey"></param>
        /// <param name="oDomain">DomainObject found</param>
        /// <returns></returns>
        public bool TryGetDomainWithAPIKey(string pAPIKey, out DomainEntity oDomain)
        {
            if (pAPIKey != null)
            {
                lock (_domainsLock)
                {
                    foreach (var kvp in ActiveDomains)
                    {
                        if (kvp.Value.API_Key == pAPIKey)
                        {
                            oDomain = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            oDomain = null;
            return false;
        }

        /// <summary>
        /// Find and return a DomainEntity based on a SenderKey.
        /// This gets the domain that is being sent from a particular source.
        /// The SenderKey is remembered when domain heartbeats are received.
        /// This lookup is sometime used to verify that requests (like account creation)
        ///     are coming from a known place.
        /// </summary>
        /// <param name="pSenderKey"></param>
        /// <param name="oDomain">DomainObject found</param>
        /// <returns></returns>
        public bool TryGetDomainWithSenderKey(string pSenderKey, out DomainEntity oDomain)
        {
            if (pSenderKey != null)
            {
                lock (_domainsLock)
                {
                    foreach (var kvp in ActiveDomains)
                    {
                        if (kvp.Value.LastSenderKey == pSenderKey)
                        {
                            oDomain = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            oDomain = null;
            return false;
        }

        public void AddDomain(string pDomainID, DomainEntity pDomainEntity)
        {
            lock (_domainsLock)
            {
                ActiveDomains.Add(pDomainID, pDomainEntity);
                pDomainEntity.Updated();
            }
        }
        public void RemoveDomain(DomainEntity pDomainEntity)
        {
            lock (_domainsLock)
            {
                ActiveDomains.Remove(pDomainEntity.DomainID);
            }
        }

        public IEnumerable<DomainEntity> Enumerate()
        {
            List<DomainEntity> aEntities;
            lock (_domainsLock)
            {
                aEntities = new List<DomainEntity>(ActiveDomains.Values);
            }
            return aEntities.AsEnumerable();
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
        public string Public_Key;   // DomainServers's public key
        public string API_Key;      // Access key if a temp domain
        public string SponserAccountID; // The account that gave this domain an access key
        public string IceServerAddr;// IP address of ICE server being used by this domain

        // Information that comes in via heartbeat
        public string Version;      // DomainServer's build version (like "K3")
        public string Protocol;     // Protocol version
        public string NetworkAddr;  // reported network address
        public string NetworkingMode;   // one of "full", ?
        public bool Restricted;     // 'true' if restricted to users with accounts
        public int NumUsers;        // regular users logged in
        public int AnonUsers;       // number of anonymous users
        public int TotalUsers;      // number of users
        public Dictionary<string, int> Hostnames;  // User segmentation

        // More information that's metadata that's passed in PUT domain
        public int Capacity;        // Total possible users
        public string Description;  // Short description of domain
        public string Maturity;     // Maturity rating
        public string Restriction;  // Access restrictions ("open")
        public string[] Hosts;      // Usernames of people who can be domain "hosts"
        public string[] Tags;       // Categories for describing the domain

        // admin stuff
        public string IPAddrOfFirstContact;     // IP address that registered this domain
        public DateTime WhenDomainEntryCreated; // What the variable name says
        public DateTime TimeOfLastHeartbeat;    // time of last heartbeat 
        public string LastSenderKey;            // a key identifying the sender

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
                iceServerAddr = Context.Params.P<string>(AppParams.P_DEFAULT_ICE_SERVER);
            }
            return iceServerAddr;
        }
    }
}
