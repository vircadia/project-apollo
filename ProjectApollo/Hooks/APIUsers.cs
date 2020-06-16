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
using System.Text;
using System.IO;

using Project_Apollo.Entities;
using Project_Apollo.Registry;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;

namespace Project_Apollo.Hooks
{
    public class APIUsers
    {
        private static readonly string _logHeader = "[User]";

        // == PUT /api/v1/user/heartbeat ======================================================e
        // The heartbeat depends on whether logged in or not.
        // If not logged in, it just pings the server
        // If logged in, sends location data if it has changed
        public struct bodyUserHeartbeatLocation {
            // whole section is optional and sent if changed
            public LocationInfo location;
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
                    try
                    {
                        // The 'location' information is sent only when it changes
                        JObject requestBodyJSON = pReq.RequestBodyJSON();
                        if (requestBodyJSON.ContainsKey("location"))
                        {
                            bodyUserHeartbeatLocation requestData = pReq.RequestBodyObject<bodyUserHeartbeatLocation>();
                            aAccount.Location = requestData.location;
                        }
                        respBody.Data = new bodyUserHeartbeatResponse()
                        {
                            session_id = aSession?.SessionID
                        };
                    }
                    catch (Exception e)
                    {
                        Context.Log.Error("{0} Exception parsing body of /api/v1/user/heartbeat. AccountID={1}. {2}",
                                        _logHeader, aAccount.AccountID, e);
                        respBody.RespondFailure();
                    }
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
            public bodyLocationInfo location;
        }
        public struct bodyLocationInfo
        {
            public string node_id;      // sessionID
            public bodyNamedLoc root;
            public bodyNamedLoc domain;
        }
        public struct bodyNamedLoc
        {
            public string name;
        }
        [APIPath("/api/v1/users", "GET", true)]
        public RESTReplyData user_get(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            PaginationInfo pagination = new PaginationInfo(pReq);
            AccountFilterInfo acctFilter = new AccountFilterInfo(pReq);

            SessionEntity aSession = Sessions.Instance.GetSession(pReq.SenderKey);

            // var foundUsers = new List<AccountEntity>(pagination.Filter<AccountEntity>(acctFilter.Filter()));
            respBody.Data = new bodyUsersReply() {
                users = pagination.Filter<AccountEntity>(acctFilter.Filter()).Select(acct =>
                {
                    return new bodyUser()
                    {
                        username = acct.Username,
                        connection = "",
                        images = acct.Images,
                        location = new bodyLocationInfo()
                        {
                            node_id = aSession?.SessionID,
                            root = new bodyNamedLoc()
                            {
                                name = acct.Location?.PlaceID
                            }
                        }
                    };
                }).ToArray()
            };

            pagination.AddReplyFields(respBody);

            replyData.Body = respBody;  // serializes JSON
            return replyData;
        }

        // == POST /api/v1/users ======================================================e
        public struct bodyUsersPost
        {
            public Dictionary<string,string> user;
        }
        // TODO: What authorization is needed for this? DDOS attack possibility?
        [APIPath("/api/v1/users", "POST", true)]
        public RESTReplyData user_create(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            if (Sessions.Instance.ShouldBeThrottled(pReq.SenderKey, Sessions.Op.ACCOUNT_CREATE))
            {
                respBody.RespondFailure();
                respBody.Data = new
                {
                    operation = "throttled"
                };
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
                        respBody.Data = new Dictionary<string, string>() {
                            { "username", "already exists" }
                        };
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
                    friends = new List<string>()
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
        // = PUT /api/v1/user/friends/% ==================================================
        [APIPath("/api/v1/user/friends/%", "POST", true)]
        public RESTReplyData user_friends_post(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // TODO: Should put something
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

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // TODO: Should put something
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

        // = GET /api/v1/user/connections ==================================================
        public struct bodyUserConnectionsReply
        {
            public List<string> connections;
        };
        [APIPath("/api/v1/user/connections", "GET", true)]
        public RESTReplyData user_connections_get(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                respBody.Data = new bodyUserConnectionsReply()
                {
                    connections = new List<string>()
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
        // = PUT /api/v1/user/connections/% ==================================================
        [APIPath("/api/v1/user/connections/%", "POST", true)]
        public RESTReplyData user_connections_post(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // TODO: Should put something
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

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                // TODO: Should put something
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
                    bodyUserHeartbeatLocation locInfo = pReq.RequestBodyObject<bodyUserHeartbeatLocation>();
                    aAccount.Location = locInfo.location;
                    aAccount.Updated();
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


        // == GET /api/v1/users/%/location ==================================================
        [APIPath("/api/v1/users/%/location", "GET", true)]
        public RESTReplyData get_location(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                respBody.Data = new bodyUserHeartbeatLocation()
                {
                    location = aAccount.Location
                };
            }
            else
            {
                Context.Log.Error("{0} GET user/location requested without auth token. Token={1}", _logHeader, pReq.AuthToken);
                respBody.RespondFailure();
            }
            replyData.Body = respBody;
            return replyData;
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
                    using var memStream = new MemoryStream();
                    byteStream.CopyTo(memStream);
                    includedPublicKey= Convert.ToBase64String(memStream.ToArray());

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

            string accountID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Accounts.Instance.TryGetAccountWithID(accountID, out AccountEntity aAccount))
            {
                respBody.Data = new bodyUserPublicKeyReply()
                {
                    public_key = aAccount.Public_Key
                };
            }
            else
            {
                respBody.RespondFailure();
            }

            replyData.Body = respBody;
            return replyData;
        }
    }
}
