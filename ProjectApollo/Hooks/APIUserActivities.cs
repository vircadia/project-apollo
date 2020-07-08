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

using Project_Apollo.Entities;
using Project_Apollo.Registry;

using Newtonsoft.Json;

namespace Project_Apollo.Hooks
{
    public class APIUserActivities
    {
        private static readonly string _logHeader = "[UserActivities]";

        public struct bodyUserActivityRequest
        {
            public string action_name;
        }
        [APIPath("/api/v1/user_activities", "POST", true)]
        public RESTReplyData user_activity (RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();

            try
            {
                bodyUserActivityRequest reqBody = pReq.RequestBodyObject<bodyUserActivityRequest>();

                if (Accounts.Instance.TryGetAccountWithAuthToken(pReq.AuthToken, out AccountEntity aAccount))
                {

                    // What does an activity do?
                    Context.Log.Info("{0} Received user_activity={1} from {2}",
                                    _logHeader, reqBody.action_name, aAccount.Username);
                }
                else
                {
                    Context.Log.Info("{0} Received user_activity={1} from unknown user",
                                    _logHeader, reqBody.action_name);
                    respBody.Status = "notfound";
                    replyData.Status = (int)HttpStatusCode.NotFound;
                }
            }
            catch
            {
                Context.Log.Error("{0} Badly formed user_activities request from {1}",
                                        _logHeader, pReq.SenderKey);
                replyData.Status = (int)HttpStatusCode.BadRequest;
            }
            replyData.SetBody(respBody, pReq);
            return replyData;
        }
    }
}
