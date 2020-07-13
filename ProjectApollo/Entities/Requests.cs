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
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Project_Apollo.Configuration;

namespace Project_Apollo.Entities
{
    public class Requests : EntityStorage
    {
        private static readonly string _logHeader = "[Requests]";

        private static readonly object _requestsLock = new object();
        private static Requests _instance;
        public static Requests Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_requestsLock)
                    {
                        // race condition check
                        if (_instance == null)
                        {
                            _instance = new Requests();
                            _instance.Init();
                        }
                    }
                }
                return _instance;
            }
        }

        // List of all known requests
        private readonly Dictionary<string, RequestEntity> ActiveRequests = new Dictionary<string, RequestEntity>();

        public Requests() : base(RequestEntity.RequestEntityTypeName)
        {
            // Start a background task that checks and deleted idle requests
            CreateRequestExpirationBackgroundTask();
        }

        public void Init()
        {
            // Fill my list of requests
            lock (_requestsLock)
            {
                foreach (RequestEntity anEntity in AllStoredEntities<RequestEntity>()) {
                    ActiveRequests.Add(anEntity.RequestID, anEntity);
                }
                Context.Log.Debug("{0} Initialized by reading in {1} RequestEntities",
                            _logHeader, ActiveRequests.Count.ToString());
            }
        }

        /// <summary>
        /// Find and return a RequestEntity based on the RequestID.
        /// </summary>
        /// <param name="pRequestID"></param>
        /// <param name="oRequest">RequestEntity found</param>
        /// <returns></returns>
        public bool TryGetRequestWithID(string pRequestID, out RequestEntity oRequest)
        {
            if (pRequestID != null)
            {
                lock (_requestsLock)
                {
                    return ActiveRequests.TryGetValue(pRequestID, out oRequest);
                }
            }
            oRequest = null;
            return false;
        }

        // Return 'true' if the sender should be throttled from doing the passed op
        public void AddRequest(RequestEntity pRequestEntity)
        {
            lock (_requestsLock)
            {
                // Using RequestID as the key because it is a GUID and known good a filenames.
                // Would be best to use SenderKey as index but that might have a colon in it.
                ActiveRequests.Add(pRequestEntity.RequestID, pRequestEntity);
                pRequestEntity.Updated();
            }
        }
        public void DeleteRequest(RequestEntity pRequest)
        {
            lock (_requestsLock)
            {
                ActiveRequests.Remove(pRequest.RequestID);
                RemoveFromStorage(pRequest);
            }
        }
        public IEnumerable<RequestEntity> Enumerate()
        {
            List<RequestEntity> aRequests;
            lock (_requestsLock)
            {
                aRequests = new List<RequestEntity>(ActiveRequests.Values);
            }
            return aRequests.AsEnumerable();
        }
        public delegate bool TestEntity(RequestEntity pToTest);
        public IEnumerable<RequestEntity> Enumerate(TestEntity pAction)
        {
            List<RequestEntity> aRequests = new List<RequestEntity>();
            lock (_requestsLock)
            {
                foreach (var req in ActiveRequests.Values)
                {
                    if (pAction(req))
                    {
                        aRequests.Add(req);
                    }
                }
            }
            return aRequests;
        }
        public IEnumerable<RequestEntity> Enumerate(string pRequestType)
        {
            return this.Enumerate((req) =>
            {
                return pRequestType == req.RequestType;
            });
        }

        /// <summary>
        /// Go through all the request and delete all the old ones.
        /// </summary>
        public void ExpireRequests()
        {
            // Context.Log.Debug("{0} checking for request expiration", _logHeader);
            List<RequestEntity> toDelete = new List<RequestEntity>();
            lock (_requestsLock)
            {
                long NowTicks = DateTime.UtcNow.Ticks;
                foreach (var kvp in ActiveRequests)
                {
                    if (kvp.Value.ExpirationTime.Ticks > NowTicks)
                    {
                        toDelete.Add(kvp.Value);
                    }
                }
            }
            if (toDelete.Count > 0)
            {
                Context.Log.Debug("{0} Expiring {1} requests", _logHeader, toDelete.Count);
                foreach (var req in toDelete)
                {
                    DeleteRequest(req);
                }
            }
        }
        private void CreateRequestExpirationBackgroundTask()
        {
            Context.Log.Debug("{0} creating background task for request expiration", _logHeader);
            Task.Run(() =>
            {
                try
                {
                    CancellationToken stopToken = Context.KeepRunning.Token;
                    WaitHandle waiter = stopToken.WaitHandle;
                    int sleepMS = Context.Params.P<int>(AppParams.P_REQUEST_EXPIRATION_CHECK_SECONDS) * 1000;
                    while (!Context.KeepRunning.IsCancellationRequested)
                    {
                        Requests.Instance.ExpireRequests();
                        waiter.WaitOne(sleepMS);
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Exception in Request expiration backround job. {1}",
                                        _logHeader, e);
                }
                Context.Log.Debug("{0} Request expiration backround job exiting", _logHeader);
            });
        }
    }

    /// <summary>
    /// Variables and operations on a domain
    /// </summary>
    public class RequestEntity : EntityMem
    {
        public static readonly string RequestEntityTypeName = "Request";

        public string RequestID;    // globally unique request identifier
        public string RequestType;
        public DateTime ExpirationTime;
        public JObject RequestBody;

        // admin stuff
        public DateTime WhenRequestEntryCreated; // What the variable name says

        public RequestEntity() : base(Requests.Instance)
        {
            RequestID = Guid.NewGuid().ToString();
            WhenRequestEntryCreated = DateTime.UtcNow;
            ExpirationTime = DateTime.UtcNow + TimeSpan.FromMinutes(20);
        }

        // EntityMem.EntityType()
        public override string EntityType()
        {
            return RequestEntity.RequestEntityTypeName;
        }
        // EntityMem.StorageName()
        public override string StorageName()
        {
            return RequestID;
        }
    }

    // Base class for the various request types
    // These classes wrap the underlying RequestEntity and provide
    //     the request specific variables.
    public abstract class RequestTypes<T>
    {
        private readonly RequestEntity _entity;
        public T Body;

        public RequestTypes() {
            _entity = new RequestEntity();
        }
        public RequestTypes(RequestEntity pEntity)
        {
            _entity = pEntity;
            Body = _entity.RequestBody.ToObject<T>();
        }
        public virtual void Updated()
        {
            this.ToEntity().Updated();
        }
        public virtual void AddAndUpdate()
        {
            Requests.Instance.AddRequest(this.ToEntity());
            _entity.Updated();
        }
        public virtual RequestEntity ToEntity()
        {
            _entity.RequestBody = JObject.FromObject(Body);
            return _entity;
        }
        public virtual void ExpireIn(TimeSpan pLifetime)
        {
            _entity.ExpirationTime = DateTime.UtcNow + pLifetime;
        }
    }

    // Connection request... I want to connection to another
    // Usually short term and implemented as a handshake in legacy HiFi
    public struct ConnectionSchema {
        public string node_id;              // sessionId/nodeId of requestor
        public string requestor_accountId;  // account Id of requestor
        public string proposed_node_id;     // sessionId/nodeId of other
        public bool node_accepted;          // requestor node accepts the connection
        public bool proposed_node_accepted; // proposed node accepts the connection
    };
    public class RequestConnection : RequestTypes<ConnectionSchema>
    {
        public static readonly string RequestTypeName = "Connection";

        public RequestConnection() : base() { }
        public RequestConnection(RequestEntity pEntity) : base (pEntity) { }

        // Return an IEnumerable of all of the connection requests repackaged as RequestConnection's
        public static IEnumerable<RequestConnection> GetConnectionRequests()
        {
            return Requests.Instance.Enumerate(req =>
            {
                return req.RequestType == RequestConnection.RequestTypeName;
            }).Select(req =>
            {
                return new RequestConnection(req);
            });
        }
        // Return the ConnectionRequests that have the specified node as source or proposed
        public static IEnumerable<RequestConnection> GetNodeRequests(string pNodeId)
        {
            return RequestConnection.GetConnectionRequests().Where( req =>
            {
                return req.Body.node_id == pNodeId
                    || req.Body.proposed_node_id == pNodeId;
            });
        }
    }
}
