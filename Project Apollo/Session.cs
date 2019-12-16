using System;
using System.Collections.Generic;
using System.Net;
using Project_Apollo.Registry;
using System.Text;

namespace Project_Apollo
{
    public sealed class Session
    {
        private static readonly object _lock = new object();
        private static Session _inst;

        static Session() { }
        public static Session Instance
        {
            get
            {
                lock (_lock)
                {
                    if(_inst == null)
                    {
                        _inst = new Session();
                    }
                    return _inst;
                }
            }
        }

        // Begin vars!
        public HttpListener ProductionListen;
        public APIRegistry Registry;
        // End vars!
    }
}
