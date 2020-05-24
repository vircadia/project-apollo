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
using System.Text;
using System.Reflection;

namespace Project_Apollo.Registry
{

    // The processing routines must match this signature
    public delegate RESTReplyData APIPathProcess(RESTRequestData pReq, List<string> pArgs);

    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false)]
    public class APIPath : Attribute
    {
        public string PathLike;
        public MethodInfo AssignedMethod = null;
        public string HTTPMethod;
        public bool AllowArgument;  // 'true' if request can have query parameters
        public APIPath(string Path, string Method, bool AllowArguments) // [APIPath("/api/v1/user/heartbeat", "PUT")]
        {
            PathLike = Path;
            HTTPMethod = Method;
            AllowArgument = AllowArguments;
        }
    }
}
