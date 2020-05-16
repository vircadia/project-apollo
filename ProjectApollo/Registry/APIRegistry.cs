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
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace Project_Apollo.Registry
{
    public sealed class APIRegistry
    {
        private static readonly string _logHeader = "[APIRegistry]";

        // Pointers to all the paths specified in the Hooks
        public List<APIPath> _apiPaths;

        public APIRegistry()
        {
            // Find all the path hooks and add them to '_apiPaths'
            this.LocateHooks();
        }
        /// <summary>
        /// Use app reflection to find all the methods decorated with "[APIPath]".
        /// This routine searches all of the application, finds all the APIPath
        ///     decorations and puts them in '_apiPaths' for searching through
        ///     when requests are received.
        /// </summary>
        public void LocateHooks()
        {
            _apiPaths = new List<APIPath>(); // Always reset this list at the start of this method!
            try
            {
                int i = 0;
                for (i = 0; i < AppDomain.CurrentDomain.GetAssemblies().Length; i++)
                {
                    Assembly asm = null;
                    try
                    {
                        asm = AppDomain.CurrentDomain.GetAssemblies()[i];
                    }
                    catch(Exception e)
                    {
                        // nothing needs be done here
                    }

                    if (asm != null)
                    {
                        int ii = 0;
                        for (ii = 0; ii < asm.GetTypes().Length; ii++)
                        {
                            Type _type = null;
                            try
                            {
                                _type = asm.GetTypes()[ii];
                            }
                            catch(Exception e)
                            {
                                Context.Log.Error("{0} Exception getting types: {1}", _logHeader, e.ToString());
                            }
                            if (_type != null)
                            {
                                if (_type.IsClass)
                                {
                                    foreach(MethodInfo mi in _type.GetMethods())
                                    {
                                        APIPath[] paths = (APIPath[])mi.GetCustomAttributes(typeof(APIPath), true);

                                        int ix = 0;
                                        for(ix = 0; ix<paths.Length; ix++)
                                        {
                                            APIPath _path = paths[ix];
                                            _path.AssignedMethod = mi;
                                            _apiPaths.Add(_path);

                                            Context.Log.Debug("{0} Discovered: {1}; {2}",
                                                        _logHeader, _path.PathLike, mi.Name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Context.Log.Error("{0} Exception collecting APIPath: {1}", _logHeader, e.ToString());

            }
        }

        /// <summary>
        /// Structure used to return a requests reply.
        /// Constructed to hide the HTTP hair from the processing routines.
        /// </summary>
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

        /// <summary>
        /// Process an inboound request.
        /// Search the collected APIPaths for a path match and do the operation
        ///     appropriate for that request.
        /// </summary>
        /// <param name="requestBody"></param>
        /// <param name="rawURL"></param>
        /// <param name="method"></param>
        /// <param name="remoteUser"></param>
        /// <param name="remotePort"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public ReplyData ProcessInbound(string requestBody, string rawURL,
                        string method, IPAddress remoteUser, int remotePort,
                        Dictionary<string,string> headers)
        {
            ReplyData _ReplyData = new ReplyData
            {
                Status = 418
            };

            Dictionary<string, string> notFoundDefault = new Dictionary<string, string>
            {
                { "status", "not_found" },
                { "data", "Needs more water!" } // joke... See 418 status code :P
            };
            string notFoundDef = JsonConvert.SerializeObject(notFoundDefault);
            _ReplyData.Body = notFoundDef;
            foreach(APIPath zAPIPath in _apiPaths)
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
            return _ReplyData;
        }
    }
}
