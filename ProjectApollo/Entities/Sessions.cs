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

namespace Project_Apollo.Entities
{
    public class Sessions : EntityStorage
    {
        private static readonly string _logHeader = "[Sessions]";

        private static readonly object _sessionsLock = new object();
        private static Sessions _instance;
        public static Sessions Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sessionsLock)
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
            lock (_sessionsLock)
            {
                foreach (SessionEntity anEntity in AllEntities<SessionEntity>()) {
                    ActiveSessions.Add(anEntity.SessionID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} SessionEntities",
                            _logHeader, ActiveSessions.Count.ToString());
            }
        }

        /// <summary>
        /// Find and return a SessionEntity based on the SessionID.
        /// </summary>
        /// <param name="pSessionID"></param>
        /// <param name="oSession">SessionEntity found</param>
        /// <returns></returns>
        public bool TryGetSessionWithID(string pSessionID, out SessionEntity oSession)
        {
            if (pSessionID != null)
            {
                lock (_sessionsLock)
                {
                    foreach (var kvp in ActiveSessions)
                    {
                        if (kvp.Value.SenderKey == pSessionID)
                        {
                            oSession = kvp.Value;
                            return true;
                        }
                    }
                }
            }
            oSession = null;
            return false;
        }

        /// <summary>
        /// Find and return a SessionEntity based on a SenderKey.
        /// </summary>
        /// <param name="pSenderKey"></param>
        /// <param name="oSession">SessionEntity found</param>
        /// <returns></returns>
        public bool TryGetSessionWithSenderKey(string pSenderKey, out SessionEntity oSession)
        {
            if (pSenderKey != null)
            {
                lock (_sessionsLock)
                {
                    return ActiveSessions.TryGetValue(pSenderKey, out oSession);
                }
            }
            oSession = null;
            return false;
        }

        public void AddSession(SessionEntity pSessionEntity)
        {
            lock (_sessionsLock)
            {
                ActiveSessions.Add(pSessionEntity.SenderKey, pSessionEntity);
                pSessionEntity.Updated();
            }
        }

        /// <summary>
        /// Update the session for this sender.
        /// If a session does not exist, one is created.
        /// </summary>
        /// <param name="pSenderKey"></param>
        /// <returns></returns>
        public SessionEntity UpdateSession(string pSenderKey)
        {
            SessionEntity ret = null;
            if (TryGetSessionWithSenderKey(pSenderKey, out ret))
            {
                ret.TimeOfLastHeartbeat = DateTime.UtcNow;
            }
            else
            {
                // There is no session for this sender so create one
                SessionEntity sess = new SessionEntity()
                {
                    SenderKey = pSenderKey,
                    TimeOfLastHeartbeat = DateTime.UtcNow
                };
                AddSession(sess);
                Context.Log.Debug("{0} Creating new session for {1}", _logHeader, pSenderKey);
                ret = sess;
            }
            return ret;
        }
    }

    /// <summary>
    /// Variables and operations on a domain
    /// </summary>
    public class SessionEntity : EntityMem
    {
        public static readonly string SessionEntityTypeName = "Session";

        public string SessionID;    // globally unique session identifier
        public string SenderKey;    // the source if this session

        // admin stuff
        public DateTime WhenSessionEntryCreated; // What the variable name says
        public DateTime TimeOfLastHeartbeat;    // time of last heartbeat 

        public SessionEntity() : base(Sessions.Instance)
        {
            SessionID = Guid.NewGuid().ToString();
            WhenSessionEntryCreated = DateTime.UtcNow;
        }

        // EntityMem.EntityType()
        public override string EntityType()
        {
            return SessionEntity.SessionEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return SessionID;
        }
    }
}
