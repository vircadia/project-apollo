using Newtonsoft.Json;
using Project_Apollo.Registry;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static Project_Apollo.Registry.APIRegistry;

namespace Project_Apollo
{
    class Program
    {
        public static ManualResetEvent MRE = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Session sess = Session.Instance;
            HttpListener listener = null;
            Console.WriteLine("WELCOME TO PROJECT APOLLO");
            try
            {
                Console.WriteLine("=> STARTUP");
                listener = new HttpListener();
                listener.Prefixes.Add("http://*:9400/");
                listener.Prefixes.Add("http://*:9401/");
                listener.Start();
                listener.BeginGetContext(OnWeb, null);
            } catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("\n" + e.StackTrace);
            }
            MRE.Reset();
            Console.WriteLine("=> WAITING FOR REQUESTS");
            sess.ProductionListen = listener;
            sess.Registry = APIRegistry.Instance; // set !
            MRE.WaitOne();

            listener.Stop();

            return;
        }

        private static void OnWeb(IAsyncResult ar)
        {
            try
            {

                HttpListenerContext ctx = null;
                try
                {
                    ctx = Session.Instance.ProductionListen.EndGetContext(ar);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error getting context\n"+e.StackTrace+"\n"+e.Message);
                }

                Session.Instance.ProductionListen.BeginGetContext(OnWeb, null);

                Stream body = ctx.Request.InputStream;

                StreamReader sr = new StreamReader(body, ctx.Request.ContentEncoding);
                string resp = sr.ReadToEnd();

                if (resp == "END")
                {
                    MRE.Set();
                }

                string tmp = "";
                if(ctx.Request.ContentType != "application/json" && ctx.Request.ContentType != "text/html")
                {
                    tmp = resp;
                    resp = "*not displaying binary data to console!";
                }
                string CONSOLEPRINT = (new string('=', 10) + "\nBEGIN PROD REQUEST\n" + new string('=', 10) + "\n\nCONTENT TYPE: " + ctx.Request.ContentType + "\nMETHOD: " + ctx.Request.HttpMethod + "\nURL: " + ctx.Request.RawUrl + "\n\nREQUEST BODY: " + resp + "\nPORT: " + ctx.Request.LocalEndPoint.Port.ToString() + "\nUSER-AGENT: " + ctx.Request.UserAgent + "\nHeaders: \n"+JsonConvert.SerializeObject(Tools.NVC2Dict(ctx.Request.Headers), Formatting.Indented)+"\n\n" + new string('=', 10) + "\nEND PROD REQUEST\n" + new string('=', 10));
                if(tmp!="")
                    resp = tmp;
                tmp = "";
                string STR = "";
                Console.WriteLine(STR);
                STR = (new string('=', 10) + "\nBEGIN PROD REQUEST\n" + new string('=', 10) + "\n\nCONTENT TYPE: " + ctx.Request.ContentType + "\nMETHOD: " + ctx.Request.HttpMethod + "\nURL: " + ctx.Request.RawUrl + "\n\nREQUEST BODY: " + resp + "\nPORT: " + ctx.Request.LocalEndPoint.Port.ToString() + "\nUSER-AGENT: " + ctx.Request.UserAgent + "\nHeaders: \n" + JsonConvert.SerializeObject(Tools.NVC2Dict(ctx.Request.Headers), Formatting.Indented) + "\n\n" + new string('=', 10) + "\nEND PROD REQUEST\n" + new string('=', 10));

                File.AppendAllText("RequestLog.txt", "\n\n"+STR+"\n\n");
                sr.Close();
                body.Close();

                
                ReplyData _reply = APIRegistry.Instance.ProcessInbound(resp, ctx.Request.RawUrl, ctx.Request.HttpMethod, ctx.Request.RemoteEndPoint.Address, ctx.Request.RemoteEndPoint.Port, Tools.NVC2Dict(ctx.Request.Headers), CONSOLEPRINT);

                byte[] buffer = Encoding.UTF8.GetBytes("\n"+_reply.Body);
                ctx.Response.ContentLength64 = buffer.Length;
                ctx.Response.AddHeader("Server", "1.5");
                ctx.Response.StatusCode = _reply.Status;
                Stream output = ctx.Response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace + "\n\n" + e.Message);
            }
        }
    }

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
            byte[] Source = UTF8Encoding.UTF8.GetBytes(ToHash);
            byte[] Hash = new MD5CryptoServiceProvider().ComputeHash(Source);
            return Tools.Hash2String(Hash);
        }

        public static string MD5Hash(byte[] ToHash)
        {
            return Tools.Hash2String(new MD5CryptoServiceProvider().ComputeHash(ToHash));
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
