using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace Project_Apollo.Registry
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false)]
    public class APIPath : Attribute
    {
        public string PathLike;
        public MethodInfo AssignedMethod = null;
        public string HTTPMethod;
        public bool AllowArgument;
        public APIPath(string Path, string Method, bool AllowArguments) // [APIPath("/api/v1/user/heartbeat", "PUT")]
        {
            PathLike = Path;
            HTTPMethod = Method;
            AllowArgument = AllowArguments;
        }
    }
}
