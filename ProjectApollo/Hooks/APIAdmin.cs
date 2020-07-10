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
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Newtonsoft.Json.Linq;
using Project_Apollo.Entities;
using Project_Apollo.Registry;

namespace Project_Apollo.Hooks
{
    class APIAdmin
    {
        private static readonly string _logHeader = "[APIAdmin]";

        public struct bodyAccountsReply
        {
            public bodyAccountInfo[] accounts;
        }
        public struct bodyAccountInfo
        {
            public string accountid;
            public string username;
            public string email;
            public string public_key;
            public object images;
            public object location;
            public string[] friends;
            public string[] connections;
            public bool administrator;
            public string when_account_created;
            public string time_of_last_heartbeat;

            public bodyAccountInfo(AccountEntity pAcct)
            {
                accountid = pAcct.AccountID;
                username = pAcct.Username;
                email = pAcct.Email;
                public_key = pAcct.Public_Key;
                images = new
                {
                    hero = pAcct.Images.Hero,
                    thumbnail = pAcct.Images.Thumbnail,
                    tiny = pAcct.Images.Tiny
                };
                location = new 
                {
                    connected = pAcct.Location.Connected.ToString(),
                    path = pAcct.Location.Path,
                    placeid = pAcct.Location.PlaceID,
                    domainid = pAcct.Location.DomainID,
                    availability = pAcct.Location.Availability.ToString()
                };
                friends = pAcct.Friends.ToArray();
                connections = pAcct.Connections.ToArray();
                administrator = pAcct.Administrator;
                when_account_created = XmlConvert.ToString(pAcct.WhenAccountCreated, XmlDateTimeSerializationMode.Utc);
                time_of_last_heartbeat = XmlConvert.ToString(pAcct.TimeOfLastHeartbeat, XmlDateTimeSerializationMode.Utc);
            }
        }
        /// <summary>
        /// API request to return detailed account information
        /// </summary>
        /// <param name="pReq"></param>
        /// <param name="pArgs"></param>
        /// <returns></returns>
        [APIPath("/api/v1/accounts", "GET", true)]
        public RESTReplyData admin_get_accounts(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                PaginationInfo pagination = new PaginationInfo(pReq);
                AccountFilterInfo acctFilter = new AccountFilterInfo(pReq);
                AccountScopeFilter scopeFilter = new AccountScopeFilter(pReq, aAccount);

                respBody.Data = new bodyAccountsReply() {
                    accounts = pagination.Filter<AccountEntity>(scopeFilter.Filter(acctFilter.Filter(aAccount))).Select(acct =>
                    {
                        return new bodyAccountInfo(acct);
                    }).ToArray()
                };
            }
            else
            {
                Context.Log.Error("{0} GET accounts requested with bad auth", _logHeader);
                respBody.RespondFailure("Bad authorization");
            }
            replyData.SetBody(respBody, pReq);
            return replyData;
        }

        [APIPath("/api/v1/account/%", "POST", true)]
        public RESTReplyData admin_post_accounts(RESTRequestData pReq, List<string> pArgs)
        {
            return APITargetedAccountOperation(pReq, pArgs, (pRespBody, pSrcAcct, pTargetAcct) =>
            {
                try
                {
                    JObject body = pReq.RequestBodyJSON();
                    if (body.ContainsKey("accounts"))
                    {
                        JObject acctInfo = (JObject)body["accounts"];
                        Tools.SetIfSpecified<string>(acctInfo, "username", ref pTargetAcct.Username);
                        Tools.SetIfSpecified<string>(acctInfo, "email", ref pTargetAcct.Email);
                        Tools.SetIfSpecified<string>(acctInfo, "public_key", ref pTargetAcct.Public_Key);
                        if (acctInfo.ContainsKey("images"))
                        {
                            JObject imageInfo = (JObject)acctInfo["images"];
                            if (pTargetAcct.Images == null)
                            {
                                pTargetAcct.Images = new UserImages();
                            }
                            Tools.SetIfSpecified<string>(imageInfo, "hero", ref pTargetAcct.Images.Hero);
                            Tools.SetIfSpecified<string>(imageInfo, "thumbnail", ref pTargetAcct.Images.Thumbnail);
                            Tools.SetIfSpecified<string>(imageInfo, "tiny", ref pTargetAcct.Images.Tiny);
                        }
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} POST /api/v1/account/%: exception parsing body '{1}' from {2} account {3}",
                                _logHeader, pReq.RequestBody, pReq.SenderKey, pSrcAcct.Username);
                    pRespBody.RespondFailure("Exception parsing message body", e.ToString());
                    pRespBody.ErrorData("error", "parse body failure");
                }
            });
        }

        [APIPath("/api/v1/account/%", "DELETE", true)]
        public RESTReplyData admin_delete_accounts(RESTRequestData pReq, List<string> pArgs)
        {
            return APITargetedAccountOperation(pReq, pArgs, (pRespBody, pSrcAcct, pTargetAcct) =>
            {
                Context.Log.Error("{0} UNIMPLIMENTED: DELETE /api/v1/account/%. From={1}",
                                _logHeader, pReq.SenderKey);
                pRespBody.RespondFailure("Not implemented");
            });
        }

        public struct bodyTokensReply
        {
            public bodyTokenInfo[] tokens;
        }
        public struct bodyTokenInfo
        {
            public string tokenid;
            public string token;
            public string refresh_token;
            public string token_creation_time;
            public string token_expiration_time;
            public string scope;

            public bodyTokenInfo(AuthTokenInfo pToken)
            {
                tokenid = pToken.TokenId;
                token = pToken.Token;
                refresh_token = pToken.RefreshToken;
                token_creation_time = XmlConvert.ToString(pToken.TokenCreationTime, XmlDateTimeSerializationMode.Utc);
                token_expiration_time = XmlConvert.ToString(pToken.TokenExpirationTime, XmlDateTimeSerializationMode.Utc);
                scope = pToken.Scope.ToString();
            }
        }
        [APIPath("/api/v1/account/%/tokens", "GET", true)]
        public RESTReplyData account_get_tokens(RESTRequestData pReq, List<string> pArgs)
        {
            return APITargetedAccountOperation(pReq, pArgs, (pRespBody, pSrcAcct, pTargetAcct) =>
            {
                PaginationInfo pagination = new PaginationInfo(pReq);
                pRespBody.Data = new bodyTokensReply()
                {
                    tokens = pagination.Filter<AuthTokenInfo>(pTargetAcct.AuthTokens.Enumerate()).Select(tok =>
                    {
                        return new bodyTokenInfo(tok);
                    }).ToArray()
                };
            });
        }
        /*  There is nothing that anyone can change in a token
        [APIPath("/api/v1/account/%/token/%", "POST", true)]
        public RESTReplyData account_post_tokens(RESTRequestData pReq, List<string> pArgs)
        {
            return APITargetedOperation(pReq, pArgs, (pRespBody, pSrcAcct, pTargetAcct) =>
            {
                string tokenId = pArgs.Count > 1 ? pArgs[1] : null;
                if (tokenId != null) {
                    if (pTargetAcct.TryGetAuthTokenInfo(tokenId, out AuthTokenInfo tokenInfo))
                    {
                        JObject requestData = pReq.RequestBodyJSON();
                        JObject tokenStuff = (JObject)requestData["token"];
                        Tools.SetIfSpecified<AuthTokenInfo.ScopeCode>(tokenStuff, "scope", ref tokenInfo.Scope);
                    }
                }
            });
        }
        */
        [APIPath("/api/v1/account/%/token/%", "DELETE", true)]
        public RESTReplyData account_delete_tokens(RESTRequestData pReq, List<string> pArgs)
        {
            return APITargetedAccountOperation(pReq, pArgs, (pRespBody, pSrcAcct, pTargetAcct) =>
            {
                string tokenId = pArgs.Count > 1 ? pArgs[1] : null;
                if (tokenId != null) {
                    if (pTargetAcct.TryGetAuthTokenInfo(tokenId, out AuthTokenInfo tokenInfo))
                    {
                        pTargetAcct.AuthTokens.Delete(tokenInfo);
                    }
                }
                

            });
        }

        public delegate void DoOperation(ResponseBody pRespBody, AccountEntity pRequestingAccount, AccountEntity pTargetAccount);
        /// <summary>
        /// Do an API operation where one account is operating on a targetted account.
        /// Takes all the request parameters and an operation to perfrom on the target account.
        /// The target account is specified by an accountID as the first parameter in the
        /// request URL.
        /// </summary>
        /// <remarks>
        /// The requesting account must be the target account or an administration account.
        /// </remarks>
        /// <param name="pReq"></param>
        /// <param name="pArgs"></param>
        /// <param name="pDoOp"></param>
        /// <returns></returns>
        public RESTReplyData APITargetedAccountOperation(RESTRequestData pReq, List<string> pArgs, DoOperation pDoOp)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
            {
                string otherAcct = pArgs.Count > 0 ? pArgs[0] : null;
                if (otherAcct != null)
                {
                    if (Accounts.Instance.TryGetAccountWithID(otherAcct, out AccountEntity targetAccount))
                    {
                        // either the requestor is admin or the same account
                        if (aAccount.Administrator || aAccount.AccountID == targetAccount.AccountID)
                        {
                            pDoOp(respBody, aAccount, targetAccount);
                        }
                        else
                        {
                            respBody.RespondFailure("Not account or administrator");
                            replyData.Status = (int)HttpStatusCode.Unauthorized;
                        };
                    }
                    else
                    {
                        respBody.RespondFailure("No such account");
                        replyData.Status = (int)HttpStatusCode.Unauthorized;
                    };
                }
                else
                {
                    respBody.RespondFailure("Account not included in request URL");
                }
            }
            else
            {
                respBody.RespondFailure("Unauthorized");
            }
            replyData.SetBody(respBody, pReq);
            return replyData;
        }


    }
}
