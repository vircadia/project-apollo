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
                        using (Stream body = _listenerContext.Request.InputStream)
                        {
                            using (StreamReader sr = new StreamReader(body, _listenerContext.Request.ContentEncoding))
                            {
                                _requestBody = sr.ReadToEnd();
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
        // Something for accessing headers

    }
    
    /// <summary>
    /// Structure used to return a requests reply.
    /// Constructed to hide the HTTP hair from the processing routines.
    /// </summary>
    public class RESTReplyData
    {
        public RESTReplyData()
        {
            CustomOutputHeaders = new Dictionary<string, string>();
        }
        public string Body;
        public int Status;
        public string CustomStatus; // <-- Examples: OK, Not Found, Authorization Required, etc.
        public Dictionary<string, string> CustomOutputHeaders;
    }

}
