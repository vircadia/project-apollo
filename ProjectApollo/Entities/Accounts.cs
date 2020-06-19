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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using Project_Apollo.Configuration;

namespace Project_Apollo.Entities
{
    public class Accounts : EntityStorage
    {
        private static readonly string _logHeader = "[Accounts]";

        private static readonly object _accountLock = new object();
        private static Accounts _instance;
        public static Accounts Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_accountLock)
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
            CreateAuthTokenExpirationBackgroundTask();
        }

        public void Init()
        {
            // Fill my list of domains
            lock (_accountLock)
            {
                foreach (AccountEntity anEntity in AllStoredEntities<AccountEntity>())
                {
                    ActiveAccounts.Add(anEntity.AccountID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} AccountEntities",
                            _logHeader, ActiveAccounts.Count.ToString());
            }
        }

        /// <summary>
        /// Find and return a AccountEntity based on the AccountID.
        /// </summary>
        /// <param name="pAccountID"></param>
        /// <param name="oAccount">AccountEntity found</param>
        /// <returns></returns>
        public bool TryGetAccountWithID(string pAccountID, out AccountEntity oAccount)
        {
            if (pAccountID == null)
            {
                oAccount = null;
                return false;
            }
            lock (_accountLock)
            {
                return ActiveAccounts.TryGetValue(pAccountID, out oAccount);
            }
        }

        /// <summary>
        /// Find the account information based on the passed AuthToken.
        /// </summary>
        /// <param name="pAccountID"></param>
        /// <param name="oAccount">AccountEntity found</param>
        /// <returns></returns>
        public bool TryGetAccountWithAuthToken(string pAuthToken, out AccountEntity oAccount)
        {
            if (pAuthToken != null)
            {
                lock (_accountLock)
                {
                    foreach (var kvp in ActiveAccounts)
                    {
                        if (kvp.Value.GetAuthTokenInfo(pAuthToken) != null)
                        {
                            oAccount = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            oAccount = null;
            return false;
        }
        /// <summary>
        /// Find the account information based on the passed AuthToken.
        /// </summary>
        /// <param name="pAccountID"></param>
        /// <param name="oAccount">AccountEntity found</param>
        /// <returns></returns>
        public bool TryGetAccountWithUsername(string pUsername, out AccountEntity oAccount)
        {
            if (pUsername != null)
            {
                string lowerUsername = pUsername.ToLower();
                lock (_accountLock)
                {
                    foreach (var kvp in ActiveAccounts)
                    {
                        if (kvp.Value.Username.ToLower() == lowerUsername)
                        {
                            oAccount = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            oAccount = null;
            return false;
        }

        public void AddAccount(AccountEntity pAccountEntity)
        {
            lock (_accountLock)
            {
                ActiveAccounts.Add(pAccountEntity.AccountID, pAccountEntity);
                pAccountEntity.Updated();
            }
        }

        /// <summary>
        /// Create an account using the info supplied.
        /// </summary>
        /// <param name="pUserName"></param>
        /// <param name="pPassword"></param>
        /// <param name="pEmail"></param>
        /// <returns>The created AccountEntity or 'null' if the account could not be created</returns>
        public AccountEntity CreateAccount(string pUserName, string pPassword, string pEmail)
        {
            AccountEntity newAcct = null;
            lock (_accountLock)
            {
                // Verify that the user name is unique
                string lowerUsername = pUserName.ToLower();
                var matchingAccts = ActiveAccounts.Where(kvp =>
                    {
                        return lowerUsername == kvp.Value.Username.ToLower();
                    }
                );
                if (matchingAccts.Count() == 0)
                {
                    newAcct = new AccountEntity()
                    {
                        Username = pUserName,
                        Email = pEmail,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(pPassword)
                    };

                    AddAccount(newAcct);
                }
            }
            return newAcct;
        }

        // Hash the passed password
        public bool ValidPassword(AccountEntity pAcct, string pPassword)
        {
            return BCrypt.Net.BCrypt.Verify(pPassword, pAcct.PasswordHash);
        }

        // Return an IEnumerable of all the accounts.
        // This takes a snapshot of the current account list and returns those.
        public IEnumerable<AccountEntity> AllAccountEntities()
        {
            List<AccountEntity> aEntities;
            lock (_accountLock)
            {
                aEntities = new List<AccountEntity>(ActiveAccounts.Values);
            }
            foreach (AccountEntity ent in aEntities)
            {
                yield return ent;
            }
            yield break;
        }
        public void ExpireAuthTokens()
        {
            lock (_accountLock)
            {
                foreach (var acct in ActiveAccounts.Values)
                {
                    int numExpired = 0;
                    for (int ii = acct.AuthTokens.Count - 1; ii >= 0; ii--)
                    {
                        var token = acct.AuthTokens[ii];
                        if (token.HasExpired())
                        {
                            acct.AuthTokens.RemoveAt(ii);
                            numExpired++;
                            Context.Log.Debug("{0} Expiring AuthToken for acct {1}",
                                            _logHeader, acct.Username);
                        }
                    }
                    if (numExpired > 0)
                    {
                        acct.Updated();
                    }
                }
            }
        }
        /// <summary>
        /// Create a background thread that periodically checks to see if
        ///     any account AccessTokens have expired.
        /// </summary>
        private void CreateAuthTokenExpirationBackgroundTask()
        {
            // TODO: start up AuthToken expiration checker
            Task.Run(() =>
            {
                CancellationToken stopToken = Context.KeepRunning.Token;
                WaitHandle waiter = stopToken.WaitHandle;
                int sleepMS = Context.Params.P<int>(AppParams.P_ACCOUNT_AUTHTOKENEXPIRATIONCHECKSECONDS) * 1000;
                try
                {
                    while (!stopToken.IsCancellationRequested)
                    {
                        Accounts.Instance.ExpireAuthTokens();
                        waiter.WaitOne(sleepMS);
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Exception in AuthToken expiration backround job. {1}",
                                        _logHeader, e);
                }
                Context.Log.Debug("{0} AuthToken expiration backround job exiting", _logHeader);
            });
        }
    }

    /// <summary>
    /// The account information.
    /// </summary>
    public class AccountEntity : EntityMem
    {
        public static readonly string AccountEntityTypeName = "Account";

        public string AccountID;                // globally unique account identifier
        public string Username;
        public string Email;
        public string PasswordHash;

        public string Public_Key;
        // tokens for access by this user
        public List<AuthTokenInfo> AuthTokens = new List<AuthTokenInfo>();

        public string AccountSettings;          // JSON of client settings

        public UserImages Images;

        public LocationInfo Location;           // Where the user says they are

        public List<string> Connections = new List<string>();
        public List<string> Friends = new List<string>();

        // Old stuff
        public string xmpp_password;
        public string discourse_api_key;
        public string wallet_id;

        // admin stuff
        public string IPAddrOfCreator;          // IP address that created this account
        public DateTime WhenAccountCreated;     // What the variable name says
        public DateTime TimeOfLastHeartbeat;    // time that we had a heartbeat for this account

        public AccountEntity() : base(Accounts.Instance)
        {
            AccountID = Guid.NewGuid().ToString();
            WhenAccountCreated = DateTime.UtcNow;

            // Old stuff
            xmpp_password = Tools.SHA256Hash("deprecated");
            discourse_api_key = Tools.SHA256Hash("deprecated");
            wallet_id = Tools.SHA256Hash(AccountID);
        }

        // EntityMem.EntityType()
        public override string EntityType()
        {
            return AccountEntity.AccountEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return AccountID;
        }
        public bool IsOnline()
        {
            // Presume the account is online if there has been a heartbest in the last minute
            return TimeOfLastHeartbeat != null
                    && (DateTime.UtcNow - TimeOfLastHeartbeat).TotalSeconds < 60;
        }
        // Get the authorization information for a particular token.
        // Returns 'null' if there is no such authorization.
        public AuthTokenInfo GetAuthTokenInfo(string pToken)
        {
            foreach (var authInfo in AuthTokens)
            {
                if (authInfo.Token == pToken)
                {
                    return authInfo;
                }
            }
            return null;
        }
        /// <summary>
        /// Create an access token for this account and add it to the list
        ///     of usable tokens.
        /// </summary>
        /// <param name="pScope">scope of token (usually 'owner')</param>
        /// <param name="pParam">an extra uniquifying string to add to generated token</param>
        /// <returns>The created token</returns>
        public AuthTokenInfo CreateAccessToken(string pScope, string pParam = "")
        {
            // Some quick tokens. Eventually move to JWT tokens.
            TimeSpan tokenExpirationInterval =
                    new TimeSpan(Context.Params.P<int>(AppParams.P_ACCOUNT_AUTHTOKEN_LIFETIME_HOURS), 0, 0);
            int tokenExpirationSeconds = (int)tokenExpirationInterval.TotalSeconds;
            string refreshToken = Tools.SHA256Hash(tokenExpirationSeconds.ToString() + ";" + pParam);
            string accessToken = Tools.SHA256Hash(DateTime.UtcNow.ToString() + ";" + refreshToken);

            AuthTokenInfo authInfo = new AuthTokenInfo(accessToken, refreshToken)
            {
                TokenExpirationTime = DateTime.UtcNow + tokenExpirationInterval,
                Scope = pScope,
                ExtraParam = pParam
            };

            this.AuthTokens.Add(authInfo);
            this.Updated();

            return authInfo;
        }
        /// <summary>
        /// Search this account's access tokens for the one that needs refreshing
        ///     and return a new token to replace it.
        /// Since the refresh tokens are unique to each token, that is used to
        ///     lookup the one to create a replacement for.
        /// </summary>
        /// <param name="pRefreshToken">The refresh authoriation token</param>
        /// <returns>The created token or 'null' if the refreshing could not happen</returns>
        public AuthTokenInfo RefreshAccessToken(string pRefreshToken)
        {
            AuthTokenInfo ret = null;
            try
            {
                AuthTokenInfo refreshable = this.AuthTokens.Where(tok => { return pRefreshToken == tok.RefreshToken; }).First();
                // If one of  the tokens is refreshable, create a new one
                if (refreshable != null)
                {
                    ret = CreateAccessToken(refreshable.Scope, refreshable.ExtraParam);
                }
            }
            catch
            {
                // The .Where().First() throws if there is not a refreshable token
                ret = null;
            }
            return ret;
        }

    }

    public class AuthTokenInfo
    {
        public string Token;
        public string RefreshToken;
        public DateTime TokenCreationTime;
        public DateTime TokenExpirationTime;
        public string Scope;    // "owner" for account access, "domain" for domain access
        public string ExtraParam;

        public AuthTokenInfo()
        {
            TokenCreationTime = DateTime.UtcNow;
            // Default expiration is one day
            TokenExpirationTime = TokenCreationTime + new TimeSpan(24, 0, 0);
        }
        public AuthTokenInfo(string pToken) : this()
        {
            Token = pToken;
        }
        public AuthTokenInfo(string pToken, string pRefreshToken) : this()
        {
            Token = pToken;
            RefreshToken = pRefreshToken;
        }
        public bool HasExpired()
        {
            return TokenExpirationTime < DateTime.UtcNow;
        }
    }

    public class UserImages
    {
        public string Hero;
        public string Thumbnail;
        public string Tiny;
    }
    public class LocationInfo
    {
        // Location information passed in heartbeat
        public bool Connected;
        public string Path;
        public string PlaceID;
        public string NetworkAddress;
        public int NetworkPort;
        public string NodeID;
        public string Availability;
    };
}

