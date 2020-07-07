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
using Project_Apollo.Configuration;

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
            // The /oauth/token request doesn't return a regular respBody

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

                        AuthTokenInfo.ScopeCode userScope = AuthTokenInfo.ScopeCode.owner;
                        if (reqArgs.ContainsKey("scope"))
                        {
                            try
                            {
                                userScope = Enum.Parse<AuthTokenInfo.ScopeCode>(reqArgs["scope"] ?? "owner");
                            }
                            catch
                            {
                                Context.Log.Error("{0} /oauth/token: unknown scope code: {1}", _logHeader, reqArgs["scope"]);
                                userScope = AuthTokenInfo.ScopeCode.owner;
                            }
                        }

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
                scope = pTokenInfo.Scope.ToString(),
                created_at = ((DateTimeOffset)pTokenInfo.TokenCreationTime).ToUnixTimeSeconds()
            });
        }

        // = GET /user/tokens/new =================================================
        // This request is used by the domain-server to get user tokens. Used as part
        //    of the "associate domain with account" logic.
        // This is a human readable form that exists for backward compatibility.
        // Remove when domain initialization is completely replaced with the other form.
        // Example: http://192.168.86.41:19400/user/tokens/new?for_domain_server=true
        [APIPath("/user/tokens/new", "GET", true)]
        public RESTReplyData user_tokens(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info

            if (pReq.Queries.TryGetValue("for_domain_server", out string oForDomainServer))
            {
                if (Boolean.Parse(oForDomainServer))
                {
                    // Getting a token for a domain server
                    if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity oAccount))
                    {
                        AuthTokenInfo token = oAccount.CreateAccessToken(AuthTokenInfo.ScopeCode.domain);
                        // The domain/account association is permenant... expiration is far from now
                        token.TokenExpirationTime = new DateTime(2999, 12, 31);

                        replyData.Body = $"<center><h2>Your domain's access token is: {token.Token}</h2></center>";
                        replyData.MIMEType = "text/html";
                    }
                    else
                    {
                        // If the user is not logged in, go to a page to login and set things up
                        replyData.Status = (int)HttpStatusCode.Found;
                        replyData.CustomOutputHeaders.Add("Location",
                                    Context.Params.P<string>(AppParams.P_DOMAIN_TOKENGEN_URL));
                        replyData.MIMEType = "text/html";
                    }
                }
            }
            return replyData;
        }
        // = GET /api/v1/token/new =================================================
        // Get a new authorization token associated with the account of the header authorization.
        // The token is for the account. A scope can be specified with "?scope=SCOPE".
        // This request is often used to get a domain's access token for it's sponsering account.
        //     In this case, "?scope=domain".
        // Example: http://metaverse.vircadia.com:9400/api/v1/token/new?scope=domain
        [APIPath("/api/v1/token/new", "POST", true)]
        public RESTReplyData new_scope_token(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Sessions.Instance.ShouldBeThrottled(pReq.SenderKey, Sessions.Op.TOKEN_CREATE))
            {
                respBody.RespondFailure();
                respBody.Data = new
                {
                    operation = "throttled"
                };
            }
            else
            {
                // Getting a token for a domain server
                if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity oAccount))
                {
                    AuthTokenInfo token = null;

                    string scope = pReq.Queries.ContainsKey("scope") ? pReq.Queries["scope"] : "owner";
                    switch (scope)
                    {
                        case "domain":
                            token = oAccount.CreateAccessToken(AuthTokenInfo.ScopeCode.domain);
                            // The domain/account association lasts longer
                            token.TokenExpirationTime = new DateTime(2999, 12, 31);
                            break;
                        case "owner":
                            token = oAccount.CreateAccessToken(AuthTokenInfo.ScopeCode.owner);
                            break;
                        default:
                            break;
                    }

                    if (token != null)
                    {
                        respBody.Data = new
                        {
                            token = token.Token,
                            token_id = token.TokenId,
                            refresh_token = token.RefreshToken,
                            token_expiration_seconds = (int)(token.TokenExpirationTime - token.TokenCreationTime).TotalSeconds,
                            account_name = oAccount.Username
                        };
                    }
                    else
                    {
                        respBody.RespondFailure();
                    }
                }
                else
                {
                    // Not a known account.
                    respBody.RespondFailure();
                }
            }
            replyData.Body = respBody;
            return replyData;
        }
    }
}
