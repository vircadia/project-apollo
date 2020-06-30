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

using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Project_Apollo.Logging
{
    public abstract class Logger
    {
        public enum LogLevels
        {
            Error,
            Info,
            Warn,
            Debug
        }

        public LogLevels LogLevel { get; set; }

        public Logger()
        {
            LogLevel = LogLevels.Info;
        }

        // Set the log level from a string
        public virtual void SetLogLevel(string pLevel)
        {
            LogLevel = (pLevel.ToLower()) switch
            {
                "error" => LogLevels.Info,
                "info" => LogLevels.Info,
                "warn" => LogLevels.Warn,
                "debug" => LogLevels.Debug,
                _ => LogLevels.Info,
            };
        }

        /// <summary>
        /// See that the log file is flushed out
        /// </summary>
        public abstract void Flush();

        public abstract void Info(string pMsg, params object[] pParms);
        public abstract void Warn(string pMsg, params object[] pParms);
        public abstract void Debug(string pMsg, params object[] pParms);
        public abstract void Error(string pMsg, params object[] pParms);
    }
}
