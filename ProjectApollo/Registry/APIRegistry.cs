using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace Project_Apollo.Registry
{
    public sealed class APIRegistry
    {
        // BEGIN SINGLETON!

        private static readonly object _lock = new object();
        private static APIRegistry _inst;
        static APIRegistry () { }
        public static APIRegistry Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_inst == null)
                    {
                        _inst = new APIRegistry();
                        _inst.LocateHooks();

                    }
                    return _inst;
                }
            }
        }

        // END SINGLETON INVOKE!

        public List<APIPath> _discovered;
        public void LocateHooks()
        {
            _discovered = new List<APIPath>(); // Always reset this list at the start of this method!
            try
            {
                int i = 0;
                for (i = 0; i < AppDomain.CurrentDomain.GetAssemblies().Length; i++)
                {
                    Assembly asm = null;

                    try
                    {
                        asm = AppDomain.CurrentDomain.GetAssemblies()[i];
                    } catch(Exception e)
                    {
                        // nothing needs be done here
                    }

                    if(asm != null)
                    {
                        int ii = 0;
                        for (ii = 0; ii < asm.GetTypes().Length; ii++)
                        {
                            Type _type = null;
                            try
                            {
                                _type = asm.GetTypes()[ii];
                            }catch(Exception E) { }
                            if(_type != null)
                            {
                                if (_type.IsClass)
                                {
                                    foreach(MethodInfo MI in _type.GetMethods())
                                    {
                                        APIPath[] paths = (APIPath[])MI.GetCustomAttributes(typeof(APIPath), true);

                                        int ix = 0;
                                        for(ix = 0; ix<paths.Length; ix++)
                                        {
                                            APIPath _path = paths[ix];
                                            _path.AssignedMethod = MI;
                                            _discovered.Add(_path);

                                            Console.WriteLine("Discovered: " + _path.PathLike + "; " + MI.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }catch(Exception e)
            {

            }
        }

        public class ReplyData
        {
            public ReplyData()
            {
                CustomOutputHeaders = new Dictionary<string, string>();
            }
            public string Body;
            public int Status;
            public string CustomStatus; // <-- Examples: OK, Not Found, Authorization Required, etc.
            public Dictionary<string, string> CustomOutputHeaders;
        }

        public ReplyData ProcessInbound(string requestBody, string rawURL, string method, IPAddress remoteUser, int remotePort, Dictionary<string,string> headers, string consoleoutput)
        {
            ReplyData _ReplyData = new ReplyData();
            _ReplyData.Status = 418;
            
            Dictionary<string, string> notFoundDefault = new Dictionary<string, string>();
            notFoundDefault.Add("status", "not_found");
            notFoundDefault.Add("data", "Needs more water!"); // joke... See 418 status code :P
            string notFoundDef = JsonConvert.SerializeObject(notFoundDefault);
            _ReplyData.Body = notFoundDef;
            foreach(APIPath zAPIPath in _discovered)
            {
                // compare strings; If a % symbol is located, then skip that so long as the inbound string matches totally.
                // Append the value of % in the inbound request to the array passed to the function
                List<string> arguments = new List<string>();
                string sCheck = zAPIPath.PathLike;
                bool Found = true; // Default to true
                if (method != zAPIPath.HTTPMethod) Found = false;
                if(rawURL.IndexOf('?')!=-1 && !zAPIPath.AllowArgument) { 
                }
                else
                {

                    string[] aCheck = sCheck.Split(new[] { '/' });
                    string[] actualRequest = rawURL.Split(new[] { '/','?' }); // if it contains a ?, we'll put that into the GETBody
                    string theArgs = "";

                    if (rawURL.Contains('?'))
                    {
                        // adjust the ActualRequest list to not contain the argument value, IF the allow arguments flag is set
                        if (zAPIPath.AllowArgument)
                        {
                            // continue
                            string[] tmp1 = rawURL.Split(new[] { '?' });
                            theArgs = tmp1[1];
                            actualRequest = tmp1[0].Split(new[] { '/' });
                        }
                    }
                    if (actualRequest.Length == aCheck.Length)
                    {

                        int i = 0;

                        for (i = 0; i < aCheck.Length; i++)
                        {
                            // TODO: CHANGE THIS SLOPPY MESS TO REGEX.. FOR NOW IT WORKS!
                            if (aCheck[i] == "%")
                            {
                                arguments.Add(actualRequest[i]);
                            }
                            else
                            {

                                if (aCheck[i] == actualRequest[i])
                                {
                                    // we're good!

                                }
                                else
                                {
                                    // check other path hooks before returning 404!
                                    Found = false;
                                }
                            }
                        }
                    }
                    else Found = false;

                    arguments.Add(theArgs);
                }

                if (Found)
                {
                    // Run the method
                    Console.WriteLine("Running: " + zAPIPath.PathLike + "; " + zAPIPath.AssignedMethod.Name+"; For inbound: "+rawURL);
                    object _method = Activator.CreateInstance(zAPIPath.AssignedMethod.DeclaringType);
                    _ReplyData = (ReplyData)zAPIPath.AssignedMethod.Invoke(_method, new object[] { remoteUser, remotePort, arguments, requestBody, method, headers });

                    Console.WriteLine("====> " + _ReplyData.Body);
                    
                    return _ReplyData;
                }
            }
            // an API Path wasn't found
            // check the filesystem
            string[] noArgPath = rawURL.Split(new[] { '?' });
            if (File.Exists($"htdocs/{noArgPath[0]}"))  // This will provide a way to display HTML to the user. If the server must process data internally, please use a method & attribute. Nothing is stopping you from also loading in a HTML/js file and returning a stylized response.
            {
                _ReplyData.Status = 200;
                _ReplyData.Body = File.ReadAllText($"htdocs/{noArgPath[0]}");
                Dictionary<string, string> customHeaders = null; // This is mainly going to be used in instances where the domain-server needs a document but CORS isnt set
                if (File.Exists($"htdocs/{noArgPath[0]}.headers"))
                    customHeaders = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText($"htdocs/{noArgPath[0]}.headers"));

                if (customHeaders != null)
                    _ReplyData.CustomOutputHeaders = customHeaders;


            }
            Console.WriteLine(consoleoutput); // <--- We only echo on a not_found as this could get messy otherwise... 
            return _ReplyData;
        }
    }
}
