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
using System.IO;
using System.Text;

using Project_Apollo.Configuration;
using Project_Apollo.Entities;

namespace Project_Apollo.Logging
{
    public class LogFileLogger : Logger
    {
        private readonly LogWriter _logWriter;
        private Logger _consoleLogger;

        /// <summary>
        /// Create a logger that writes to a file.
        /// There will be multiple files (rotated the number of minutes specified)
        ///     and each filename will begin with "MetaverseServer-".
        /// The target directory is created if it doesn't exist.
        /// </summary>
        /// <param name="pLogDirectory">Directory to create the log files</param>
        /// <param name="pAlsoLogToConsole">if 'true', also write to the console each message</param>
        public LogFileLogger(string pLogDirectory, bool pAlsoLogToConsole=false) : base()
        {
            string logDir = EntityStorage.GenerateAbsStorageLocation(pLogDirectory);
            // Verify the log directory exists
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            if (pAlsoLogToConsole)
            {
                _consoleLogger = new ConsoleLogger();
                _consoleLogger.LogLevel = LogLevel;
            }

            // Initialize the logger with a default log level.
            int rotateMinutes = Context.Params.P<int>(AppParams.P_LOGGER_ROTATE_MINS);
            bool forceFlush = Context.Params.P<bool>(AppParams.P_LOGGER_FORCE_FLUSH);
            _logWriter = new LogWriter(logDir, "MetaverseServer-", rotateMinutes, forceFlush);
        }

        public override void SetLogLevel(string pLevel)
        {
            base.SetLogLevel(pLevel);
            if (_consoleLogger != null) _consoleLogger.SetLogLevel(pLevel);
        }

        /// <summary>
        /// Version of logger that returns a stub that doesn't do any logging.
        /// Use this for no logging but allowing all the log statements to
        /// exist in the code.
        /// </summary>
        public LogFileLogger() : base()
        {
            _logWriter = null;
        }

        /// <summary>
        /// See that the log file is flushed out
        /// </summary>
        public override void Flush()
        {
            if (_logWriter != null)
            {
                _logWriter.Flush();
            }
        }

        public override void Info(string pMsg, params object[] pParms)
        {
            if (_logWriter != null
                && ( LogLevel == LogLevels.Info
                    || LogLevel == LogLevels.Warn
                    || LogLevel == LogLevels.Debug))
            {
                _logWriter.Write("INFO," + String.Format(pMsg, pParms));
                if (_consoleLogger != null) _consoleLogger.Info(pMsg, pParms);
            }
        }
        public override void Warn(string pMsg, params object[] pParms)
        {
            if (_logWriter != null
                && (LogLevel == LogLevels.Warn
                    || LogLevel == LogLevels.Debug))
            {
                _logWriter.Write("WARN," + String.Format(pMsg, pParms));
                if (_consoleLogger != null) _consoleLogger.Debug(pMsg, pParms);
            }
        }
        public override void Debug(string pMsg, params object[] pParms)
        {
            if (_logWriter != null
                && ( LogLevel == LogLevels.Debug))
            {
                _logWriter.Write("DEBUG," + String.Format(pMsg, pParms));
                if (_consoleLogger != null) _consoleLogger.Debug(pMsg, pParms);
            }
        }
        public override void Error(string pMsg, params object[] pParms)
        {
            if (_logWriter != null)
            {
                _logWriter.Write("ERROR," + String.Format(pMsg, pParms));
                if (_consoleLogger != null) _consoleLogger.Error(pMsg, pParms);
            }
        }
    }
}
