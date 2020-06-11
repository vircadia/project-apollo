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
using System.Text;

using Project_Apollo.Configuration;
using Project_Apollo.Registry;

namespace Project_Apollo.Hooks
{
    public class APIMisc
    {
        public static readonly string _logHeader = "[APIMisc]";

        // == GET /api/metaverse_info ==================================
        private struct bodyMetaverseInfoResponse
        {
            public string metaverse_url;
            public string ice_server_url;
            public string metaverse_server_version;
        }
        [APIPath("/api/metaverse_info", "GET", true)]
        public RESTReplyData get_metaverse_info(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info
            ResponseBody respBody = new ResponseBody();     // The request's "data" response info

            respBody.Data = new bodyMetaverseInfoResponse()
            {
                metaverse_url = Context.Params.P<string>(AppParams.P_METAVERSE_SERVER_URL),
                ice_server_url = Context.Params.P<string>(AppParams.P_DEFAULT_ICE_SERVER),
                metaverse_server_version = ThisAssembly.AssemblyInformationalVersion
            };
            replyData.Body = respBody;  // serializes JSON

            return replyData;
        }
    }
}
