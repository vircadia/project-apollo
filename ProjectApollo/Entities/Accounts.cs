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
    public class Accounts : EntityStorage
    {
        private static readonly string _logHeader = "[Accounts]";

        private static readonly object accountLock = new object();
        private static Accounts _instance;
        public static Accounts Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (accountLock)
                    {
                        // race condition check
                        if (_instance == null)
                        {
                            _instance = new Accounts();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        // List of all known domains
        private readonly Dictionary<string, AccountEntity> ActiveAccounts = new Dictionary<string, AccountEntity>();


        public Accounts() : base(AccountEntity.AccountEntityTypeName)
        {
        }

        public void Init()
        {
            // Fill my list of domains
            lock (accountLock)
            {
                foreach (AccountEntity anEntity in AllEntities<AccountEntity>())
                {
                    ActiveAccounts.Add(anEntity.AccountID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} DomainEntities",
                            _logHeader, ActiveAccounts.Count.ToString());
            }
        }

        /// <summary>
        /// Find and return a DomainEntity based on the DomainID.
        /// </summary>
        /// <param name="pDomainID"></param>
        /// <param name="oDomain">DomainObject found</param>
        /// <returns></returns>
        public bool TryGetAccountWithID(string pAccountID, out AccountEntity oAccount)
        {
            return ActiveAccounts.TryGetValue(pAccountID, out oAccount);
        }

        public void AddAccount(string pAccountID, AccountEntity pAccountEntity)
        {
            lock (accountLock)
            {
                ActiveAccounts.Add(pAccountID, pAccountEntity);
                pAccountEntity.Updated();
            }
        }
    }

    /// <summary>
    /// Variables and operations on a domain
    /// </summary>
    public class AccountEntity : EntityMem
    {
        public static readonly string AccountEntityTypeName = "Account";

        public string AccountID;    // globally unique account identifier
        public string PlaceName;    // place name
        public string Public_Key;   // DomainServers's public key

        // admin stuff
        public string IPAddrOfFirstContact;     // IP address that created this account
        public DateTime WhenAccountCreated;     // What the variable name says
        public DateTime TimeOfLastHeartbeat;    // time of last heartbeat 

        public AccountEntity() : base(Accounts.Instance)
        {
            WhenAccountCreated = DateTime.UtcNow;
        }

        // EntityMem.EntityType()
        public override string EntityType()
        {
            return DomainEntity.DomainEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return AccountID;
        }
    }
}
