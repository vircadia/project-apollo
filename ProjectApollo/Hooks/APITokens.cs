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

using Project_Apollo.Entities;
using Project_Apollo.Registry;

using Newtonsoft.Json;

namespace Project_Apollo.Hooks
{
    public class token_oauth
    {
        private static readonly string _logHeader = "[APITokens]";

        public struct bodyLoginReply
        {
            public string access_token;
            public string token_type;
            public int expires_in;
            public string refresh_token;
            public string scope;
            public Int64 created_at;
        }
        [APIPath("/oauth/token", "POST", true)]
        public RESTReplyData user_login(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info

            // Should verify that the content-type is "application/x-www-form-urlencoded"
            Dictionary<string, string> reqArgs = Tools.PostBody2Dict(pReq.RequestBody);

            string accessGrantType = reqArgs["grant_type"];
            switch (accessGrantType)
            {
                case "password":
                    {
                        // There are several types of "password"s passed by Interface:
                        // PLAIN PASSWORD
                        string userName = reqArgs["username"];
                        string userPassword = reqArgs["password"];

                        // STEAM
                        // string userPassword = reqArgs["steam_auth_ticket"];

                        // OCULUS
                        // string userPassword = reqArgs["oculus_nonce"];
                        // string userPassword = reqArgs["oculus_id"];

                        string userScope = reqArgs["scope"] ?? "owner";

                        // Context.Log.Debug("{0} Get access token for {1} with password", _logHeader, userName);

                        if (Accounts.Instance.TryGetAccountWithUsername(userName, out AccountEntity aAccount))
                        {
                            if (Accounts.Instance.ValidPassword(aAccount, userPassword))
                            {
                                Context.Log.Debug("{0} Login of user {1}", _logHeader, userName);

                                AuthTokenInfo authInfo = aAccount.CreateAccessToken(userScope, pReq.SenderKey + ";" + userName);

                                // The response does not follow the usual {status: , data: } form.
                                replyData.Body = OAuthTokenResponseBody(authInfo);
                            }
                            else
                            {
                                Context.Log.Debug("{0} Login failed for user {1}", _logHeader, userName);
                                // The password doesn't work.
                                replyData.Body = OAuthResponseError("Login failed");
                                replyData.Status = (int)HttpStatusCode.Unauthorized;
                            }
                        }
                        else
                        {
                            Context.Log.Error("{0} Attempt to get token for unknown user {1}. Sender={2}",
                                            _logHeader, userName, pReq.SenderKey);
                            replyData.Body = OAuthResponseError("Unknown user");
                        }
                        break;
                    }
                case "authorization_code":
                    {
                        string clientID = reqArgs["client_id"];
                        string clientSecret = reqArgs["client_secret"];
                        string clientAuthCode = reqArgs["code"];
                        string redirectURL = reqArgs["redirect_url"];

                        Context.Log.Error("{0} Attempt to login with 'authorization_code'. clientID={1}",
                                            _logHeader, clientID);
                        replyData.Body = OAuthResponseError("Cannot process 'authorization_code'");
                        replyData.Status = (int)HttpStatusCode.Unauthorized;
                        break;
                    }
                case "refresh_token":
                    {
                        string refreshingToken = reqArgs["refresh_token"];
                        string userScope = reqArgs["scope"] ?? "owner";

                        if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
                        {
                            Context.Log.Debug("{0} Refreshing access token for account {1}", _logHeader, aAccount.AccountID);
                            AuthTokenInfo refreshToken = aAccount.RefreshAccessToken(refreshingToken);
                            if (refreshToken != null)
                            {
                                replyData.Body = OAuthTokenResponseBody(refreshToken);
                            }
                            else
                            {
                                replyData.Body = OAuthResponseError("Cannot refresh");
                            }

                        }
                        else
                        {
                            Context.Log.Error("{0} Attempt to refresh token for not logged in user", _logHeader);
                            replyData.Body = OAuthResponseError("Unknown user");
                        }
                        break;
                    }
                default:
                    Context.Log.Error("{0} Attempt to login with unknown grant type. Type={1}",
                                        _logHeader, accessGrantType);
                    replyData.Body = OAuthResponseError("Unknown grant type: " + accessGrantType);
                    replyData.Status = (int)HttpStatusCode.Unauthorized;
                    break;
            }

            // Context.Log.Debug("{0} oauth/token replyBody={1}", _logHeader, replyData.Body);
            return replyData;
        }

        // Create the type of error body the oauth request is looking for
        private string OAuthResponseError(string pMsg)
        {
            return JsonConvert.SerializeObject(new Dictionary<string, string>()
            {
                { "error", pMsg }
            });
        }
        private string OAuthTokenResponseBody(AuthTokenInfo pTokenInfo)
        {
            return JsonConvert.SerializeObject(new bodyLoginReply()
            {
                access_token = pTokenInfo.Token,
                token_type = "Bearer",
                expires_in = (int)(pTokenInfo.TokenExpirationTime - pTokenInfo.TokenCreationTime).TotalSeconds,
                refresh_token = pTokenInfo.RefreshToken,
                scope = pTokenInfo.Scope,
                created_at = ((DateTimeOffset)pTokenInfo.TokenCreationTime).ToUnixTimeSeconds()
            });
        }

        /*
        // = GET /user/tokens/new =================================================
        // THIS IS EXPERIMENTAL AND NEW SO HANDS OFF!!
        [APIPath("/user/tokens/new", "GET", true)]
        public RESTReplyData user_tokens(RESTRequestData pReq, List<string> pArgs)
        {
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
                RESTReplyData rd = new RESTReplyData();
                return rd;
        }
            */
    }
}
