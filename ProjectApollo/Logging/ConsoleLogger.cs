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

namespace Project_Apollo.Logging
{
    public class ConsoleLogger : Logger
    {
        /// <summary>
        /// See that the log file is flushed out
        /// </summary>
        public override void Flush()
        {
        }

        public override void Info(string pMsg, params object[] pParms)
        {
            if ( ( LogLevel == LogLevels.Info
                    || LogLevel == LogLevels.Warn
                    || LogLevel == LogLevels.Debug))
            {
                System.Console.WriteLine("INFO: " + String.Format(pMsg, pParms));
            }
        }
        public override void Warn(string pMsg, params object[] pParms)
        {
            if ( (LogLevel == LogLevels.Warn
                    || LogLevel == LogLevels.Debug))
            {
                System.Console.WriteLine("WARN: " + String.Format(pMsg, pParms));
            }
        }
        public override void Debug(string pMsg, params object[] pParms)
        {
            if ( ( LogLevel == LogLevels.Debug))
            {
                System.Console.WriteLine("DEBUG: " + String.Format(pMsg, pParms));
            }
        }
        public override void Error(string pMsg, params object[] pParms)
        {
            System.Console.WriteLine("ERROR: " + String.Format(pMsg, pParms));
        }
    }
}
