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
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Project_Apollo.Configuration;

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

        // The operations one might do in a session the might need throttling
        public enum Op
        {
            TOKEN_CREATE,
            ACCOUNT_CREATE
        }

        // List of all known domains
        private readonly Dictionary<string, SessionEntity> ActiveSessions = new Dictionary<string, SessionEntity>();


        public Sessions() : base(SessionEntity.SessionEntityTypeName)
        {
            // Start a background task that checks and deleted idle sessions
            CreateSessionExpirationBackgroundTask();
        }

        public void Init()
        {
            // Fill my list of domains
            lock (_sessionsLock)
            {
                foreach (SessionEntity anEntity in AllStoredEntities<SessionEntity>()) {
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
                    return ActiveSessions.TryGetValue(pSessionID, out oSession);
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
            oSession = GetSession(pSenderKey);
            return oSession != null;
        }

        /// <summary>
        /// Get the session based on SenderKey.
        /// </summary>
        /// <param name="pSenderKey"></param>
        /// <returns>'null' if no session found</returns>
        public SessionEntity GetSession(string pSenderKey)
        {
            SessionEntity ret = default;
            if (pSenderKey != null)
            {
                lock (_sessionsLock)
                {
                    foreach (var kvp in ActiveSessions)
                    {
                        if (kvp.Value.SenderKey == pSenderKey)
                        {
                            ret = kvp.Value;
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        // Return 'true' if the sender should be throttled from doing the passed op
        public bool ShouldBeThrottled(string pSenderKey, Op pOp)
        {
            bool ret = true;
            if (TryGetSessionWithSenderKey(pSenderKey, out SessionEntity oSession))
            {
                switch (pOp)
                {
                    case Op.ACCOUNT_CREATE:
                        if (++oSession.AccountCreateCount <= Context.Params.P<int>(AppParams.P_SESSION_THROTTLE_ACCOUNT_CREATE))
                        {
                            ret = false;
                        }
                        break;
                    case Op.TOKEN_CREATE:
                        if (++oSession.TokenCreateCount <= Context.Params.P<int>(AppParams.P_SESSION_THROTTLE_TOKEN_CREATE))
                        {
                            ret = false;
                        }
                        break;
                    default:
                        ret = true;
                        break;
                }
            }
            else
            {
                // If there is no session, create one so we can track this creator
                SessionEntity sess = new SessionEntity()
                {
                    SenderKey = pSenderKey,
                    TimeOfLastHeartbeat = DateTime.UtcNow
                };
                AddSession(sess);
                Context.Log.Debug("{0} Creating a throttle session for {1}", _logHeader, pSenderKey);
                ret = false;
            }
            return ret;
        }
        public void ClearThrottleCounters(SessionEntity pSession)
        {
            pSession.AccountCreateCount = 0;
            pSession.TokenCreateCount = 0;
        }
        public void AddSession(SessionEntity pSessionEntity)
        {
            lock (_sessionsLock)
            {
                // Using SessionID as the key because it is a GUID and known good a filenames.
                // Would be best to use SenderKey as index but that might have a colon in it.
                ActiveSessions.Add(pSessionEntity.SessionID, pSessionEntity);
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
                ClearThrottleCounters(ret);
                ret.Updated();
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
        /// <summary>
        /// Go through all the sessions and delete all the old ones.
        /// </summary>
        public void ExpireSessions()
        {
            // Context.Log.Debug("{0} checking for session expiration", _logHeader);
            lock (_sessionsLock)
            {
                List<SessionEntity> toDelete = new List<SessionEntity>();
                int timeToKeepIdleSessions = Context.Params.P<int>(AppParams.P_SESSION_IDLE_EXPIRE_SECONDS);
                foreach (var kvp in ActiveSessions)
                {
                    if ((DateTime.UtcNow - kvp.Value.TimeOfLastHeartbeat).TotalSeconds > timeToKeepIdleSessions)
                    {
                        toDelete.Add(kvp.Value);
                    }
                }
                if (toDelete.Count > 0)
                {
                    Context.Log.Debug("{0} Expiring {1} sessions", _logHeader, toDelete.Count);
                    foreach (var sess in toDelete)
                    {
                        ActiveSessions.Remove(sess.SessionID);
                        RemoveFromStorage(sess);
                    }
                }
            }
        }
        private void CreateSessionExpirationBackgroundTask()
        {
            Context.Log.Debug("{0} creating background task for session expiration", _logHeader);
            Task.Run(() =>
            {
                try
                {
                    CancellationToken stopToken = Context.KeepRunning.Token;
                    WaitHandle waiter = stopToken.WaitHandle;
                    int sleepMS = Context.Params.P<int>(AppParams.P_SESSION_IDLE_CHECK_SECONDS) * 1000;
                    while (!Context.KeepRunning.IsCancellationRequested)
                    {
                        Sessions.Instance.ExpireSessions();
                        waiter.WaitOne(sleepMS);
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Exception in Session expiration backround job. {1}",
                                        _logHeader, e);
                }
                Context.Log.Debug("{0} Session expiration backround job exiting", _logHeader);
            });
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
        public int AccountCreateCount;
        public int TokenCreateCount;

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
