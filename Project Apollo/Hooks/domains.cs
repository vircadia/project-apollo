using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Project_Apollo.Registry;
using RandomNameGeneratorLibrary;
using static Project_Apollo.Registry.APIRegistry;

namespace Project_Apollo.Hooks
{
    public class domains
    {

        #region PUT Ice Address

        public struct PutIceServerRequest
        {
            public string api_key;
            public Dictionary<string, string> domain;
        }
        public struct DomainReplyData
        {
            public string id;
            public string name;
            public string ice_server_address;
        }
        public struct DomainReplyDataWithKey
        {
            public string id;
            public string name;
            public string ice_server_address;
            public string api_key;
        }
        public struct PutIceServerResponse
        {
            public string status;
            public DomainReplyData domain;
        }

        [APIPath("/api/v1/domains/%/ice_server_address", "PUT", true)]
        public ReplyData put_ice_address(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();
            string domainID = arguments[0];
            DomainReplyData drd = new DomainReplyData();
            PutIceServerRequest isr = JsonConvert.DeserializeObject<PutIceServerRequest>(body);
            PutIceServerResponse isres = new PutIceServerResponse();

            if (Session.Instance.DomainsMem.SetIP(domainID, remoteIP.ToString(), isr.api_key))
            {
                isres.status = "success";
            }
            else
            {
                isres.status = "fail";
            }


            drd.id = domainID;
            DomainMemory.DomainObject dobj = Session.Instance.DomainsMem.Itms[domainID].Obj;
            drd.ice_server_address = dobj.IPAddr;
            drd.name = dobj.PlaceName;

            isres.domain = drd;
            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(isres);

            return rd;
        }


        #endregion

        #region Get Domain

        public struct GetDomainError
        {
            public string status;
            public Dictionary<string, string> data;
        }
        [APIPath("/api/v1/domains/%", "GET", true)]
        public ReplyData get_domain(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();
            string domainID = arguments[0];
            DomainReplyData drd = new DomainReplyData();
            PutIceServerResponse isres = new PutIceServerResponse();
            isres.status = "success";
            DomainMemory.DomainObject dobj = new DomainMemory.DomainObject();
            drd.id = domainID;
            if (Session.Instance.DomainsMem.Itms.ContainsKey(domainID))
            {

                dobj = Session.Instance.DomainsMem.Itms[domainID].Obj;
            }
            else
            {
                // return nothing!
                GetDomainError gde = new GetDomainError();
                gde.status = "fail";
                gde.data = new Dictionary<string, string>();
                gde.data.Add("domain", "there is no domain with that ID");
                rd.Status = 404;
                rd.Body = JsonConvert.SerializeObject(gde);
                return rd;

            }
            drd.ice_server_address = dobj.IPAddr;
            drd.name = dobj.PlaceName;

            isres.domain = drd;
            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(isres);

            return rd;
        }

        #endregion

        #region Temporary Place Name Generate


        public struct TemporaryPlaceNameReply
        {
            public string status;
            public Dictionary<string, DomainReplyDataWithKey> data;
        }

        [APIPath("/api/v1/domains/temporary", "POST", true)]
        public ReplyData get_temporary_name(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();

            PersonNameGenerator png = new PersonNameGenerator();
            PlaceNameGenerator plng = new PlaceNameGenerator();
            // We're generating the entire domain entry in the data store
            DomainMemory.DomainObject DO = new DomainMemory.DomainObject()
            {
                PlaceName = png.GenerateRandomFirstName() + "-" + plng.GenerateRandomPlaceName() + "-" + new Random().Next(500, 9000).ToString(),
                DomainID = Guid.NewGuid().ToString(),
                IPAddr = remoteIP.ToString()
            };


            DO.API_Key = Tools.MD5Hash($":{DO.PlaceName}::{DO.DomainID}:{DO.IPAddr}");

            DomainReplyDataWithKey drd = new DomainReplyDataWithKey();
            drd.ice_server_address = DO.IPAddr;
            drd.api_key = DO.API_Key;
            drd.id = DO.DomainID;
            drd.name = DO.PlaceName;

            TemporaryPlaceNameReply tpnr = new TemporaryPlaceNameReply();
            tpnr.status = "success";
            tpnr.data = new Dictionary<string, DomainReplyDataWithKey>();
            tpnr.data.Add("domain", drd);

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(tpnr);

            DomainMemory.MemoryItem mi = new DomainMemory.MemoryItem();
            mi.Obj = DO;
            Session.Instance.DomainsMem.Itms.Add(DO.DomainID, mi);

            rd.CustomOutputHeaders = new Dictionary<string, string>();
            rd.CustomOutputHeaders.Add("X-Rack-CORS", "miss; no-origin");
            rd.CustomOutputHeaders.Add("Access-Control-Allow-Origin", "*");
            return rd;
        }

        #endregion

        #region Public Key
        [APIPath("/api/v1/domains/%/public_key", "PUT", true)]

        public ReplyData set_public_key(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            string[] Lines = body.Split(new[] { '\n' });

            string Data = "";

            int index = 0;
            foreach (string S in Lines)
            {
                if (index > 3 && S.IndexOf("boundary") == -1)
                {
                    Data += S + "\n";
                }
                index++;
            }

            ReplyData rd = new ReplyData();
            users.replyPacket rp = new users.replyPacket();
            if (Session.Instance.DomainsMem.SetPublicKey(remoteIP.ToString(), Tools.Base64Encode(Data), arguments[0]))
            {
                rd.Status = 200;
                rp.status = "success";
            }
            else
            {
                rd.Status = 403;
                rp.status = "fail";
            }

            rp.data = "";
            rd.Body = JsonConvert.SerializeObject(rp);



            return rd;


        }

        // TODO: CHANGE TO REGEX
        [APIPath("/api/v1/domains/%/public_key", "GET", true)]
        public ReplyData get_public_key(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();

            Console.WriteLine("====> Request: Get_Public_Key (domain)");

            string pub = Session.Instance.DomainsMem.Itms[arguments[0]].Obj.Public_Key;


            users.users_reply ur = new users.users_reply();
            if (pub == "") ur.status = "fail";
            else ur.status = "success";

            ur.data = new Dictionary<string, string>();
            ur.data.Add("public_key", pub);

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(ur);

            return rd;
        }
        #endregion

        #region Heartbeat

        public struct HeartbeatPacket // Put this inside a Dictionary, key being "domain"
        {
            public string automatic_networking;
            public HeartbeatData heartbeat;
        }

        public struct HeartbeatData
        {
            public int num_anon_users;
            public int num_users;
            public string protocol;
            public bool restricted;
            public string version;
            // user_hostnames   -    Currently not known what this data looks like, however it is probably a Dictionary of some sort
        }


        [APIPath("/api/v1/domains/%", "PUT", true)]
        public ReplyData domain_heartbeat(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            // Check the Authorization header for a valid Access token
            // If token is valid, begin updating stuff
            ReplyData rd = new ReplyData();
            rd.Status = 200;
            rd.Body = "";
            if (Headers.ContainsKey("Authorization"))
            {
                string Token = Headers["Authorization"].Split(new[] { ' ' })[1];
                UserAccounts ua = UserAccounts.GetAccounts();
                foreach (KeyValuePair<string, UserAccounts.Account> kvp in ua.AllAccounts)
                {
                    if (kvp.Value.ActiveTokens.ContainsKey(Token))
                    {
                        // Start updating shit
                        Dictionary<string, HeartbeatPacket> requestData = JsonConvert.DeserializeObject<Dictionary<string, HeartbeatPacket>>(body);

                        DomainMemory mem = Session.Instance.DomainsMem;
                        // Check if this domain is in memory!
                        if (mem.Itms.ContainsKey(arguments[0]))
                        {
                            // start

                            DomainMemory.MemoryItem mi = mem.Itms[arguments[0]];
                            DomainMemory.DomainObject obj = mi.Obj;
                            // First check that there is a API Key
                            if(obj.API_Key == "" || obj.API_Key == null)
                            {
                                rd.Status = 401;
                                break;
                            }
                            obj.NetworkingMode = requestData["domain"].automatic_networking;
                            HeartbeatData dat = requestData["domain"].heartbeat;
                            obj.Protocol = dat.protocol;
                            obj.Restricted = dat.restricted;
                            obj.Version = dat.version;
                            obj.LoggedIn = dat.num_users;
                            obj.Anon = dat.num_anon_users;
                            obj.TotalUsers = dat.num_anon_users + dat.num_users;
                            mi.Obj = obj;

                            Session.Instance.DomainsMem.Itms[arguments[0]] = mi;


                            rd.Status = 200;
                            rd.Body = "";
                            
                        } else
                        {
                            rd.Status = 404; // this will trigger a new temporary domain name
                        }
                    }
                }
            }


            // fallback
            if (Session.Instance.DomainsMem.Itms.ContainsKey(arguments[0]) == false) rd.Status = 404;
            return rd;

        }


        #endregion

    }



    public class DomainMemory // <--- This is to be used to keep only recently accessed domains in memory!
        // Anything accessed within... the last 2 hours should stay in cached memory, after that it should be unloaded
    {
        #region Base Definitions
        private static readonly object _lck = new object();
        public struct DomainObject
        {
            public string DomainID;
            public string PlaceName;
            public string IPAddr;
            public string API_Key;
            public string Public_Key;
            public string Protocol;
            public string Version;
            public bool Restricted;
            public int TotalUsers;
            public int Anon;
            public int LoggedIn;
            public string NetworkingMode;
        }


        public struct MemoryItem
        {
            private DomainObject _obj;
            public DateTime LastAccessed;
            public DomainObject Obj
            {
                get
                {
                    LastAccessed = DateTime.Now;
                    return _obj;
                }
                set
                {
                    _obj = value;
                    LastAccessed = DateTime.Now;
                    SaveItem();
                }
            }

            public void SaveItem()
            {
                lock (_lck)
                {
                    if (!Directory.Exists("domains")) Directory.CreateDirectory("domains");
                    File.WriteAllText("domains/" + Obj.DomainID + ".json", JsonConvert.SerializeObject(Obj, Formatting.Indented));
                }
            }
            
        }


        public Dictionary<string, MemoryItem> Itms = new Dictionary<string, MemoryItem>();
        private void Scan()
        {
            List<string> ToRemove = new List<string>();

            foreach (KeyValuePair<string, MemoryItem> kvp in Itms)
            {
                if (kvp.Value.LastAccessed.AddHours(2) < DateTime.Now)
                {
                    ToRemove.Add(kvp.Key);
                }
            }
            foreach (string rem in ToRemove)
            {
                Itms.Remove(rem);
            }
        }
        public void LoadDomain(string ID, string api_key="")
        {
            if (Directory.Exists("domains"))
            {
                if (File.Exists($"domains/{ID}.json"))
                {
                    lock (_lck)
                    {

                        MemoryItem mi1 = new MemoryItem();
                        DomainObject obj1 = JsonConvert.DeserializeObject<DomainObject>(File.ReadAllText($"domains/{ID}.json"));
                        mi1.Obj = obj1;
                        Itms.Add(obj1.DomainID, mi1);
                        return;
                    }
                }
            }

            MemoryItem mi = new MemoryItem();
            DomainObject obj = new DomainObject
            {
                DomainID = ID
            };
            PlaceNameGenerator pngl = new PlaceNameGenerator();
            obj.PlaceName = pngl.GenerateRandomPlaceName()+"-"+pngl.GenerateRandomPlaceName()+"-"+(new Random()).Next(500, 8000).ToString(); //<-- Does this need to be changed in the future??
            obj.IPAddr = Session.Instance.CFG.DefaultIceServerAddress;
            obj.API_Key = api_key;
            mi.Obj = obj;

            Itms.Add(obj.DomainID, mi);
        }
        #endregion

        #region Set Info
        public bool SetPlaceName(string DomainID, string PlaceName, string APIKey="")
        {
            Scan();

            if (Itms.ContainsKey(DomainID))
            {
                MemoryItem mi = Itms[DomainID];
                DomainObject obj = mi.Obj;
                if (obj.API_Key != APIKey) return false;
                obj.PlaceName = PlaceName;
                mi.Obj = obj;
                Itms[DomainID] = mi;
                return true;
            }
            else
            {
                // Load to memory
                LoadDomain(DomainID);
                return SetPlaceName(DomainID, PlaceName);
                
            }
        }


        public bool SetIP(string DomainID, string IP, string APIKey="")
        {
            Scan();

            if (Itms.ContainsKey(DomainID))
            {
                MemoryItem mi = Itms[DomainID];
                DomainObject obj = mi.Obj;
                if (obj.API_Key != APIKey) return false;
                obj.IPAddr = IP;
                mi.Obj = obj;
                Itms[DomainID] = mi;
                return true;
            }
            else
            {
                LoadDomain(DomainID);
                return SetIP(DomainID, IP);
            }
        }

        public bool SetPublicKey(string IP, string Key, string ID)
        {
            Scan();

            if (Itms.ContainsKey(ID))
            {
                MemoryItem mi = Itms[ID];
                DomainObject obj = mi.Obj;
                if(obj.IPAddr == IP)
                {
                    obj.Public_Key = Key;
                    mi.Obj = obj;
                    Itms[ID] = mi;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                LoadDomain(ID);
                return SetPublicKey(IP, Key, ID);
            }
        }

        #endregion
    }
}
