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

using Project_Apollo.Entities;
using Project_Apollo.Registry;

using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Newtonsoft.Json.Linq;

namespace Project_Apollo.Hooks
{
    public class APIDomains
    {
        public static readonly string _logHeader = "[APIDomains]";

        // ===GET /api/v1/domains/% =================================================

        // the following structure defns don't seem right for this request
        public struct DomainReplyData
        {
            public string id;
            public string name;
            public string ice_server_address;
        }
        public struct DomainReplyDataWithKey
        {
            public string id;
            public string name;
            public string ice_server_address;
            public string api_key;
        }
        public struct GetDomainError
        {
            public string status;
            public Dictionary<string, string> data;
        }
        [APIPath("/api/v1/domains/%", "GET", true)]
        public RESTReplyData get_domain(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainEntity dobj))
            {
                PutIceServerResponse isres = new PutIceServerResponse();
                isres.status = "success";

                DomainReplyData dReplyData = new DomainReplyData();
                dReplyData.id = domainID;
                dReplyData.ice_server_address = dobj.IPAddr;
                dReplyData.name = dobj.PlaceName;

                isres.domain = dReplyData;

                respBody.Data = isres;
                replyData.Body = respBody;
            }
            else
            {
                // return nothing!
                respBody.RespondFailure();
                respBody.Data = new Dictionary<string, string>()
                {
                    {  "domain", "there is no domain with that ID" }
                };
                replyData.Status = 404;
                replyData.Body = respBody;
            }

            return replyData;
        }

        // ===PUT /api/v1/domains/% ==========================================
        public struct bodyHeartbeatData
        {
            public int num_anon_users;
            public int num_users;
            public string protocol;
            public bool restricted;
            public string version;
            // user_hostnames   -    Currently not known what this data looks like,
            //            however it is probably a Dictionary of some sort
        }
        public struct bodyDomainPutData
        {
            public string description;
            public int capacity;
            public string maturity;
            public string restriction;
            public string api_key;
            public string[] tags;
            public string[] hosts;

            public string automatic_networking;
            public string protocol;
            public bool restricted;
            public string version;

            public bodyHeartbeatData heartbeat;
        }
        // Domains call has differing bodies but they all start with a 'Domain' top level
        public struct bodyHeartbeatRequest
        {
            public bodyDomainPutData Domain;
        }

        [APIPath("/api/v1/domains/%", "PUT", true)]
        public RESTReplyData domain_heartbeat(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainEntity dobj))
            {
                try
                {
                    bodyHeartbeatRequest requestData = pReq.RequestBodyObject<bodyHeartbeatRequest>();
                }
                catch (Exception e)
                {
                    respBody.RespondFailure();
                    Context.Log.Error("{0} domain_heartbeat: Exception parsing body: {1}", _logHeader, e.ToString());
                }
            /*
                JObject requestData = pReq.RequestBodyJSON();
                // Dictionary<string, HeartbeatPacket> requestData =
                //                 JsonConvert.DeserializeObject<Dictionary<string, HeartbeatPacket>>(pReq.RequestBody);

                // First check that there is a API Key
                if (dobj.API_Key == "" || dobj.API_Key == null)
                {
                    replyData.Status = 401;
                }
                else
                {
                    dobj.NetworkingMode = requestData["domain"].automatic_networking;
                    HeartbeatData dat = requestData["domain"].heartbeat;
                    dobj.Protocol = dat.protocol;
                    dobj.Restricted = dat.restricted;
                    dobj.Version = dat.version;
                    dobj.LoggedIn = dat.num_users;
                    dobj.Anon = dat.num_anon_users;
                    dobj.TotalUsers = dat.num_anon_users + dat.num_users;

                    // Update domain info
                    // Session.Instance.DomainsMem.Itms[pArgs[0]] = mi;

                    // construct reply
                }
            */
            }
            else
            {
                Context.Log.Debug("{0} domain_heartbeat: unknown domain. Returning 404", _logHeader);
                replyData.Status = 404; // this will trigger a new temporary domain name
            }

            replyData.Body = respBody;
            return replyData;
        }
        // =======================================================================
        public struct PutIceServerRequest
        {
            public string api_key;
            public Dictionary<string, string> domain;
        }
        public struct PutIceServerResponse
        {
            public string status;
            public DomainReplyData domain;
        }

        [APIPath("/api/v1/domains/%/ice_server_address", "PUT", true)]
        public RESTReplyData put_ice_address(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainEntity dobj))
            {
                PutIceServerRequest isr = JsonConvert.DeserializeObject<PutIceServerRequest>(pReq.RequestBody);

                if (Context.DomainEntities.SetIP(domainID, pReq.RemoteUser.ToString(), isr.api_key))
                {
                    DomainReplyData drd = new DomainReplyData();

                    PutIceServerResponse isres = new PutIceServerResponse();

                    isres.status = "success";
                    drd.id = domainID;
                    drd.ice_server_address = dobj.IPAddr;
                    drd.name = dobj.PlaceName;

                    isres.domain = drd;
                    respBody.Data = isres;
                }
            }

            replyData.Body = respBody;
            return replyData;
        }

        public struct TemporaryPlaceNameReply
        {
            public string status;
            public Dictionary<string, DomainReplyDataWithKey> data;
        }

        [APIPath("/api/v1/domains/temporary", "POST", true)]
        public RESTReplyData get_temporary_name(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData rd = new RESTReplyData();

            PersonNameGenerator png = new PersonNameGenerator();
            PlaceNameGenerator plng = new PlaceNameGenerator();
            // We're generating the entire domain entry in the data store
            DomainEntity newDomain = new DomainEntity()
            {
                PlaceName = png.GenerateRandomFirstName() + "-" + plng.GenerateRandomPlaceName() + "-" + new Random().Next(500, 9000).ToString(),
                DomainID = Guid.NewGuid().ToString(),
                IPAddr = pReq.RemoteUser.ToString()
            };


            newDomain.API_Key = Tools.MD5Hash($":{newDomain.PlaceName}::{newDomain.DomainID}:{newDomain.IPAddr}");

            DomainReplyDataWithKey drd = new DomainReplyDataWithKey();
            drd.ice_server_address = newDomain.IPAddr;
            drd.api_key = newDomain.API_Key;
            drd.id = newDomain.DomainID;
            drd.name = newDomain.PlaceName;

            TemporaryPlaceNameReply tpnr = new TemporaryPlaceNameReply();
            tpnr.status = "success";
            tpnr.data = new Dictionary<string, DomainReplyDataWithKey>();
            tpnr.data.Add("domain", drd);

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(tpnr);

            DomainMemory.MemoryItem mi = new DomainMemory.MemoryItem();
            mi.Obj = newDomain;
            Context.DomainEntities.AddDomain(newDomain.DomainID, newDomain);

            rd.CustomOutputHeaders = new Dictionary<string, string>();
            rd.CustomOutputHeaders.Add("X-Rack-CORS", "miss; no-origin");
            rd.CustomOutputHeaders.Add("Access-Control-Allow-Origin", "*");
            return rd;
        }

        [APIPath("/api/v1/domains/%/public_key", "PUT", true)]

        public RESTReplyData set_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainEntity aDomain))
            {
                try
                {
                    byte[] publicKey = pReq.RequestBodyFile("public_key");
                    if (publicKey != null && publicKey.Length > 0)
                    {
                        if (!aDomain.SetPublicKey(pReq.RemoteUser.ToString(), publicKey))
                        {
                            // failure
                            replyData.Status = 403;
                            respBody.RespondFailure();
                        }
                    }
                    else
                    {
                        Context.Log.Error("{0} attempt to set public_key with zero length body", _logHeader);
                        respBody.RespondFailure();
                    }
                    byte[] apiKey = pReq.RequestBodyFile("api_key");
                    if (apiKey != null && apiKey.Length > 0)
                    {

                        if (!aDomain.SetApiKey(pReq.RemoteUser.ToString(), apiKey))
                        {
                            // failure
                            replyData.Status = 403;
                            respBody.RespondFailure();
                        }
                    }
                    else
                    {
                        Context.Log.Error("{0} attempt to set api_key with zero length body", _logHeader);
                        respBody.RespondFailure();
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} exception parsing multi-part http: {1}", _logHeader, e.ToString());
                    respBody.RespondFailure();
                }

            }
            else
            {
                Context.Log.Error("{0} attempt to set public_key for unknown domain {1}", _logHeader, domainID);
                respBody.RespondFailure();
            }

            replyData.Body = respBody;
            return replyData;
        }

        // TODO: CHANGE TO REGEX
        [APIPath("/api/v1/domains/%/public_key", "GET", true)]
        public RESTReplyData get_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainEntity aDomain))
            {
                // return domains public_key in "public_key" field of response
            }
            else
            {
                respBody.RespondFailure();
            }
            return new RESTReplyData(respBody);
        }

    }
}
