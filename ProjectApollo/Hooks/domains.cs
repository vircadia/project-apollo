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

namespace Project_Apollo.Hooks
{
    public class domains
    {

        public struct PutIceServerRequest
        {
            public string api_key;
            public Dictionary<string, string> domain;
        }
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

            if (Domains.Instance().SetIP(domainID, pReq.RemoteUser.ToString(), isr.api_key))
            {
                isres.status = "success";
            }
            else
            {
                isres.status = "fail";
            }


            if (Domains.Instance().TryGetDomainWithID(domainID, out DomainObject dobj))
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

        public struct GetDomainError
        {
            public string status;
            public Dictionary<string, string> data;
        }
        [APIPath("/api/v1/domains/%", "GET", true)]
        public RESTReplyData get_domain(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData rd = new RESTReplyData();
            string domainID = pArgs[0];
            DomainReplyData drd = new DomainReplyData();
            PutIceServerResponse isres = new PutIceServerResponse();
            isres.status = "success";
            drd.id = domainID;
            if (Domains.Instance().TryGetDomainWithID(domainID, out DomainObject dobj))
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
            Domains.Instance().AddDomain(DO.DomainID, mi);

            rd.CustomOutputHeaders = new Dictionary<string, string>();
            rd.CustomOutputHeaders.Add("X-Rack-CORS", "miss; no-origin");
            rd.CustomOutputHeaders.Add("Access-Control-Allow-Origin", "*");
            return rd;
        }

        [APIPath("/api/v1/domains/%/public_key", "PUT", true)]

        public RESTReplyData set_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            string[] Lines = pReq.RequestBody.Split(new[] { '\n' });

            string Data = "";

            int index = 0;
            foreach (string S in Lines)
            {
                if (index > 3 && S.IndexOf("boundary") == -1)
                {
                    Data += S + "\n";
                }
                index++;
            }

            ResponseBody respBody = new ResponseBody();

            string domainID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Domains.Instance().TryGetDomainWithID(domainID, out DomainObject aDomain))
            {
                if (aDomain.SetPublicKey(pReq.RemoteUser.ToString(), Tools.Base64Encode(Data)))
                {
                    // success
                }
                else
                {
                    // failure
                    // rd.Status = 403;
                    // rp.status = "fail";
                }
            }
            else
            {
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
            if (Domains.Instance().TryGetDomainWithID(domainID, out DomainObject aDomain))
            {
                // return domains public_key in "public_key" field of response
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

        public struct HeartbeatPacket // Put this inside a Dictionary, key being "domain"
        {
            public string automatic_networking;
            public HeartbeatData heartbeat;
        }

        public struct HeartbeatData
        {
            public int num_anon_users;
            public int num_users;
            public string protocol;
            public bool restricted;
            public string version;
            // user_hostnames   -    Currently not known what this data looks like, however it is probably a Dictionary of some sort
        }


        [APIPath("/api/v1/domains/%", "PUT", true)]
        public RESTReplyData domain_heartbeat(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();

            // Check the Authorization header for a valid Access token
            // If token is valid, begin updating stuff
            if (Users.Instance().TryGetUserWithAuth(pReq.AuthToken, out UserObject aUser))
            {
                // Start updating shit
                Dictionary<string, HeartbeatPacket> requestData =
                            JsonConvert.DeserializeObject<Dictionary<string, HeartbeatPacket>>(pReq.RequestBody);

                string domainID = pArgs.Count == 1 ? pArgs[0] : null;
                if (Domains.Instance().TryGetDomainWithID(domainID, out DomainObject dobj))
                {
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
                }

            } else
            {
                replyData.Status = 404; // this will trigger a new temporary domain name
            }
            return replyData;
        }
    }
}
