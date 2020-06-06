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

namespace Project_Apollo.Entities
{
    public class Sessions : EntityStorage
    {
        private static readonly string _logHeader = "[Sessions]";

        private static readonly object sessionLock = new object();
        private static Sessions _instance;
        public static Sessions Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (sessionLock)
                    {
                        // race condition check
                        if (_instance == null)
                        {
                            _instance = new Sessions();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        // List of all known domains
        private readonly Dictionary<string, SessionEntity> ActiveSessions = new Dictionary<string, SessionEntity>();

        public Sessions() : base(SessionEntity.SessionEntityTypeName)
        {
        }

        public void Init()
        {
            // Fill my list of domains
            lock (sessionLock)
            {
                foreach (SessionEntity anEntity in AllEntities<SessionEntity>())
                {
                    ActiveSessions.Add(anEntity.SessionID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} DomainEntities",
                            _logHeader, ActiveSessions.Count.ToString());
            }
        }

        /// <summary>
        /// Find and return a DomainEntity based on the DomainID.
        /// </summary>
        /// <param name="pDomainID"></param>
        /// <param name="oDomain">DomainObject found</param>
        /// <returns></returns>
        public bool TryGetSessionWithSenderKey(string pSenderKey, out SessionEntity oSession)
        {
            if (pSenderKey == null)
            {
                oSession = null;
                return false;
            }
            lock (sessionLock)
            {
                return ActiveSessions.TryGetValue(pSenderKey, out oSession);
            }
        }
        /// <summary>
        /// Find the session that has the passed authorization token.
        /// This finds the session when we are just looking at the auth token
        ///     sent in the http header. The presumption is that auth tokens are
        ///     unique.
        /// </summary>
        /// <param name="pAuthToken"></param>
        /// <param name="oSession">Set to the found SessionEntity</param>
        /// <returns>'true' if the session was found</returns>
        public bool TryGetSessionWithAuth(string pAuthToken, out SessionEntity oSession)
        {
            lock (sessionLock)
            {
                foreach (var kvp in ActiveSessions)
                {
                    if (kvp.Value.AuthToken == pAuthToken)
                    {
                        oSession = kvp.Value;
                        return true;
                    }
                }
            }
            oSession = null;
            return false;
        }

        public void AddSession(string pSenderKey, SessionEntity pSessionEntity)
        {
            lock (sessionLock)
            {
                ActiveSessions.Add(pSenderKey, pSessionEntity);
                pSessionEntity.Updated();
            }
        }
    }

    /// <summary>
    /// The account information.
    /// </summary>
    public class SessionEntity : EntityMem
    {
        public static readonly string SessionEntityTypeName = "Session";

        public string SessionID;                // globally unique session identifier
        public string SenderKey;                // an identifier for the sender (usually net addr)
        public string AccountID;                // ID of account associated with this session
        public string AuthToken;                // token for access by this user for this session
        public string DomainID;                 // domain associated with this session

        public LocationInfo Location;           // Where the user says they are

        // admin stuff
        public string IPAddrOfCreator;          // IP address that created this session
        public DateTime WhenSessionCreated;     // What the variable name says
        public DateTime TimeOfLastHeartbeat;    // last time we had activity on this session

        public SessionEntity() : base(Sessions.Instance)
        {
            SessionID = new Guid().ToString();
            WhenSessionCreated = DateTime.UtcNow;
        }

        // EntityMem.EntityType()
        public override string EntityType()
        {
            return DomainEntity.DomainEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return SessionID;
        }
    }

}

