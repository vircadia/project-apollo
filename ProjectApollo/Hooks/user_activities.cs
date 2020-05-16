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
using Newtonsoft.Json;
using Project_Apollo.Registry;
using static Project_Apollo.Registry.APIRegistry;

namespace Project_Apollo.Hooks
{
    public class user_activities
    {

        public struct user_activities_reply
        {
            public string status;
        }

        public struct user_activity_input
        {
            public string action_name;
        }
        [APIPath("/api/v1/user_activities", "POST", true)]
        public ReplyData user_activity (IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            Heartbeat_Memory hbmem = new Heartbeat_Memory();
            user_activity_input uai = (user_activity_input)JsonConvert.DeserializeObject<user_activity_input>(body);
            ReplyData rd = new ReplyData();
            rd.Status = 404;
            rd.Body = "{\"status\":\"notfound\"}";
            if(uai.action_name == "quit")
            {

                
                if (hbmem.Contains(remoteIP.ToString())) hbmem.Rem(remoteIP.ToString());
                rd = new ReplyData();
                rd.Status = 200;
                user_activities_reply uar = new user_activities_reply();
                uar.status = "success";
                rd.Body = JsonConvert.SerializeObject(uar);
                Console.WriteLine("=====> user_action: quit; "+remoteIP.ToString());
                return rd;
            }
            return rd;
        }
        
    }
}
