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

using Project_Apollo.Configuration;

namespace Project_Apollo.Hooks
{
    public class APICommerce
    {
        public static readonly string _logHeader = "[APICommerce]";

        // == GET /api/v1/commerce/marketplace_key ==================================
        private struct bodyMarketplaceKeyResponse
        {
            public string public_key;
        }
        [APIPath("/api/v1/commerce/marketplace_key", "GET", true)]
        public RESTReplyData get_marketplace_key(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            respBody.Data = new bodyMarketplaceKeyResponse()
            {
                public_key = Context.Params.P<string>(AppParams.P_COMMERCE_MARKETPLACEKEY)
            };
            replyData.SetBody(respBody, pReq);

            return replyData;
        }

        // == GET /api/v1/commerce/available_updates?per_page=10 ==================================
        // == GET /api/v1/commerce/history?per_page=10 ==================================
    }
}
