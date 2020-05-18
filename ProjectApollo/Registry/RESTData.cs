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
using System.IO;
using Newtonsoft.Json;

namespace Project_Apollo.Registry
{
    /// <summary>
    /// Structure used to enclose request context to make fetching data
    ///     easier for the various processors.
    /// </summary>
    public class RESTRequestData
    {
        private static readonly string _logHeader = "[RESTRequestData]";

        private HttpListenerContext _listenerContext;

        public RESTRequestData(HttpListenerContext pCtx)
        {
            _listenerContext = pCtx;
        }

        private string _requestBody;
        public string RequestBody
        {
            get
            {
                if (_requestBody == null)
                {
                    try
                    {
                        if (_listenerContext.Request.HasEntityBody)
                        {
                            using (Stream body = _listenerContext.Request.InputStream)
                            {
                                using (StreamReader sr = new StreamReader(body, _listenerContext.Request.ContentEncoding))
                                {
                                    _requestBody = sr.ReadToEnd();
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Context.Log.Error("{0} Exception fetching request body. URL={1}",
                                    _logHeader, _listenerContext.Request.RawUrl);
                        _requestBody = null;
                    }
                }
                return _requestBody;
            }
        }
        public T RequestBodyObject<T>() {
            return (T)JsonConvert.DeserializeObject<T>(this.RequestBody);
        }
        public string RawURL
        {
            get
            {
                return _listenerContext.Request.RawUrl;
            }
        }
        public string Method
        {
            get
            {
                return _listenerContext.Request.HttpMethod;
            }
        }
        public IPAddress RemoteUser
        {
            get
            {
                return _listenerContext.Request.RemoteEndPoint.Address;
            }
        }
        public int RemotePort
        {
            get
            {
                return _listenerContext.Request.RemoteEndPoint.Port;
            }
        }
        private Dictionary<string, string> _headers;
        public Dictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = Tools.NVC2Dict(_listenerContext.Request.Headers);
                }
                return _headers;
            }
        }
        private Dictionary<string, string> _queryParameters;
        public Dictionary<string, string> Queries
        {
            get
            {
                if (_queryParameters == null)
                {
                    _queryParameters = Tools.NVC2Dict(_listenerContext.Request.QueryString);
                }
                return _headers;
            }
        }
        // Return the authorization token from the request. Return 'null' if no token
        public string AuthToken
        {
            get
            {
                string token = _listenerContext.Request.Headers["Authorization"];
                return token;
            }
        }

    }
    
    /// <summary>
    /// Structure used to return a requests reply.
    /// Constructed to hide the HTTP hair from the processing routines.
    ///
    /// While one can fill all the individual fields for the HTTP response,
    /// there are some helper functions that return in the body the usual
    /// construction -- JSON string of "{ "status": "success", "data", RESPONSE }"
    /// </summary>
    public class RESTReplyData
    {
        // String body to serialize into the response
        public string Body;
        // HTTP reply status code (200, etc)
        public int Status;
        // If defined, added to the status response code 
        public string CustomStatus; // <-- Examples: OK, Not Found, Authorization Required, etc.
        // Header fields to add to the response
        public Dictionary<string, string> CustomOutputHeaders;
        public RESTReplyData()
        {
            Status = 200;   // Assume successful response
            CustomOutputHeaders = new Dictionary<string, string>();
        }
        public RESTReplyData(string pBody)
        {
            Status = 200;   // Assume successful response
            CustomOutputHeaders = new Dictionary<string, string>();
            Body = pBody;
        }

    }

}
