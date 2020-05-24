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
using Project_Apollo.Registry;
using Newtonsoft.Json;
using System.IO;
using static Project_Apollo.Registry.APIRegistry;

namespace Project_Apollo.Hooks
{
    public class APIHeartbeat
    {
        public struct heartbeat_ReplyData
        {
            public string status;
            public Dictionary<string, string> data;
        }
        [APIPath("/api/v1/user/heartbeat", "PUT", true)]
        public RESTReplyData Heartbeat(RESTRequestData pReq, List<string> pArgs)
        {
            RESTReplyData _reply = new RESTReplyData();

            Heartbeat_Memory mem = Heartbeat_Memory.GetHeartbeat();
            if (mem.Contains(pReq.RemoteUser.ToString())) mem.Set(pReq.RemoteUser.ToString(), Guid.NewGuid().ToString());
            else mem.Add(pReq.RemoteUser.ToString(), Guid.NewGuid().ToString());
            heartbeat_ReplyData hbrd = new heartbeat_ReplyData();
            hbrd.status = "success";
            hbrd.data = new Dictionary<string, string>();
            hbrd.data.Add("session_id", mem.Get(pReq.RemoteUser.ToString()));
            _reply.Status = 200;
            _reply.Body = JsonConvert.SerializeObject(hbrd);


            return _reply;
        }
    }

    public class Heartbeat_Memory
    {
        private static readonly object _lock = new object();
        public static Heartbeat_Memory GetHeartbeat()
        {
            lock (_lock)
            {

                if (!File.Exists("presence.json"))
                {
                    Heartbeat_Memory hm = new Heartbeat_Memory();
                    return hm;
                }
                string js = File.ReadAllText("presence.json");
                return (Heartbeat_Memory)JsonConvert.DeserializeObject<Heartbeat_Memory>(js);
            }
        }
        private static readonly object retr = new object();
        public Dictionary<string, string> _pres = new Dictionary<string, string>();
        public bool Contains(string Key)
        {
            return _pres.ContainsKey(Key);
        }

        public void Add(string Key,string Val)
        {
            _pres.Add(Key, Val);
            Commit();
        }

        public void Rem(string Key)
        {
            _pres.Remove(Key);
            Commit();
        }

        public void Set(string Key,string Val)
        {
            _pres[Key] = Val;
            Commit();
        }

        public string Get(string Key)
        {
            return _pres[Key];
        }

        public void Commit()
        {
            lock (_lock)
            {

                File.WriteAllText("presence.json", JsonConvert.SerializeObject(this));
            }
        }

    }
}
