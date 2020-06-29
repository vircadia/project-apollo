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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Project_Apollo
{
    public class Tools
    {
        private static readonly string _logHeader = "[Tools]";

        public static Int32 getTimestamp()
        {
            return int.Parse(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        public static string Hash2String(byte[] Hash)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in Hash)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public static string MD5Hash(string ToHash)
        {
            MD5 md = MD5.Create();
            
            byte[] Source = UTF8Encoding.UTF8.GetBytes(ToHash);
            byte[] Hash = md.ComputeHash(Source);
            return Tools.Hash2String(Hash);
        }

        public static string MD5Hash(byte[] ToHash)
        {
            MD5 md = MD5.Create();
            return Tools.Hash2String(md.ComputeHash(ToHash));
        }

        public static string SHA256Hash(string ToHash)
        {
            SHA256 hasher = SHA256.Create();
            return Tools.Hash2String(hasher.ComputeHash(UTF8Encoding.UTF8.GetBytes(ToHash)));
        }

        public static string SHA256Hash(byte[] ToHash)
        {
            SHA256 Hasher = SHA256.Create();
            return Tools.Hash2String(Hasher.ComputeHash(ToHash));
        }

        // For unknown reasons, public keys are sent to the metaverse server
        //    in RSA PKCS#1 format but the keys are sent out as
        //    Base64 converted SubjectPublicKeyInfo formatted keys.
        // This takes a stream of bytes of a PKCS#1 binary formatted key
        //    and returns the Base64 string.
        public static string ConvertPublicKeyStreamToBase64(Stream pKeyStream)
        {
            using var memStream = new MemoryStream();
            pKeyStream.CopyTo(memStream);
            var publicKey = RSA.Create();
            publicKey.ImportRSAPublicKey(memStream.ToArray(), out int bytesRead);
            byte[] outPublicKey = publicKey.ExportSubjectPublicKeyInfo();
            return Convert.ToBase64String(outPublicKey);
        }
        
        public static Dictionary<string,string> PostBody2Dict(string body)
        {
            Dictionary<string, string> ReplyData = new Dictionary<string, string>();
            string[] args = body.Split(new[] { '?', '&' });
            foreach(string arg in args)
            {
                string[] kvp = arg.Split(new[] { '=' });

                ReplyData.Add(kvp[0], kvp[1]);
            }

            return ReplyData;
        }

        public static Dictionary<string,string> NVC2Dict(NameValueCollection nvc)
        {
            Dictionary<string, string> reply = new Dictionary<string, string>();

            foreach(var k in nvc.AllKeys)
            {
                reply.Add(k, nvc[k]);
            }

            return reply;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            if(base64EncodedData==null)return "";
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static int Clamp(int pIn, int pMin, int pMax)
        {
            int ret = pIn;
            if (ret < pMin) ret = pMin;
            if (ret > pMax) ret = pMax;
            return ret;
        }

        // Return a string of the passed length of random digits.
        // Note that this is not crypographically random.
        public static string RandomString(int pLen)
        {
            int len = Clamp(pLen, 2, 128);
            string digits = "0123456789";
            var rand = new Random();
            return String.Join("", Enumerable.Range(0, len).Select(ii =>
            {
               return digits[rand.Next(0,10)];
            }) );
        }

        /// <summary>
        /// Get my external IP address.
        /// Since this process could be NATed, we use an external service to
        /// get the address. If that fails, we try to get our interface address.
        /// </summary>
        /// <returns>IP address as a string. Could be IPv4 or IPv6. 'null' if nothing can be figured out</returns>
        public static async Task<string> GetMyExternalIPAddress()
        {
            string retIPAddr = null;
            try
            {
                HttpWebRequest hwr = HttpWebRequest.CreateHttp("https://api.ipify.org");
                WebResponse resp = await hwr.GetResponseAsync();
                Stream strm = resp.GetResponseStream();
                StreamReader sr = new StreamReader(strm);
                retIPAddr = sr.ReadToEnd();
                Context.Log.Debug("{0} Fetched external IP address is {1}", _logHeader, retIPAddr);
            }
            catch (Exception e)
            {
                Context.Log.Debug("{0} Exception getting external IP address: {1}", _logHeader, e);
                retIPAddr = null;
            }

            if (retIPAddr == null)
            {
                // If the external fetch failed, get our interface address.
                // Look for the first IP address that is Ethernet, up, and not virtual or loopback.
                // Cribbed from https://stackoverflow.com/questions/6803073/get-local-ip-address
                retIPAddr = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                            && x.OperationalStatus == OperationalStatus.Up
                            && !x.Description.ToLower().Contains("virtual")
                            && !x.Description.ToLower().Contains("pseudo")
                    )
                    .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork
                            && !IPAddress.IsLoopback(x.Address)
                    )
                    .Select(x => x.Address.ToString())
                    .First();
                Context.Log.Debug("{0} Computed external IP address is {1}", _logHeader, retIPAddr);
            }

            return retIPAddr;
        }

        // JSON tools
        // See if the item is specified in the JObject and, if so, set the destination location.
        public static void SetIfSpecified<T>(JObject pSpecs, string pItem, ref T pDest)
        {
            JToken maybe = pSpecs[pItem];
            if (maybe != null)
            {
                pDest = maybe.ToObject<T>();
                // Context.Log.Debug("{0} SetIfSpecified: Setting {1}={2}", _logHeader, pItem, pDest);
            }
            else
            {
                // Context.Log.Debug("{0} SetIfSpecified: Not setting {1}", _logHeader, pItem);
            }
        }
    }
}
