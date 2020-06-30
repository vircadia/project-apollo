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
using System.Net;
using System.IO;
using System.Web;

using Project_Apollo.Entities;
using Project_Apollo.Registry;

using Newtonsoft.Json.Linq;
using System.Linq;

namespace Project_Apollo.Hooks
{
    public class APIUsers
    {
        private static readonly string _logHeader = "[User]";

        // == PUT /api/v1/user/heartbeat ======================================================e
        // The heartbeat depends on whether logged in or not.
        // If not logged in, it just pings the server
        // If logged in, sends location data if it has changed
        public struct bodyUserHeartbeatLocation
        {
            // whole section is optional and sent if changed
            public bodyHeartbeatLocationInfo location;
        }
        public struct bodyHeartbeatLocationInfo {
            public bool connected;      // 'true' if discoverable and domain.connected
            public string path;         // XYZ/XYZW
            public string place_id;     // UUID of root place
            public string domain_id;    // UUID of domain
            public string network_address;  // network address of domain
            public string network_port; // domain port
            public string node_id;      // sessionUUID
            public string availability;   // Discoverabiliy mode
        };
        public struct bodyUserHeartbeatResponse {
            public string session_id;
        };
        [APIPath("/api/v1/user/heartbeat", "PUT", true)]
        public RESTReplyData user_heartbeat(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            SessionEntity aSession = Sessions.Instance.UpdateSession(pReq.SenderKey);

            if (!String.IsNullOrEmpty(pReq.AuthToken))
            {
                // The user thinks they are logged in, update location info
                if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
                {
                    aAccount.TimeOfLastHeartbeat = DateTime.UtcNow;
                    GetAccountLocationIfSpecified(aAccount, pReq);

                    respBody.Data = new bodyUserHeartbeatResponse()
                    {
                        session_id = aSession?.SessionID
                    };
                }
                else
                {
                    // Who is this clown?
                    replyData.Status = (int)HttpStatusCode.Unauthorized;
                    respBody.RespondFailure();
                    Context.Log.Error("{0} Heartbeat from account with non-matching token. Sender={1}",
                                        _logHeader, pReq.SenderKey);
                }
            }
            else
            {
                // A new heartbeat from a new place.
                // Is this a non-logged in user?
                Context.Log.Info("{0} Heartbeat from unknown sender {1}", _logHeader, pReq.SenderKey);
                respBody.Data = new bodyUserHeartbeatResponse()
                {
                    session_id = aSession?.SessionID
                };
            }

            replyData.Body = respBody;
            return replyData;
        }
        // == GET /api/v1/users ======================================================e
        //          ?filter=connections|friends
        //          ?status=online
        //          ?per_page=N
        //          ?page=N
        //          ?search=specificName
        // TODO: is authentication required?
        public struct bodyUsersReply
        {
            public bodyUser[] users;
        }
        public struct bodyUser
        {
            public string username;
            public string connection;
            public UserImages images;
            // the code seems to allow "location", "place", or "domain"
            public bodyLocationInfo location;
        }
        public struct bodyLocationInfo
        {
            public string node_id;      // sessionID
            public bodyNamedLoc root;
            public string path;
            public bool online;
        }
        public struct bodyNamedLoc
        {
            public string name;
            public bodyDomainInfo domain;
        }
        public struct bodyDomainInfo
        {
            public string id;
            public string name;
            public string default_place_name;
            public string network_address;
            public string network_port;
            public string ice_server_address;
        }
        [APIPath("/api/v1/users", "GET", true)]
        public RESTReplyData user_get(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            SessionEntity aSession = Sessions.Instance.GetSession(pReq.SenderKey);

            PaginationInfo pagination = new PaginationInfo(pReq);

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                AccountFilterInfo acctFilter = new AccountFilterInfo(pReq);
                AccountScopeFilter scopeFilter = new AccountScopeFilter(pReq, aAccount);

                respBody.Data = new bodyUsersReply() {
                    users = pagination.Filter<AccountEntity>(scopeFilter.Filter(acctFilter.Filter(aAccount))).Select(acct =>
                    {
                        bodyUser ret = new bodyUser()
                        {
                            username = acct.Username,
                            images = acct.Images,
                        };
                        ret.location = BuildLocationInfo(acct);
                        return ret;
                    }).ToArray()
                };
            }

            // Pagination fills the top level of the reply with paging info
            pagination.AddReplyFields(respBody);

            replyData.Body = respBody;  // serializes JSON
            return replyData;
        }

        // == POST /api/v1/users ======================================================e
        public struct bodyUsersPost
        {
            public Dictionary<string,string> user;
        };
        // TODO: What authorization is needed for this? DDOS attack possibility?
        [APIPath("/api/v1/users", "POST", true)]
        public RESTReplyData user_create(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            if (Sessions.Instance.ShouldBeThrottled(pReq.SenderKey, Sessions.Op.ACCOUNT_CREATE))
            {
                respBody.RespondFailure();
                respBody.ErrorData("operation", "throttled");
            }
            else
            {
                try
                {
                    bodyUsersPost requestData = pReq.RequestBodyObject<bodyUsersPost>();

                    string userName = requestData.user["username"];
                    string userPassword = requestData.user["password"];
                    string userEmail = requestData.user["email"];

                    Context.Log.Debug("{0} Creating account {1}/{2}", _logHeader, userName, userEmail);

                    AccountEntity newAcct = Accounts.Instance.CreateAccount(userName, userPassword, userEmail);
                    if (newAcct == null)
                    {
                        respBody.RespondFailure();
                        respBody.ErrorData("username", "already exists");
                        Context.Log.Debug("{0} Failed acct creation. Username already exists. User={1}",
                                        _logHeader, userName);
                    }
                    else
                    {
                        // Successful account creation
                        newAcct.IPAddrOfCreator = pReq.SenderKey;
                        newAcct.Updated();
                        Context.Log.Debug("{0} Successful acct creation. User={1}", _logHeader, userName);
                    }
                }
                catch (Exception e)
                {
                    replyData.Status = (int)HttpStatusCode.BadRequest;
                    Context.Log.Error("{0} Badly formed create account request. {1}", _logHeader, e);
                }
            }

            replyData.Body = respBody;
            return replyData;

        }

        // = POST /api/v1/user/locker ==================================================
        [APIPath("/api/v1/user/locker", "POST", true)]
        public RESTReplyData user_locker_set(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                aAccount.AccountSettings = pReq.RequestBody;
                aAccount.Updated();
            }
            else
            {
                Context.Log.Error("{0} POST user/locker requested without auth token. Token={1}", _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }

        // = GET /api/v1/user/locker ==================================================
        [APIPath("/api/v1/user/locker", "GET", true)]
        public RESTReplyData user_locker_get(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                respBody.Data = aAccount.AccountSettings;
            }
            else
            {
                Context.Log.Error("{0} GET user/locker requested without auth token. Token={1}", _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }

        // = GET /api/v1/user/friends ==================================================
        public struct bodyUserFriendsReply
        {
            public List<string> friends;
        };
        [APIPath("/api/v1/user/friends", "GET", true)]
        public RESTReplyData user_friends_get(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                respBody.Data = new bodyUserFriendsReply()
                {
                    friends = aAccount.Friends
                };
            }
            else
            {
                Context.Log.Error("{0} GET user/friends requested without auth token. Token={1}",
                                        _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }
        // = POST /api/v1/user/friends ==================================================
        public struct bodyUserFriendsPost
        {
            public string username;
        }
        [APIPath("/api/v1/user/friends", "POST", true)]
        public RESTReplyData user_friends_post(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                bodyUserFriendsPost body = pReq.RequestBodyObject<bodyUserFriendsPost>();
                string friendname = body.username;
                Context.Log.Debug("{0} user_friends_post: adding friend {1} for user {2}",
                                _logHeader, body.username, aAccount.Username);
                if (Accounts.Instance.TryGetAccountWithUsername(friendname, out AccountEntity _))
                {
                    if (!aAccount.Friends.Contains(friendname))
                    {
                        aAccount.Friends.Add(friendname);
                    }
                }
                else
                {
                    Context.Log.Error("{0} user_friends_post: attempt to add friend that does not exist. Requestor={1}, friend={2}",
                                                _logHeader, aAccount.Username, friendname);
                    respBody.RespondFailure();
                }
            }
            else
            {
                Context.Log.Error("{0} GET user/friends requested without auth token. Token={1}",
                                        _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }
        // = DELETE /api/v1/user/friends/% ==================================================
        [APIPath("/api/v1/user/friends/%", "DELETE", true)]
        public RESTReplyData user_friends_delete(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            string friendname = pArgs.Count == 1 ? pArgs[0] : null;
            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                Context.Log.Debug("{0} user_friends_delete: removing friend {1} for user {2}",
                                _logHeader, friendname, aAccount.Username);
                aAccount.Friends.Remove(friendname);
            }
            else
            {
                Context.Log.Error("{0} DELETE user/friends requested without auth token. Token={1}",
                                        _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }

        // = POST /api/v1/user/connection_request ==================================================
        public struct bodyUserConnectionRequestPost
        {
            string node_id;
            string proposed_node_id;
        }
        [APIPath("/api/v1/user/connection_request", "POST", true)]
        public RESTReplyData user_connections_request_post(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // Not implemented
                respBody.RespondFailure();
                Context.Log.Debug("{0} user/connection_request: user={1}, body={2}",
                                _logHeader, aAccount.Username, pReq.RequestBody);
            }
            else
            {
                Context.Log.Error("{0} POST user/connection_request for unauthorized user. Token={1}",
                                        _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }
        // = POST /api/v1/user/connections ==================================================
        public struct bodyUserConnectionsPost
        {
            public string username;
        }
        [APIPath("/api/v1/user/connections", "POST", true)]
        public RESTReplyData user_connections_post(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // TODO: Should put something
                bodyUserConnectionsPost body = pReq.RequestBodyObject<bodyUserConnectionsPost>();
                string connectionname = body.username;
                Context.Log.Debug("{0} user_connections_post: adding connection {1} for user {2}",
                                _logHeader, body.username, aAccount.Username);
                if (Accounts.Instance.TryGetAccountWithUsername(connectionname, out AccountEntity _))
                {
                    if (!aAccount.Connections.Contains(connectionname))
                    {
                        aAccount.Connections.Add(connectionname);
                    }
                }
                else
                {
                    Context.Log.Error("{0} user_friends_post: attempt to add friend that does not exist. Requestor={1}, friend={2}",
                                                _logHeader, aAccount.Username, connectionname);
                    respBody.RespondFailure();
                }
            }
            else
            {
                Context.Log.Error("{0} GET user/connections requested without auth token. Token={1}",
                                        _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }
        // = DELETE /api/v1/user/connections/% ==================================================
        [APIPath("/api/v1/user/connections/%", "DELETE", true)]
        public RESTReplyData user_connections_delete(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            string connectionname = pArgs.Count == 1 ? pArgs[0] : null;
            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                Context.Log.Debug("{0} user_connections_delete: removing connection {1} for user {2}",
                                _logHeader, connectionname, aAccount.Username);
                aAccount.Friends.Remove(connectionname);
            }
            else
            {
                Context.Log.Error("{0} DELETE user/connections requested without auth token. Token={1}",
                                        _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }

        // == PUT /api/v1/user/location ================================================
        [APIPath("/api/v1/user/location", "PUT", true)]
        public RESTReplyData user_location_set(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                try
                {
                    GetAccountLocationIfSpecified(aAccount, pReq);
                }
                catch
                {
                    Context.Log.Error("{0} PUT user/location Failed body parsing. Acct={1}",
                                        _logHeader, aAccount.AccountID);
                    respBody.RespondFailure();
                }
            }
            else
            {
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }
        private void GetAccountLocationIfSpecified(AccountEntity pAccount, RESTRequestData pReq) {
            try
            {
                // The 'location' information is sent only when it changes
                JObject requestBodyJSON = pReq.RequestBodyJSON();
                if (requestBodyJSON.ContainsKey("location"))
                {
                    bodyUserHeartbeatLocation requestData = pReq.RequestBodyObject<bodyUserHeartbeatLocation>();

                    bodyHeartbeatLocationInfo loc = requestData.location;
                    pAccount.Location = new LocationInfo()
                    {
                        Connected = loc.connected,
                        Path = loc.path,
                        PlaceID = loc.place_id,
                        DomainID = loc.domain_id,
                        NetworkAddress = loc.network_address,
                        NetworkPort = Int32.Parse(loc.network_port ?? "9400"),
                        NodeID = loc.node_id,
                        Availability = Enum.Parse<Discoverability>(loc.availability ?? "none", false)
                    };
                    pAccount.Updated();
                }
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} GetAccountLocationIfSpecified: Exception parsing body of message. AccountID={1}. {2}",
                                _logHeader, pAccount.AccountID, e);
                Context.Log.Error("{0} GetAccountLocationIfSpecified:     Body = {1}", _logHeader, pReq.RequestBody);
            }
        }


        // == GET /api/v1/users/%/location ==================================================
        [APIPath("/api/v1/users/%/location", "GET", true)]
        public RESTReplyData get_location(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // aAccount = the requesting account
                // TODO: lookup the specified user and see if requestor can see their location
                string otherAccountName = pArgs.Count == 1 ? pArgs[0] : null;
                if (!String.IsNullOrEmpty(otherAccountName))
                {
                    // Convert the %20's to spaces, etc
                    otherAccountName = HttpUtility.UrlDecode(otherAccountName);
                    if (Accounts.Instance.TryGetAccountWithUsername(otherAccountName, out AccountEntity otherAccount))
                    {
                        // Using only one field in 'bodyUser', others will be null and thus not sent
                        respBody.Data = new bodyUser()
                        {
                            location = BuildLocationInfo(aAccount)
                        };
                    }
                    else
                    {
                        respBody.RespondFailure();
                    }
                }
                else
                {
                    respBody.RespondFailure();
                }
            }
            else
            {
                Context.Log.Error("{0} GET user/location requested without auth token. Token={1}", _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
        }
        private bodyLocationInfo BuildLocationInfo(AccountEntity pAccount, SessionEntity pSession = null)
        {
            bodyLocationInfo ret;
            if (pAccount.Location != null)
            {
                if (!String.IsNullOrEmpty(pAccount.Location.DomainID)
                    && Domains.Instance.TryGetDomainWithID(pAccount.Location.DomainID, out DomainEntity aDomain))
                {
                    ret = new bodyLocationInfo()
                    {
                        node_id = pSession?.SessionID,
                        root = new bodyNamedLoc()
                        {
                            domain = new bodyDomainInfo()
                            {
                                id = aDomain.DomainID,
                                name = aDomain.PlaceName,
                                network_address = aDomain.NetworkAddr,
                                // network_port = pDomain.?,
                                ice_server_address = aDomain.IceServerAddr
                            },
                            name = aDomain.PlaceName
                        },
                        path = pAccount.Location.Path,
                        online = pAccount.IsOnline
                    };
                }
                else
                {
                    // The domain does not have an ID
                    ret = new bodyLocationInfo()
                    {
                        node_id = pSession?.SessionID,
                        online = pAccount.IsOnline,
                        root = new bodyNamedLoc()
                        {
                            domain = new bodyDomainInfo()
                            {
                                network_address = pAccount.Location.NetworkAddress,
                                network_port = pAccount.Location.NetworkPort.ToString()
                            }

                        }
                    };
                }
            }
            else
            {
                ret = new bodyLocationInfo()
                {
                    node_id = pSession?.SessionID,
                    online = pAccount.IsOnline
                };
            }
            return ret;
        }

        // = GET /api/v1/user/profile =======================================================
        public struct bodyUserProfileReply
        {
            public bodyUserProfileInfo user;
        }
        public struct bodyUserProfileInfo
        {
            public string username;
            public string xmpp_password;
            public string discourse_api_key;
            public string wallet_id;
        }
        [APIPath("/api/v1/user/profile", "GET", true)]
        public RESTReplyData user_profile_gen(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                respBody.Data = new bodyUserProfileReply()
                {
                    user = new bodyUserProfileInfo()
                    {
                        username = aAccount.Username,
                        xmpp_password = aAccount.xmpp_password,
                        discourse_api_key = aAccount.discourse_api_key,
                        wallet_id = aAccount.wallet_id
                    }
                };
            }
            else
            {
                Context.Log.Error("{0} GET user/profile requested without auth token. Token={1}", _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            // Context.Log.Debug("{0} GET user/profile. Response={1}", _logHeader, replyData.Body);
            return replyData;
        }

        // = PUT /api/v1/user/public_key ====================================================
        [APIPath("/api/v1/user/public_key", "PUT", true)]

        public RESTReplyData set_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            string includedAPIKey = null;
            string includedPublicKey = null;
            bool parsed = false;    // whether multipart was parsed
            try
            {
                // The PUT sends the key as binary but it is later sent around
                //    and processed as a Base64 string.
                Stream byteStream = pReq.RequestBodyMultipartStream("public_key");
                if (byteStream != null)
                {
                    includedPublicKey = Tools.ConvertPublicKeyStreamToBase64(byteStream);
                    // There might has been an APIKey
                    includedAPIKey = pReq.RequestBodyMultipart("api_key");
                    parsed = true;
                }
                else
                {
                    Context.Log.Error("{0} could not extract public key from request body in PUT user/public_key", _logHeader);
                }
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} exception parsing multi-part http: {1}", _logHeader, e.ToString());
                parsed = false;
            }

            if (parsed)
            {
                // If there is account authorization in the header, set the account public key.
                // If no account authorization (the client is not logged in), check for matching
                //     APIkey and then set the session public key.
                if (String.IsNullOrEmpty(pReq.AuthToken))
                {
                    if (includedAPIKey != null
                            && Domains.Instance.TryGetDomainWithAPIKey(includedAPIKey, out DomainEntity aDomain))
                    {
                        aDomain.Public_Key = includedPublicKey;
                    }
                    else
                    {
                        Context.Log.Error("{0} PUT user/set_public_key requested without APIKey. APIKey={1}",
                                                _logHeader, includedAPIKey);
                        replyData.Status = (int)HttpStatusCode.Unauthorized;
                    }
                }
                else
                {
                    if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
                    {
                        aAccount.Public_Key = includedPublicKey;
                    }
                    else
                    {
                        Context.Log.Error("{0} PUT user/set_public_key requested but could not find acct. Token={1}",
                                                _logHeader, pReq.AuthToken);
                        replyData.Status = (int)HttpStatusCode.Unauthorized;
                    }
                }
            }
            else
            {
                Context.Log.Error("{0} PUT user/set_public_key failure parsing", _logHeader);
                respBody.RespondFailure();
            }

            replyData.Body = respBody;
            return replyData;
        }

        // = GET /api/v1/users/%/public_key =================================================
        public struct bodyUserPublicKeyReply
        {
            public string public_key;
        }
        [APIPath("/api/v1/users/%/public_key", "GET", true)]
        public RESTReplyData get_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            string username = pArgs.Count == 1 ? pArgs[0] : null;
            if (Accounts.Instance.TryGetAccountWithUsername(username, out AccountEntity aAccount))
            {
                respBody.Data = new bodyUserPublicKeyReply()
                {
                    public_key = aAccount.Public_Key
                };
            }
            else
            {
                Context.Log.Error("{0} GET fetch of user public key for unknown acct. SenderKey: {1}, Username: {2}",
                                        _logHeader, pReq.SenderKey, username);
                respBody.RespondFailure();
            }

            replyData.Body = respBody;
            return replyData;
        }
    }
}
