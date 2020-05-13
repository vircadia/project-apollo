using System;
using System.Collections.Generic;
using System.Net;
using Project_Apollo.Registry;
using System.Text;
using System.Threading;
using Project_Apollo.Hooks;

namespace Project_Apollo
{
    public sealed class Session
    {
        private static readonly object _lock = new object();
        private static Session _inst;
        private APIRegistry _reg;
        private DomainMemory _mem;
        static Session() {
            
        }
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
        public APIRegistry Registry
        {
            get
            {
                lock (_lock)
                {

                    if (Registry == null) _reg = new APIRegistry();
                    return _reg;
                }
            }
            set
            {
                _reg = value;
            }
        }
        public DomainMemory DomainsMem
        {
            get
            {
                lock (_lock)
                {
                    if (_mem == null) _mem = new DomainMemory();
                    return _mem;
                }
            }
            set
            {
                _mem = value;
            }
        }
        public ManualResetEvent QuitWait;
        public List<string> TemporaryStackData = new List<string>();
        public Configuration CFG = Configuration.LoadConfig();
        // End vars!
    }
}
