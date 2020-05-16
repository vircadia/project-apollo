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
    public class Logger
    {
        public enum LogLevels
        {
            Error,
            Info,
            Warn,
            Debug
        }

        public LogLevels LogLevel { get; set; }

        private readonly LogWriter _logWriter;
        /// <summary>
        /// Create a logger taht writes to a file.
        /// There will be multiple files (rotated the number of minutes specified)
        ///     and each filename will begin with "MetaverseServer-".
        /// The target directory is created if it doesn't exist.
        /// </summary>
        /// <param name="pLogDirectory"></param>
        public Logger(string pLogDirectory)
        {
            // Verify the log directory exists
            if (!Directory.Exists(pLogDirectory))
            {
                Directory.CreateDirectory(pLogDirectory);
            }

            // Initialize the logger with a default log level.
            _logWriter = new LogWriter(pLogDirectory, "MetaverseServer-", 60);
            LogLevel = LogLevels.Info;
        }

        /// <summary>
        /// Version of logger that returns a stub that doesn't do any logging.
        /// Use this for no logging but allowing all the log statements to
        /// exist in the code.
        /// </summary>
        public Logger()
        {
            _logWriter = null;
        }

        // Set the log level from a string
        public void SetLogLevel(string pLevel)
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
        public void Flush()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
            }
        }

        public void Info(string pMsg, params string[] pParms)
        {
            if (_logWriter != null
                && ( LogLevel == LogLevels.Info
                    || LogLevel == LogLevels.Warn
                    || LogLevel == LogLevels.Debug))
            {
                _logWriter.Write(String.Format(pMsg, pParms));
            }
        }
        public void Warn(string pMsg, params string[] pParms)
        {
            if (_logWriter != null
                && (LogLevel == LogLevels.Warn
                    || LogLevel == LogLevels.Debug))
            {
                _logWriter.Write(String.Format(pMsg, pParms));
            }
        }
        public void Debug(string pMsg, params string[] pParms)
        {
            if (_logWriter != null
                && ( LogLevel == LogLevels.Debug))
            {
                _logWriter.Write(String.Format(pMsg, pParms));
            }
        }
        public void Error(string pMsg, params string[] pParms)
        {
            if (_logWriter != null)
            {
                _logWriter.Write(String.Format(pMsg, pParms));
            }
        }
    }
}
