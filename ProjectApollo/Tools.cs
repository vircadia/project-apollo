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
using System.Security.Cryptography;
using System.Text;

namespace Project_Apollo
{
    public class Tools
    {
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
    }
}
