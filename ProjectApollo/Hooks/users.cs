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
using Project_Apollo.Registry;
using static Project_Apollo.Registry.APIRegistry;
using System.Security.Cryptography;


namespace Project_Apollo.Hooks
{
    public class users
    {
        private static readonly string _logHeader = "[User]";

        #region Create User
        public struct users_request
        {
            public Dictionary<string, string> user;
        }
        public struct users_reply
        {
            public string status;
            public Dictionary<string, string> data;
        }
        [APIPath("/api/v1/users", "POST", true)]
        public ReplyData user_create(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {

            ReplyData data = new ReplyData();
            users_request usreq = new users_request();
            // This specific endpoint only creates a user
            try
            {
                usreq = JsonConvert.DeserializeObject<users_request>(body);
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} Malformed reception: {1}", _logHeader, e.ToString());
                throw new NotImplementedException(); // if this fails then the request is malformed!
            }
            UserAccounts UA = UserAccounts.GetAccounts();
            UA.CreateAccount(usreq.user["username"], usreq.user["email"], usreq.user["password"]);
            if (UA.GetStatus())
            {
                // success return nothing
                data.Status = 200;
                data.Body = "";
                return data;
            }
            else
            {
                data.Status = 200;
                users_reply ur = new users_reply();
                ur.status = "fail";
                ur.data = new Dictionary<string, string>();
                ur.data.Add("username", "already exists!");
                data.Body = JsonConvert.SerializeObject(ur);
                return data;
            }

        }
        #endregion

        #region /api/v1/user/locker
        [APIPath("/api/v1/user/locker", "POST", true)]
        public ReplyData user_locker_set(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            UserAccounts UA = UserAccounts.GetAccounts();
            string AccessToken = Headers["Authorization"].Split(new[] { ' ' })[1];

            UA.SetAccountSettings(AccessToken, body);

            users_reply ur = new users_reply();
            ur.status = "success";
            ur.data = new Dictionary<string, string>();
            ReplyData rd = new ReplyData();
            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(ur);
            return rd;
        }

        public struct replyPacket
        {
            public string status;
            public string data;
        }

        [APIPath("/api/v1/user/locker", "GET", true)]
        public ReplyData user_locker_get(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            replyPacket not_found = new replyPacket();
            not_found.status = "fail";
            not_found.data = "";
            ReplyData rd = new ReplyData();
            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(not_found);
            string[] authHead = Headers["Authorization"].Split(new[] { ' ' });
            if (authHead.Length == 1) return rd;
            string AccessToken = Headers["Authorization"].Split(new[] { ' ' })[1];

            UserAccounts UA = UserAccounts.GetAccounts();
            string settings = UA.GetAccountSettings(AccessToken);


            replyPacket rp = new replyPacket();
            rp.status = "success";
            rp.data = settings;

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(rp);
            return rd;
        }


        #endregion

        #region User Location
        public struct LocationPacket
        {
            public string status;
            public UserAccounts.Location location;
        }
        [APIPath("/api/v1/user/location", "PUT", true)]
        public ReplyData user_location_set(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            LocationPacket loc = JsonConvert.DeserializeObject<LocationPacket>(body);
            string AccessToken = Headers["Authorization"].Split(new[] { ' ' })[1];

            UserAccounts UA = UserAccounts.GetAccounts();
            UA.UpdateLocation(loc.location, AccessToken);

            replyPacket rp = new replyPacket();
            rp.status = "success";
            rp.data = "no error";

            ReplyData rd = new ReplyData();
            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(rp);
            return rd;
        }


        [APIPath("/api/v1/users/%/location", "GET", true)]
        public ReplyData get_location(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();
            Console.WriteLine("====> Request: Get_Location");

            UserAccounts UA = UserAccounts.GetAccounts();
            UserAccounts.Location loc = UA.GetLocation(arguments[0]);

            LocationPacket lp = new LocationPacket();
            if (loc.network_address != "")
            {

                lp.status = "success";
                lp.location = loc;
            }
            else
            {
                lp.status = "user has no location";

            }

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(lp);
            return rd;

        }

        #endregion

        #region User Profile
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
        public ReplyData user_profile_gen(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();
            rd.Status = 200;


            UserProfile up = new UserProfile(UserAccounts.GetAccounts().GetAccountName(Headers["Authorization"].Split(new[] { ' ' })[1]));
            user_profile_reply upr = new user_profile_reply();
            upr.status = "success";
            upr.data = new Dictionary<string, UserProfile>();
            upr.data.Add("user", up);

            rd.Body = JsonConvert.SerializeObject(upr);
            return rd;
        }
        #endregion

        #region Public Key
        [APIPath("/api/v1/user/public_key", "PUT", true)]

        public ReplyData set_public_key(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            ReplyData rd = new ReplyData();
            rd.Status = 404;
            rd.Body = "{'status':'fail'}";
            if (Headers.ContainsKey("Authorization") == false) return rd;
            string[] Lines = body.Split(new[] { '\n' });

            string Data = "";

            int index=0;
            foreach(string S in Lines)
            {
                if (index > 3 && S.IndexOf("boundary")==-1)
                {
                    Data += S + "\n";
                }
                index++;
            }

            UserAccounts UA = UserAccounts.GetAccounts();

            replyPacket rp = new replyPacket();
            rp.status = UA.SetPublicKey(Data, Headers["Authorization"].Split(new[] { ' ' })[1]);
            rp.data = "no error";

            
            if (rp.status == "fail") rd.Status = 401;
            else
                rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(rp);
            return rd;


        }

        // TODO: CHANGE TO REGEX
        [APIPath("/api/v1/users/%/public_key", "GET", true)]
        public ReplyData get_public_key(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string,string> Headers)
        {
            ReplyData rd = new ReplyData();

            Console.WriteLine("====> Request: Get_Public_Key");

            UserAccounts UA = UserAccounts.GetAccounts();
            string pub=UA.GetPublicKey(arguments[0]);

            users_reply ur = new users_reply();
            if (pub == "no such users") ur.status = "fail";
            else ur.status = "success";

            ur.data = new Dictionary<string, string>();
            ur.data.Add("public_key", pub);

            rd.Status = 200;
            rd.Body = JsonConvert.SerializeObject(ur);

            return rd;
        }
        #endregion

        #region User Tokens

        [APIPath("/user/tokens/new", "GET", true)]
        public ReplyData user_tokens(IPAddress remoteIP, int remotePort, List<string> arguments, string body, string method, Dictionary<string, string> Headers)
        {
            if(Headers.ContainsKey("Authorization") == false)
            {

                ReplyData rd = new ReplyData();
                rd.Status = 401;
                rd.Body = "";
                Session.Instance.TemporaryStackData.Add(remoteIP.ToString());
                rd.CustomOutputHeaders.Add("WWW-Authenticate", "Basic realm='Tokens'");
                rd.Body = "<h2>You are not logged in!";
                rd.CustomOutputHeaders.Add("Content-Type", "text/html");
                return rd;
            } else
            {
                // Validate login!
                string[] req = arguments[0].Split(new[] { '?', '&', '=' });
                string[] authHeader = Headers["Authorization"].Split(new[] { ' ' });

                if(authHeader[0] == "Basic" && Session.Instance.TemporaryStackData.Contains(remoteIP.ToString()))
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

                                Session.Instance.TemporaryStackData.Remove(remoteIP.ToString());
                                ReplyData rd1 = new ReplyData();
                                rd1.Status = 200;
                                rd1.Body = $"<center><h2>Your domain's access token is: {Token}</h2></center>";
                                rd1.CustomOutputHeaders.Add("Content-Type", "text/html");

                                return rd1;
                            }
                        }
                    }
                }

                ReplyData rd = new ReplyData();
                rd.Body = "Invalid authorization header was provided!<br/>If you were not prompted for credentials again, close the tab or the browser and try again";
                rd.Status = 401;
                if (Session.Instance.TemporaryStackData.Contains(remoteIP.ToString()) == false) Session.Instance.TemporaryStackData.Add(remoteIP.ToString());
                rd.CustomOutputHeaders.Add("WWW-Authenticate", "Basic realm='Tokens'");
                return rd;
            }
        }

        #endregion

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
        public ReplyData user_login(IPAddress remoteIP, int remotePort, List<string> args, string body, string method, Dictionary<string,string> Headers)
        {
            Console.WriteLine("====> Starting User Login <=====");

            ReplyData data = new ReplyData();
            Login_Reply lr = new Login_Reply();
            Dictionary<string, string> arguments = Tools.PostBody2Dict(body);
            lr.created_at = Tools.getTimestamp();
            lr.expires_in = 1 * 24 * 60 * 60; // 1 DAY
            lr.token_type = "Bearer";
            lr.scope = arguments["scope"];
            lr.refresh_token = Tools.SHA256Hash(lr.expires_in.ToString() + ";" + remoteIP.ToString() + ";" + arguments["username"]);
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
                users.users_reply failreply = new users.users_reply();
                failreply.status = "fail";
                failreply.data = new Dictionary<string, string>();
                failreply.data.Add("reason", "Unknown");
                data.Status = 200;
                data.Body = JsonConvert.SerializeObject(failreply);
                return data;
            }

        }

        
    }

    public class UserAccounts
    {
        private static readonly object _lock = new object();
        public static UserAccounts GetAccounts()
        {
            lock (_lock)
            {

                if (!File.Exists("accounts.json"))
                {
                    UserAccounts ua = new UserAccounts();
                    return ua;
                }
                string js = File.ReadAllText("accounts.json");
                return (UserAccounts)JsonConvert.DeserializeObject<UserAccounts>(js);
            }
        }

        public struct Location
        {
            public bool connected;
            public string network_address;
            public string availability;
            public int network_port;
            public string node_id;
            public string domain_id;
            public string path;
        }

        public class Account
        {
            public string name;
            public string email;
            public string pwSalt;
            public string pwHash;
            public string account_settings;
            public string PubKey;
            public Location LastLocation;
            public Dictionary<string, string> ActiveTokens;
            public void GenAccount(string A, string B, string C)
            {
                name = A;
                email = B;
                pwSalt = Tools.SHA256Hash(Tools.getTimestamp().ToString() + ";" + (new Random().Next(99999999)).ToString());
                pwHash = Tools.MD5Hash(Tools.MD5Hash(C) + ":" + Tools.MD5Hash(pwSalt));
            }
            public Account()
            {
                ActiveTokens = new Dictionary<string, string>();
            }
        }
        private bool LastStatus;
        public bool GetStatus()
        {
            return LastStatus;
        }
        public Dictionary<string, Account> AllAccounts = new Dictionary<string, Account>();
        public void CreateAccount(string name,string email, string password)
        {
            Account a = new Account();
            a.GenAccount(name, email, password);
            LastStatus = true;
            if (!AllAccounts.ContainsKey(name))
                AllAccounts.Add(name, a);
            else
                LastStatus = false; // set to false IF the creation fails!

            save();
        }


        public bool Login(string name, string password, string access_token)
        {

            if (AllAccounts.ContainsKey(name))
            {
                Account act = AllAccounts[name];

                if (Tools.MD5Hash(Tools.MD5Hash(password) + ":" + Tools.MD5Hash(act.pwSalt)) == act.pwHash)
                {
                    if (access_token != "web")
                    {

                        if (act.ActiveTokens.ContainsKey(access_token))
                            act.ActiveTokens[access_token] = "owner";
                        else
                            act.ActiveTokens.Add(access_token, "owner");
                    }
                    Console.WriteLine("====> New Access Token: " + name + "; " + access_token);
                    AllAccounts[name] = act;
                    save();
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public string GetAccountSettings(string AccessToken)
        {
            Account a = new Account();
            int i = 0;
            foreach (string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if(a.ActiveTokens.ContainsKey(AccessToken))
                {
                    return Tools.Base64Decode(a.account_settings);
                }
            }

            return "";
        }

        public void SetAccountSettings(string AccessToken, string AccountSettings)
        {
            Account a = new Account();
            foreach(string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    a.account_settings = Tools.Base64Encode(AccountSettings);
                    AllAccounts[user] = a;
                    save();
                    return;
                }
            }
        }
        public void UpdateLocation(Location loc, string AccessToken)
        {
            Account a = new Account();
            foreach(string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if(a.ActiveTokens.ContainsKey(AccessToken))
                {
                    a.LastLocation = loc;
                    AllAccounts[user] = a;
                    save();
                    return;
                }
            }
        }

        public Location GetLocation(string user)
        {
            if (AllAccounts.ContainsKey(user))
            {
                return AllAccounts[user].LastLocation;
            }
            else return new Location();
        }

        public string GetAccountName(string AccessToken)
        {
            Account a = new Account();
            foreach(string user in AllAccounts.Keys)
            {
                a = AllAccounts[user];
                if(a.ActiveTokens.ContainsKey(AccessToken))
                {
                    return user;
                }
            }
            return "not_found";
        }

        public string SetPublicKey(string PubKey, string AccessToken)
        {
            Account a = new Account();
            foreach(string key in AllAccounts.Keys)
            {
                a = AllAccounts[key];

                if (a.ActiveTokens.ContainsKey(AccessToken))
                {
                    ///////////////////
                    //// TODO: CHECK WHAT ENCODING IS EXPECTED BY THE CLIENT
                    //// 2: CHANGE EXPECTED POST HANDLING, I HATE DEALING WITH OCTET STREAMS EVEN ON A PHP SERVER!
                    ///
                    /// SAMPLE REPLY FROM ORIGINAL METAVERSE SERVER
                    /// {"status":"success","data":{"public_key":"MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA3KTcsZZ1qLbtFAeP\njksyoTGk6cU/kUDf98Vo1otWFI0erVmJI0Sf4Ukv1fIMlIRoke8k2fIyefAt\nCJ2cDvNkEOPCgV0ibcOx/P/sEdlSBm8Ke+Sk5Ov3TQyFlsOPRrtL7mJtnVpd\nqoxc+Arsi3UIuOp0YhyLm5HFyaQQr4gnKrPb7jqjBag7BGlb59zH7cc2X7Fr\nTm6DR7bhWG8XGZZ5yT6SjZjDq93vhOTBr5tMc8g/wg/irPsgHMf+SEpSZzRK\nJXuEXE4Jn6SHAFJ0emhLfIIpeXHEDHtFDJyhFt0HeVWFW9QqylxY+cTN/y6L\n1/7EPoWIwFFQQUxRTdNTKKnVKQIDAQAB\n"}}
                    /// 
                    /// 
                    a.PubKey = Tools.Base64Encode(PubKey);
                    AllAccounts[key] = a;
                    save();
                    return "success";
                }
            }
            return "fail";
        }

        public string GetPublicKey(string user)
        {
            if (AllAccounts.ContainsKey(user)) return AllAccounts[user].PubKey;
            return "no such user";
        }

        public void save()
        {
            File.WriteAllText("accounts.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
