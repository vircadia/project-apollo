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
using Project_Apollo.Configuration;
using Project_Apollo.Hooks;

namespace Project_Apollo.Registry
{
    public sealed class APIRegistry
    {
        private static readonly string _logHeader = "[APIRegistry]";

        // Pointers to all the paths specified in the Hooks
        public List<APIPath> _apiPaths;
        public APIPath[] _apiPathsArray;

        public APIRegistry()
        {
            // Find all the path hooks and add them to '_apiPaths'
            _apiPaths = LocateHooks();
            _apiPathsArray = _apiPaths.ToArray();
        }
        /// <summary>
        /// Use app reflection to find all the methods decorated with "[APIPath]".
        /// This routine searches all of the application, finds all the APIPath
        ///     decorations and puts them in '_apiPaths' for searching through
        ///     when requests are received.
        /// </summary>
        private List<APIPath> LocateHooks()
        {
            List<APIPath> foundPaths = new List<APIPath>(); // Always reset this list at the start of this method!
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
                    catch
                    {
                        // nothing needs be done here
                        Context.Log.Error("{0} No assemblies found", _logHeader);
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

                                        for(int ix = 0; ix<paths.Length; ix++)
                                        {
                                            APIPath _path = paths[ix];
                                            _path.AssignedMethod = mi;
                                            foundPaths.Add(_path);

                                            Context.Log.Debug("{0} Discovered: {1} {2}; {3}",
                                                        _logHeader, _path.HTTPMethod, _path.PathLike, mi.Name);
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
            return foundPaths;
        }

        /// <summary>
        /// Given an URL and a method, find he APIPath processor for this request.
        /// </summary>
        /// <param name="pRawURL">the url for the request (like "/api/v1/user")</param>
        /// <param name="pMethod">the HTTP method of the request</param>
        /// <param name="oArguments">output the list of "%" parameters in the request</param>
        /// <param name="oQueryArguments">output the query parameters in the request</param>
        /// <returns></returns>
        public APIPath FindPathProcessor(string pRawURL, string pMethod, out List<string> oArguments)
        {
            APIPath ret = null;
            // compare strings; If a % symbol is located, then skip that so long as
            //      the inbound string matches totally.
            // Append the value of % in the inbound request to the array passed to the function
            List<string> arguments = new List<string>();

            for (int pathIndex = 0; pathIndex < _apiPathsArray.Length; pathIndex++)
            {
                APIPath apiPath = _apiPathsArray[pathIndex];

                arguments.Clear();

                if (pMethod == apiPath.HTTPMethod)
                {
                    string requestString = pRawURL;

                    // See if the request has a query part. If so, extract same
                    int queryIndex = requestString.IndexOf('?');
                    if (queryIndex != -1)
                    {
                        requestString = requestString.Substring(0, queryIndex);
                    }

                    string[] matchPieces = apiPath.PathLike.Split(new[] { '/' });
                    string[] reqPieces = requestString.Split(new[] { '/' });

                    // if the length doesn't match, this cannot match
                    if (matchPieces.Length == reqPieces.Length)
                    {
                        // Loop through the pieces and verify they match
                        bool matchFound = true;
                        for (int ii = 0; ii < matchPieces.Length; ii++)
                        {
                            if (matchPieces[ii] == "%")
                            {
                                // The request has a match field. Save value in 'arguments'
                                arguments.Add(reqPieces[ii]);
                            }
                            else
                            {
                                // the pieces must match
                                if (matchPieces[ii] != reqPieces[ii])
                                {
                                    matchFound = false;
                                    break;
                                }
                            }
                        }

                        if (matchFound)
                        {
                            // don't need to look at any more APIPath entries
                            ret = apiPath;
                            break;
                        }
                    }
                }
            }

            oArguments = arguments;
            return ret;
        }

        /// <summary>
        /// Process an inboound request.
        /// Search the collected APIPaths for a path match and do the operation
        ///     appropriate for that request.
        /// </summary>
        /// <param name="pReq">The wrapper for ListernerHttpContext</param>
        /// <returns></returns>
        public RESTReplyData ProcessInbound(RESTRequestData pReq)
        {
            RESTReplyData _replyData = null;

            APIPath foundPath = FindPathProcessor(pReq.RawURL, pReq.Method, out List<string> oArguments);

            if (foundPath != null)
            {
                // Found the matching, process the request
                if (Context.Params.P<bool>(AppParams.P_DEBUG_PROCESSING))
                {
                    Context.Log.Debug("{0} Processing '{1}:{2} from {3}:{4}' with {5}", _logHeader,
                                                pReq.Method, pReq.RawURL,
                                                pReq.RemoteUser, pReq.RemotePort,
                                                foundPath.AssignedMethod.Name);
                }
                try
                {
                    object _method = Activator.CreateInstance(foundPath.AssignedMethod.DeclaringType);
                    _replyData = (RESTReplyData)foundPath.AssignedMethod.Invoke(_method,
                                new object[] { pReq, oArguments });
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Exception processing: {1}", _logHeader, e.ToString());
                    _replyData = null;
                }

            }
            // If we didn't get a reply constructed, tell the requestor some error nonsense.
            if (_replyData == null)
            {
                // The request does not match any path, return error
                _replyData = new RESTReplyData();
                ResponseBody respBody = new ResponseBody();

                respBody.RespondFailure("operation not found");

                _replyData.SetBody(respBody, pReq);
            }

            return _replyData;
        }
    }
}
