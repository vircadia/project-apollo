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
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace Project_Apollo.Entities
{
// ========================================================= 20200607
// Old code that is being kept around until all of its knowledge is extracted.
// ==================================================================

/*
    /// <summary>
    /// Class for all things user.
    /// Access an Instance of this class to make requests for and about users.
    ///
    /// NOTE: the instance is referenced by many threads so be careful of
    /// locking and sharing variables.
    /// </summary>
    public class Users : EntityStorage
    {
        private static readonly string _logHeader = "[Users]";

        private static readonly object userLock = new object();
        private static Users _instance;
        public static Users Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (userLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Users();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        // List of all known users
        private readonly Dictionary<string, UserEntity> ActiveUsers = new Dictionary<string, UserEntity>();

        public Users() : base(UserEntity.UserEntityTypeName)
        {
        }

        public void Init()
        {
            // Fill my list of users
            lock (userLock)
            {
                foreach (UserEntity anEntity in AllEntities<UserEntity>()) {
                    ActiveUsers.Add(anEntity.UserID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} DomainEntities",
                            _logHeader, ActiveUsers.Count.ToString());
            }
        }

        /// <summary>
        /// Lookup a user based on their authorization.
        /// </summary>
        /// <param name="pAuthToken"></param>
        /// <returns>User information access class or 'null' if no authorized user</returns>
        public UserEntity FindUserWithAuth(string pAuthToken)
        {
            return new UserEntity();
        }
        public bool TryGetUserWithAuth(string pAuthToken, out UserEntity oUser)
        {
            bool ret = false;
            UserEntity retUser = null;
            foreach (var user in ActiveUsers.Values)
            {
                if (user.AuthKey == pAuthToken)
                {
                    ret = true;
                    retUser = user;
                    break;
                }
            }
            oUser = retUser;
            return ret;
        }
        public bool TryGetUserWithID(string pUserID, out UserEntity oUser)
        {
            return ActiveUsers.TryGetValue(pUserID, out oUser);
        }

        /// <summary>
        /// Create a password authenticated user.
        /// </summary>
        /// <param name="pUsername"></param>
        /// <param name="pUserPW"></param>
        /// <param name="pUserEmail"></param>
        /// <returns>'true' if created, 'false' if not</returns>
        public bool CreateAccountPW(string pUsername, string pUserPW, string pUserEmail)
        {
            return false;
        }
    }

    /// <summary>
    /// Wrapper for accessing per-user information
    /// </summary>
    public class UserEntity : EntityMem
    {
        public static readonly string UserEntityTypeName = "User";

        public string UserID;
        public string Username;
        public string Public_Key;
        public string PasswordHash;
        public string PasswordSalt;

        public bool Online;
        public string Connection;
        public UserLocation Location;

        public UserImages Images;

        public string IPAddrOfCreator;     // IP address that created this user
        public DateTime WhenUserEntryCreated;
        public DateTime TimeOfLastHeartbeat;

        public UserEntity() : base(Users.Instance)
        {
            WhenUserEntryCreated = DateTime.UtcNow;
        }
        // EntityMem.EntityType()
        public override string EntityType()
        {
            return DomainEntity.DomainEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return UserID;
        }

    }

    public class UserLocation
    {
        public string Path;
        public string NodeID;
        public UserRoot Root;
        public bool Online;
    }

    public class UserImages
    {
        public string Hero;
        public string Thumbnail;
        public string Tiny;
    }

    public class UserRoot
    {
        public string Name;
        public string Id;
        public MetaverseDomain Domain;
    }

    // this defn is left over from early code
    // Will probably end up a pointer to a DomainEntity
    public class MetaverseDomain
    {
        public string Id;
        public string ICEServerAddress;
        public string NetworkAddress;
        public int NetworkPort;
        public bool Online;
        public string DefaultPlaceName;
        public bool IsCloudDomain;
    }

    // ====================================================================
    // After this is all old code that will be thrown away when all
    //   the interesting logic insights have been extracted.
    public class UserAccounts
    {
        private static readonly object _lock = new object();
        public static UserAccounts GetAccounts()
        {
            lock (_lock)
            {

                if (!File.Exists("accounts.json"))
                {
                    UserAccounts ua = new UserAccounts();
                    return ua;
                }
                string js = File.ReadAllText("accounts.json");
                return (UserAccounts)JsonConvert.DeserializeObject<UserAccounts>(js);
            }
        }

        public struct Location
        {
            public bool connected;
            public string network_address;
            public string availability;
            public int network_port;
            public string node_id;
            public string domain_id;
            public string path;
        }

        public class Account
        {
            public string name;
            public string email;
            public string pwSalt;
            public string pwHash;
            public string account_settings;
            public string PubKey;
            public Location LastLocation;
            public Dictionary<string, string> ActiveTokens;
            public void GenAccount(string A, string B, string C)
            {
                name = A;
                email = B;
                pwSalt = Tools.SHA256Hash(Tools.getTimestamp().ToString() + ";" + (new Random().Next(99999999)).ToString());
                pwHash = Tools.MD5Hash(Tools.MD5Hash(C) + ":" + Tools.MD5Hash(pwSalt));
            }
            public Account()
            {
                ActiveTokens = new Dictionary<string, string>();
            }
        }
        private bool LastStatus;
        public bool GetStatus()
        {
            return LastStatus;
        }
        public Dictionary<string, Account> AllAccounts = new Dictionary<string, Account>();
        public void CreateAccount(string name, string email, string password)
        {
            Account a = new Account();
            a.GenAccount(name, email, password);
            LastStatus = true;
            if (!AllAccounts.ContainsKey(name))
                AllAccounts.Add(name, a);
            else
                LastStatus = false; // set to false IF the creation fails!

            save();
        }


        public bool Login(string name, string password, string access_token)
        {

            if (AllAccounts.ContainsKey(name))
            {
                Account act = AllAccounts[name];

                if (Tools.MD5Hash(Tools.MD5Hash(password) + ":" + Tools.MD5Hash(act.pwSalt)) == act.pwHash)
                {
                    if (access_token != "web")
                    {

                        if (act.ActiveTokens.ContainsKey(access_token))
                            act.ActiveTokens[access_token] = "owner";
                        else
                            act.ActiveTokens.Add(access_token, "owner");
                    }
                    Console.WriteLine("====> New Access Token: " + name + "; " + access_token);
                    AllAccounts[name] = act;
                    save();
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public string GetAccountSettings(string AccessToken)
        {
            Account a = new Account();
            int i = 0;
            foreach (string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    return Tools.Base64Decode(a.account_settings);
                }
            }

            return "";
        }

        public void SetAccountSettings(string AccessToken, string AccountSettings)
        {
            Account a = new Account();
            foreach (string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    a.account_settings = Tools.Base64Encode(AccountSettings);
                    AllAccounts[user] = a;
                    save();
                    return;
                }
            }
        }
        public void UpdateLocation(Location loc, string AccessToken)
        {
            Account a = new Account();
            foreach (string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    a.LastLocation = loc;
                    AllAccounts[user] = a;
                    save();
                    return;
                }
            }
        }

        public Location GetLocation(string user)
        {
            if (AllAccounts.ContainsKey(user))
            {
                return AllAccounts[user].LastLocation;
            }
            else return new Location();
        }

        public string GetAccountName(string AccessToken)
        {
            Account a = new Account();
            foreach (string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    return user;
                }
            }
            return "not_found";
        }

        public string SetPublicKey(string PubKey, string AccessToken)
        {
            Account a = new Account();
            foreach (string key in AllAccounts.Keys)
            {
                a = AllAccounts[key];

                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    ///////////////////
                    //// TODO: CHECK WHAT ENCODING IS EXPECTED BY THE CLIENT
                    //// 2: CHANGE EXPECTED POST HANDLING, I HATE DEALING WITH OCTET STREAMS EVEN ON A PHP SERVER!
                    ///
                    /// SAMPLE REPLY FROM ORIGINAL METAVERSE SERVER
                    /// {"status":"success","data":{"public_key":"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA3KTcsZZ1qLbtFAeP\njksyoTGk6cU/kUDf98Vo1otWFI0erVmJI0Sf4Ukv1fIMlIRoke8k2fIyefAt\nCJ2cDvNkEOPCgV0ibcOx/P/sEdlSBm8Ke+Sk5Ov3TQyFlsOPRrtL7mJtnVpd\nqoxc+Arsi3UIuOp0YhyLm5HFyaQQr4gnKrPb7jqjBag7BGlb59zH7cc2X7Fr\nTm6DR7bhWG8XGZZ5yT6SjZjDq93vhOTBr5tMc8g/wg/irPsgHMf+SEpSZzRK\nJXuEXE4Jn6SHAFJ0emhLfIIpeXHEDHtFDJyhFt0HeVWFW9QqylxY+cTN/y6L\n1/7EPoWIwFFQQUxRTdNTKKnVKQIDAQAB\n"}}
                    /// 
                    /// 
                    a.PubKey = Tools.Base64Encode(PubKey);
                    AllAccounts[key] = a;
                    save();
                    return "success";
                }
            }
            return "fail";
        }

        public string GetPublicKey(string user)
        {
            if (AllAccounts.ContainsKey(user)) return AllAccounts[user].PubKey;
            return "no such user";
        }

        public void save()
        {
            File.WriteAllText("accounts.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
*/
}
