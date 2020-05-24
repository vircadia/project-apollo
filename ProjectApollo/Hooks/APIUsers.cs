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

using Newtonsoft.Json;

using Project_Apollo.Entities;
using Project_Apollo.Registry;

namespace Project_Apollo.Hooks
{
    public class APIUsers
    {
        private static readonly string _logHeader = "[User]";

        // == GET /api/v1/users ======================================================e
        //          ?filter=connections|friends
        //          ?status=online
        //          ?per_page=N
        //          ?page=N
        public struct users_request
        {
            public Dictionary<string, string> User;
        }
        [APIPath("/api/v1/users", "POST", true)]
        public RESTReplyData user_create(RESTRequestData pReq, List<string> pArgs)
        {
            // This specific endpoint only creates a user
            users_request usreq;
            try
            {
                usreq = pReq.RequestBodyObject<users_request>();
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} Malformed reception: {1}", _logHeader, e.ToString());
                throw new Exception("Malformed content");
            }

            string userName = usreq.User["username"];
            string userPW = usreq.User["password"];
            string userEmail = usreq.User["email"];

            ResponseBody respBody = new ResponseBody();
            if (!Users.Instance.CreateAccountPW(userName, userPW, userEmail))
            {
                // If didn't create, return a fail
                respBody = new ResponseBody("fail", new Dictionary<string, string>()
                {
                    { "username", "already exists" }
                });
            }

            return new RESTReplyData(respBody);

        }

        // = POST /api/v1/user/locker ==================================================
        [APIPath("/api/v1/user/locker", "POST", true)]
        public RESTReplyData user_locker_set(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            if (Users.Instance.TryGetUserWithAuth(pReq.AuthToken, out UserEntity aUser))
            {
                // TODO: do whatever one does with a locker
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

        // = GET /api/v1/user/locker ==================================================
        [APIPath("/api/v1/user/locker", "GET", true)]
        public RESTReplyData user_locker_get(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            if (Users.Instance.TryGetUserWithAuth(pReq.AuthToken, out UserEntity aUser))
            {
                // TODO: do whatever one does with a locker
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }


        // == PUT /api/v1/user/location ================================================
        public struct LocationPacket
        {
            public string status;
            public UserAccounts.Location location;
        }
        [APIPath("/api/v1/user/location", "PUT", true)]
        public RESTReplyData user_location_set(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            if (Users.Instance.TryGetUserWithAuth(pReq.AuthToken, out UserEntity aUser))
            {
                LocationPacket loc = pReq.RequestBodyObject<LocationPacket>();
                // TODO: do whatever putting the location does
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }


        // == GET /api/v1/users/%/location ==================================================
        [APIPath("/api/v1/users/%/location", "GET", true)]
        public RESTReplyData get_location(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();

            string userID = pArgs.Count == 1 ? pArgs[0] : null;
            if (Users.Instance.TryGetUserWithID(userID, out UserEntity aUser))
            {
                if (aUser.Location != null)
                {
                    respBody = new ResponseBody(aUser.Location);
                }
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

        // = GET /api/v1/user/profile =======================================================
        public struct user_profile_reply
        {
            public string status;
            public Dictionary<string, UserProfile> data;
        }
        public struct UserProfile
        {
            public string username;
            public string xmpp_password;
            public string discourse_api_key;
            public string wallet_id;
            public UserProfile(string Username)
            {
                username = Username;

                xmpp_password = Tools.SHA256Hash("deprecated");
                discourse_api_key = Tools.SHA256Hash("deprecated");
                wallet_id = Tools.SHA256Hash(username);
            }
        }

        [APIPath("/api/v1/user/profile", "GET", true)]
        public RESTReplyData user_profile_gen(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            if (Users.Instance.TryGetUserWithAuth(pReq.AuthToken, out UserEntity aUser))
            {
                // TODO: do whatever about returning a profile
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

        // = PUT /api/v1/user/public_key ====================================================
        [APIPath("/api/v1/user/public_key", "PUT", true)]

        public RESTReplyData set_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            if (Users.Instance.TryGetUserWithAuth(pReq.AuthToken, out UserEntity aUser))
            {
                // TODO: do whatever about setting the user public_key
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }

        // = GET /api/v1/users/%/public_key =================================================
        [APIPath("/api/v1/users/%/public_key", "GET", true)]
        public RESTReplyData get_public_key(RESTRequestData pReq, List<string> pArgs)
        {
            ResponseBody respBody = new ResponseBody();
            if (Users.Instance.TryGetUserWithAuth(pReq.AuthToken, out UserEntity aUser))
            {
                // TODO: do whatever about returning the user public_key
            }
            else
            {
                respBody.Status = "fail";
            }
            return new RESTReplyData(respBody);
        }


        // = GET /user/tokens/new =================================================
        // THIS IS EXPERIMENTAL AND NEW SO HANDS OFF!!
        [APIPath("/user/tokens/new", "GET", true)]
        public RESTReplyData user_tokens(RESTRequestData pReq, List<string> pArgs)
        {
        /*
            if(pReq.Headers.ContainsKey("Authorization") == false)
            {

                RESTReplyData rd = new RESTReplyData();
                rd.Status = 401;
                rd.Body = "";
                Session.Instance.TemporaryStackData.Add(pReq.RemoteUser.ToString());
                rd.CustomOutputHeaders.Add("WWW-Authenticate", "Basic realm='Tokens'");
                rd.Body = "<h2>You are not logged in!";
                rd.CustomOutputHeaders.Add("Content-Type", "text/html");
                return rd;
            } else
            {
                // Validate login!
                string[] req = pArgs[0].Split(new[] { '?', '&', '=' });
                string[] authHeader = pReq.Headers["Authorization"].Split(new[] { ' ' });

                if(authHeader[0] == "Basic" && Session.Instance.TemporaryStackData.Contains(pReq.RemoteUser.ToString()))
                {
                    // Validate credentials!
                    UserAccounts ua = UserAccounts.GetAccounts();

                    string[] auth = Tools.Base64Decode(authHeader[1]).Split(new[] { ':' });

                    if(ua.Login(auth[0], auth[1], "web"))
                    {
                        // Continue to generate the token!
                        UserAccounts.Account act = ua.AllAccounts[auth[0]]; 
                        for (int i = 0; i < req.Length; i++)
                        {
                            if (req[i] == "for_domain_server" && req[i + 1] == "true")
                            {
                                // Generate the domain server token!
                                int expiry = 1 * 24 * 60 * 60;
                                int time = Tools.getTimestamp();
                                string token_type = "domain";

                                string Token = Tools.MD5Hash(expiry.ToString() + ":" + time.ToString() + "::" + token_type + ":" + act.name);
                                // Token has now been issued!
                                // Because you can obviously have more than 1 domain, this will save the token as : domain-timestamp

                                act.ActiveTokens.Add(Token, "domain");
                                ua.AllAccounts[auth[0]] = act;
                                ua.save();

                                // Exit this loop, and reply to the user!

                                Session.Instance.TemporaryStackData.Remove(pReq.RemoteUser.ToString());
                                RESTReplyData rd1 = new RESTReplyData();
                                rd1.Status = 200;
                                rd1.Body = $"<center><h2>Your domain's access token is: {Token}</h2></center>";
                                rd1.CustomOutputHeaders.Add("Content-Type", "text/html");

                                return rd1;
                            }
                        }
                    }
                }

                RESTReplyData rd = new RESTReplyData();
                rd.Body = "Invalid authorization header was provided!<br/>If you were not prompted for credentials again, close the tab or the browser and try again";
                rd.Status = 401;
                if (Session.Instance.TemporaryStackData.Contains(pReq.RemoteUser.ToString()) == false)
                    Session.Instance.TemporaryStackData.Add(pReq.RemoteUser.ToString());
                rd.CustomOutputHeaders.Add("WWW-Authenticate", "Basic realm='Tokens'");
                return rd;
            }
            */
                RESTReplyData rd = new RESTReplyData();
                return rd;
        }
    }

    public class token_oauth
    {

        public struct Login_Reply
        {
            public string access_token;
            public string token_type;
            public int expires_in;
            public string refresh_token;
            public string scope;
            public int created_at;
        }
        [APIPath("/oauth/token", "POST", true)]
        public RESTReplyData user_login(RESTRequestData pReq, List<string> pArgs)
        {
            Console.WriteLine("====> Starting User Login <=====");

            RESTReplyData data = new RESTReplyData();
            Login_Reply lr = new Login_Reply();
            Dictionary<string, string> arguments = Tools.PostBody2Dict(pReq.RequestBody);
            lr.created_at = Tools.getTimestamp();
            lr.expires_in = 1 * 24 * 60 * 60; // 1 DAY
            lr.token_type = "Bearer";
            lr.scope = arguments["scope"];
            lr.refresh_token = Tools.SHA256Hash(lr.expires_in.ToString() + ";" + pReq.RemoteUser.ToString() + ";" + arguments["username"]);
            lr.access_token = Tools.SHA256Hash(Tools.getTimestamp() + ";" + lr.refresh_token);

            UserAccounts uac = UserAccounts.GetAccounts();
            if (uac.Login(arguments["username"], arguments["password"], lr.access_token))
            {
                Console.WriteLine("===> Login success");
                data.Body = JsonConvert.SerializeObject(lr);
                data.Status = 200;
                return data;
            }
            else
            {
                Console.WriteLine("====> Login failed");
                // users.users_reply failreply = new users.users_reply();
                // failreply.status = "fail";
                // failreply.data = new Dictionary<string, string>();
                // failreply.data.Add("reason", "Unknown");
                data.Status = 200;
                // data.Body = JsonConvert.SerializeObject(failreply);
                return data;
            }

        }
    }
}
