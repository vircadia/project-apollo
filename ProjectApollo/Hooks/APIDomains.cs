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
            RESTReplyData rd = new RESTReplyData();

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;

            DomainReplyData drd = new DomainReplyData();

            PutIceServerResponse isres = new PutIceServerResponse();
            isres.status = "success";
            drd.id = domainID;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainObject dobj))
            {
                drd.ice_server_address = dobj.IPAddr;
                drd.name = dobj.PlaceName;

                isres.domain = drd;
                rd.Status = 200;
                rd.Body = JsonConvert.SerializeObject(isres);
            }
            else
            {
                // return nothing!
                GetDomainError gde = new GetDomainError();
                gde.status = "fail";
                gde.data = new Dictionary<string, string>();
                gde.data.Add("domain", "there is no domain with that ID");
                rd.Status = 404;
                rd.Body = JsonConvert.SerializeObject(gde);
            }

            return rd;
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
        public struct bodyDomainData
        {
            public string description;
            public int capacity;
            public string maturity;
            public string restriction;
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
            public bodyDomainData Domain;
        }

        [APIPath("/api/v1/domains/%", "PUT", true)]
        public RESTReplyData domain_heartbeat(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();

            try
            {
                bodyHeartbeatRequest requestData = pReq.RequestBodyObject<bodyHeartbeatRequest>();
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} domain_heartbeat: Exception parsing body: {1}", _logHeader, e.ToString());
            }

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainObject dobj))
            {
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
            RESTReplyData rd = new RESTReplyData();

            string domainID = pArgs[0];
            DomainReplyData drd = new DomainReplyData();
            PutIceServerRequest isr = JsonConvert.DeserializeObject<PutIceServerRequest>(pReq.RequestBody);
            PutIceServerResponse isres = new PutIceServerResponse();

            if (Context.DomainEntities.SetIP(domainID, pReq.RemoteUser.ToString(), isr.api_key))
            {
                isres.status = "success";
            }
            else
            {
                isres.status = "fail";
            }

            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainObject dobj))
            {
                drd.id = domainID;
                drd.ice_server_address = dobj.IPAddr;
                drd.name = dobj.PlaceName;

                isres.domain = drd;
                rd.Status = 200;
                rd.Body = JsonConvert.SerializeObject(isres);
            }

            return rd;
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
            DomainObject DO = new DomainObject()
            {
                PlaceName = png.GenerateRandomFirstName() + "-" + plng.GenerateRandomPlaceName() + "-" + new Random().Next(500, 9000).ToString(),
                DomainID = Guid.NewGuid().ToString(),
                IPAddr = pReq.RemoteUser.ToString()
            };


            DO.API_Key = Tools.MD5Hash($":{DO.PlaceName}::{DO.DomainID}:{DO.IPAddr}");

            DomainReplyDataWithKey drd = new DomainReplyDataWithKey();
            drd.ice_server_address = DO.IPAddr;
            drd.api_key = DO.API_Key;
            drd.id = DO.DomainID;
            drd.name = DO.PlaceName;

            TemporaryPlaceNameReply tpnr = new TemporaryPlaceNameReply();
            tpnr.status = "success";
            tpnr.data = new Dictionary<string, DomainReplyDataWithKey>();
            tpnr.data.Add("domain", drd);

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(tpnr);

            DomainMemory.MemoryItem mi = new DomainMemory.MemoryItem();
            mi.Obj = DO;
            Context.DomainEntities.AddDomain(DO.DomainID, mi);

            rd.CustomOutputHeaders = new Dictionary<string, string>();
            rd.CustomOutputHeaders.Add("X-Rack-CORS", "miss; no-origin");
            rd.CustomOutputHeaders.Add("Access-Control-Allow-Origin", "*");
            return rd;
        }

        [APIPath("/api/v1/domains/%/public_key", "PUT", true)]

        public RESTReplyData set_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            RESTReplyData replyData = new RESTReplyData(respBody);

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainObject aDomain))
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
                            respBody.Status = "fail";
                        }
                    }
                    else
                    {
                        Context.Log.Error("{0} attempt to set public_key with zero length body", _logHeader);
                        respBody.Status = "fail";
                    }
                    byte[] apiKey = pReq.RequestBodyFile("api_key");
                    if (apiKey != null && apiKey.Length > 0)
                    {

                        if (!aDomain.SetApiKey(pReq.RemoteUser.ToString(), apiKey))
                        {
                            // failure
                            replyData.Status = 403;
                            respBody.Status = "fail";
                        }
                    }
                    else
                    {
                        Context.Log.Error("{0} attempt to set api_key with zero length body", _logHeader);
                        respBody.Status = "fail";
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} exception parsing multi-part http: {1}", _logHeader, e.ToString());
                    respBody.Status = "fail";
                }

            }
            else
            {
                Context.Log.Error("{0} attempt to set public_key for unknown domain {1}", _logHeader, domainID);
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

        // TODO: CHANGE TO REGEX
        [APIPath("/api/v1/domains/%/public_key", "GET", true)]
        public RESTReplyData get_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Context.DomainEntities.TryGetDomainWithID(domainID, out DomainObject aDomain))
            {
                // return domains public_key in "public_key" field of response
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

    }
}
