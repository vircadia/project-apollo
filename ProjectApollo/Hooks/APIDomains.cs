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

using Project_Apollo.Entities;
using Project_Apollo.Registry;

using Newtonsoft.Json;
using RandomNameGeneratorLibrary;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Globalization;

namespace Project_Apollo.Hooks
{
    public class APIDomains
    {
        public static readonly string _logHeader = "[APIDomains]";

        // ===GET /api/v1/domains/% =================================================

        // The 'domain' response body can contain different items depending
        //    on the request. If it's not set, don't retun that field.
        public struct bodyDomainReplyData
        {
            [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
            public string id;
            [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
            public string name;
            [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
            public string ice_server_address;
            [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
            public string api_key;
        }
        public struct bodyDomainResponse
        {
            public bodyDomainReplyData domain;
        }
        [APIPath("/api/v1/domains/%", "GET", true)]
        public RESTReplyData get_domain(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Domains.Instance.TryGetDomainWithID(domainID, out DomainEntity dobj))
            {
                respBody.Data = new bodyDomainResponse()
                {
                    domain = new bodyDomainReplyData()
                    {
                        id = domainID,
                        ice_server_address = dobj.GetIceServerAddr(),
                        name = dobj.PlaceName
                    }
                };
                replyData.Body = respBody;  // serializes JSON
                // Context.Log.Debug("{0} get_domain GET: domain{1}, returning: {2}",
                //                     _logHeader, domainID, replyData.Body);
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
                // Context.Log.Debug("{0} get_domain GET: no domain! Returning: {1}", _logHeader, replyData.Body);
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
            public bodyDomainPutData domain;
        }

        [APIPath("/api/v1/domains/%", "PUT", true)]
        public RESTReplyData domain_heartbeat(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            // ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Domains.Instance.TryGetDomainWithID(domainID, out DomainEntity dobj))
            {
                try
                {
                    // Context.Log.Debug("{0} domain_heartbest PUT: Domain {1}, received: {2}",
                    //                     _logHeader, domainID, pReq.RequestBody);
                    bodyHeartbeatRequest requestData = pReq.RequestBodyObject<bodyHeartbeatRequest>();

                    // If there is no api_key, things aren't initialized
                    if (String.IsNullOrEmpty(dobj.API_Key))
                    {
                        replyData.Status = 401;
                    }
                    else
                    {
                        dobj.NetworkingMode = requestData.domain.automatic_networking;
                        bodyHeartbeatData dat = requestData.domain.heartbeat;
                        dobj.Protocol = dat.protocol;
                        dobj.Restricted = dat.restricted;
                        dobj.Version = dat.version;
                        dobj.LoggedIn = dat.num_users;
                        dobj.Anon = dat.num_anon_users;
                        dobj.TotalUsers = dat.num_anon_users + dat.num_users;

                        dobj.TimeOfLastHeartbeat = DateTime.UtcNow;
                        dobj.Updated();
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} domain_heartbeat: Exception parsing body: {1}", _logHeader, e.ToString());
                }
            }
            else
            {
                Context.Log.Error("{0} domain_heartbeat: unknown domain. Returning 404", _logHeader);
                replyData.Status = 404; // this will trigger a new temporary domain name
            }

            // replyData.Body = respBody; there is no body in the response
            return replyData;
        }
        // == PUT /api/v1/domains/%/ice_server_address ===========================
        public struct bodyIceServerPutData
        {
            public string ice_server_address;
            public string api_key;
        }
        public struct bodyIceServerPut
        {
            public bodyIceServerPutData domain;
        }
        public struct bodyIceServerPutResponse
        {
            public string id;           // returning the domainID
            public string ice_server_address;   // returning the address set
            public string name;         // place name
        }

        [APIPath("/api/v1/domains/%/ice_server_address", "PUT", true)]
        public RESTReplyData put_ice_address(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Domains.Instance.TryGetDomainWithID(domainID, out DomainEntity aDomain))
            {
                // Context.Log.Debug("{0} domains/ice_server_addr PUT. Body={1}", _logHeader, pReq.RequestBody);
                bodyIceServerPut isr = pReq.RequestBodyObject<bodyIceServerPut>();
                string includeAPIKey = isr.domain.api_key;
                if (String.IsNullOrEmpty(aDomain.API_Key) || includeAPIKey == aDomain.API_Key)
                {
                    aDomain.IceServerAddr = isr.domain.ice_server_address;
                }
                else
                {
                    respBody.RespondFailure();
                    replyData.Status = 401; // not authorized
                }
            }
            else
            {
                respBody.RespondFailure();
                replyData.Status = 404;
            }
            return replyData;
        }

        // == POST /api/v1/domains/temporary =====================================
        [APIPath("/api/v1/domains/temporary", "POST", true)]
        public RESTReplyData get_temporary_name(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();
            ResponseBody respBody = new ResponseBody();

            PersonNameGenerator png = new PersonNameGenerator();
            PlaceNameGenerator plng = new PlaceNameGenerator();

            DomainEntity newDomain = new DomainEntity()
            {
                PlaceName = png.GenerateRandomFirstName() + "-" + plng.GenerateRandomPlaceName() + "-" + new Random().Next(500, 9000).ToString(),
                DomainID = Guid.NewGuid().ToString(),
                IPAddrOfFirstContact = pReq.RemoteUser.ToString()
            };
            newDomain.API_Key = Tools.MD5Hash($":{newDomain.PlaceName}::{newDomain.DomainID}:{newDomain.IceServerAddr}");
            Domains.Instance.AddDomain(newDomain.DomainID, newDomain);

            respBody.Data = new bodyDomainResponse()
            {
                domain = new bodyDomainReplyData()
                {
                    id = newDomain.DomainID,
                    name = newDomain.PlaceName,
                    api_key = newDomain.API_Key
                }
            };
            replyData.Body = respBody;  // serializes JSON
            Context.Log.Debug("{0} Returning temporary domain: id={1}, name={2}",
                        _logHeader, newDomain.DomainID, newDomain.PlaceName);
            
            replyData.CustomOutputHeaders.Add("X-Rack-CORS", "miss; no-origin");
            replyData.CustomOutputHeaders.Add("Access-Control-Allow-Origin", "*");
            return replyData;
        }

        // == PUT /api/v1/domains/%/public_key ===================================
        [APIPath("/api/v1/domains/%/public_key", "PUT", true)]

        public RESTReplyData set_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Domains.Instance.TryGetDomainWithID(domainID, out DomainEntity aDomain))
            {
                try
                {
                    string includedAPIKey = pReq.RequestBodyMultipart("api_key");

                    // If this is a temp domain, the supplied API key must match
                    // TODO: is there another Authorization later (ie, maybe user auth?)
                    if (String.IsNullOrEmpty(aDomain.API_Key) || aDomain.API_Key == includedAPIKey)
                    {
                        // The PUT sends the key as binary but it is later sent around
                        //    and processed as a Base64 string.
                        Stream byteStream = pReq.RequestBodyMultipartStream("public_key");
                        if (byteStream != null)
                        {
                            using var memStream = new MemoryStream();
                            byteStream.CopyTo(memStream);
                            aDomain.Public_Key= Convert.ToBase64String(memStream.ToArray());
                            aDomain.Updated();
                            // Context.Log.Debug("{0} successful set of public_key for {1}", _logHeader, domainID);
                        }
                        else
                        {
                            Context.Log.Error("{0} could not extract public key from request body: domain {1}",
                                                _logHeader, domainID);
                            replyData.Status = 401; // not authorized
                            respBody.RespondFailure();
                        }
                    }
                    else
                    {
                        Context.Log.Error("{0} attempt to set public_key with non-matching APIKeys: domain {1}",
                                            _logHeader, domainID);
                        replyData.Status = 401; // not authorized
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

        // == GET /api/v1/domains/%/public_key ===================================
        public struct bodyDomainPublicKeyGetResponse
        {
            public string public_key;
        }
        [APIPath("/api/v1/domains/%/public_key", "GET", true)]
        public RESTReplyData get_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Domains.Instance.TryGetDomainWithID(domainID, out DomainEntity aDomain))
            {
                respBody.Data = new bodyDomainPublicKeyGetResponse()
                {
                    public_key = aDomain.Public_Key
                };
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
                // Context.Log.Debug("{0} get_domain GET: no domain! Returning: {1}", _logHeader, replyData.Body);
            }

            replyData.Body = respBody;  // serializes JSON
            return replyData;
        }
    }
}
