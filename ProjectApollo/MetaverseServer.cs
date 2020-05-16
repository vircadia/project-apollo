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
using System.Data;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

using Project_Apollo.Logging;
using Project_Apollo.Configuration;
using Project_Apollo.Registry;

using static Project_Apollo.Registry.APIRegistry;
using System.Data.Common;
using NUnit.Framework.Constraints;

namespace Project_Apollo
{
    /// <summary>
    /// 'Context' is a global container of useful variables.
    /// A CleanDesign would pass these around to the necessary code
    ///    but, for the moment, it is easier if these read-only
    ///    instance things are global.
    /// </summary>
    public class Context
    {
        static public AppParams Params;
        static public Logger Log;

        // If cancelled, everything shuts down
        static public CancellationTokenSource KeepRunning;

        // The HTTP listener that's waiting for requests
        static public HttpListener Listener;

        // The database we're talking to
        static public DbConnection Db;

        // All the request path handles are registered in the registry
        static public APIRegistry PathRegistry;
    }
    /// <summary>
    /// MetaverseServer entry.
    /// This sets up the global context, fetches parameters (from
    ///     command line and config file), connects to the database,
    ///     and starts the HttpListener. It then waits until something
    ///     happens from one of the REST requests that tells us to
    ///     stop
    /// </summary>
    class MetaverseServer
    {
        private static readonly string _logHeader = "[MetaverseServer]";

        static void Main(string[] args)
        {

            Console.WriteLine("WELCOME TO PROJECT APOLLO METAVERSE API SERVER");

            Context.Params = new AppParams(args);
            Context.Log = new Logger(Context.Params.P<string>("MetaverseServer.LogDirectory"));
            Context.Log.SetLogLevel(Context.Params.P<string>("LogLevel"));

            Context.KeepRunning = new CancellationTokenSource();

            MetaverseServer server = new MetaverseServer();
            server.Start();
            return;
        }

        private void Start()
        {
            // Collect all the HTTP request path handlers in the registry
            Context.PathRegistry = new APIRegistry();

            try
            {
                Context.Db = ConnectToDatabase();
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} Exception connecting to database: {1}", _logHeader, e.ToString());
                return;
            }

            try
            {
                Context.Listener = StartHttpListener();
            }
            catch (Exception e)
            {
                Context.Log.Error("{0} Exception starting HttpListener: {1}", _logHeader, e.ToString());
                return;
            }

            while (!Context.KeepRunning.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }

            this.Stop();

            return;
        }

        private void Stop()
        {
            if (Context.Db != null)
            {
                Context.Db.Close();
                Context.Db = null;
            }
            if (Context.Listener != null)
            {
                Context.Listener.Stop();
                Context.Listener = null;
            }
        }

        // Connect and initialize database
        private DbConnection ConnectToDatabase()
        {
            // No database yet
            return null;
        }

        /// <summary>
        /// Set up the HttpListener on the proper port.
        /// When a request is received, 'OnWeb' will be called to search
        /// for the APIPath for the request's path.
        /// </summary>
        /// <returns>the created HttpListener</returns>
        private HttpListener StartHttpListener()
        {
            HttpListener listener = null;

            // Let any exceptions be caught by the caller
            Context.Log.Debug("{0} Creating HttpListener", _logHeader);
            listener = new HttpListener();

            string prefix = String.Format("http://{0}:{1}/",
                            Context.Params.P<string>("Listener.Host"),
                            Context.Params.P<string>("Listener.Port"));
            Context.Log.Debug("{0} HttpListener listening on '{1}", _logHeader, prefix);
            listener.Prefixes.Add(prefix);

            listener.Start();
            // Call 'OnWeb' when request received
            listener.BeginGetContext(OnWeb, null);

            return listener;
        }

        /// <summary>
        /// Do the async request processing.
        /// </summary>
        /// <param name="ar"></param>
        private static void OnWeb(IAsyncResult ar)
        {
            try
            {
                HttpListenerContext ctx = null;
                try
                {
                    ctx = Context.Listener.EndGetContext(ar);
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Error getting context\n{1}\n{2}",
                                _logHeader, e.StackTrace, e.Message);
                    //TODO: should some sort of response be created?
                    return;
                }

                Context.Listener.BeginGetContext(OnWeb, null);

                string reqBody;
                using (Stream body = ctx.Request.InputStream)
                {
                    using (StreamReader sr = new StreamReader(body, ctx.Request.ContentEncoding))
                    {
                        reqBody = sr.ReadToEnd();
                    }
                }

                // TODO: figure out if this is from the HTTPListener as I don't want
                //    just anyone sending a message to shut us down
                if (reqBody == "END")
                {
                    Context.KeepRunning.Cancel();
                    return;
                }

                if (ctx.Request.ContentType == "application/json" || ctx.Request.ContentType == "text/html")
                {
                    Context.Log.Debug("{0} received: {1}", _logHeader, reqBody);

                    ReplyData _reply = Context.PathRegistry.ProcessInbound(reqBody,
                                ctx.Request.RawUrl,
                                ctx.Request.HttpMethod,
                                ctx.Request.RemoteEndPoint.Address,
                                ctx.Request.RemoteEndPoint.Port,
                                Tools.NVC2Dict(ctx.Request.Headers));

                    byte[] buffer = Encoding.UTF8.GetBytes("\n"+_reply.Body);
                    ctx.Response.ContentLength64 = buffer.Length;
                    ctx.Response.Headers.Add("Server", "1.5");
                    
                    ctx.Response.StatusCode = _reply.Status;
                    if (_reply.CustomStatus != null) ctx.Response.StatusDescription = _reply.CustomStatus;
                    if(_reply.CustomOutputHeaders != null)
                    {
                        ctx.Response.ContentType = "application/json";
                        
                        foreach(KeyValuePair<string,string> kvp in _reply.CustomOutputHeaders)
                        {
                            ctx.Response.Headers[kvp.Key] = kvp.Value;
                        }
                    }
                    using (Stream output = ctx.Response.OutputStream)
                    {
                        output.Write(buffer, 0, buffer.Length);
                    }
                    ctx.Response.Close();
                }
                else
                {
                    // Got a content type we don't handle
                }
                
                
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
