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

namespace Project_Apollo.Logging
{
    /// <summary>
    /// Class for writing a high performance, high volume log file.
    /// Sometimes, to debug, one has a high volume logging to do and the regular
    /// log file output is not appropriate.
    /// Create a new instance with the parameters needed and
    /// call Write() to output a line. Call Close() when finished.
    /// If created with no parameters, it will not log anything.
    /// </summary>
    public class LogWriter : IDisposable
    {
        public bool Enabled { get; private set; }

        private readonly string m_logDirectory = ".";
        private readonly int m_logMaxFileTimeMin = 5;    // 5 minutes
        public String LogFileHeader { get; set; }

        private StreamWriter m_logFile = null;
        private readonly TimeSpan m_logFileLife;
        private DateTime m_logFileEndTime;
        private readonly Object m_logFileWriteLock = new Object();
        private readonly bool m_flushWrite;

        private readonly string _logHeader = "[LOG WRITER]";

        /// <summary>
        /// Create a log writer that will not write anything. Good for when not enabled
        /// but the write statements are still in the code.
        /// </summary>
        public LogWriter()
        {
            Enabled = false;
            m_logFile = null;
        }

        /// <summary>
        /// Create a log writer instance.
        /// </summary>
        /// <param name="dir">The directory to create the log file in. May be 'null' for default.</param>
        /// <param name="headr">The characters that begin the log file name. May be 'null' for default.</param>
        /// <param name="maxFileTime">Maximum age of a log file in minutes. If zero, will set default.</param>
        /// <param name="flushWrite">Whether to do a flush after every log write. Best left off but
        /// if one is looking for a crash, this is a good thing to turn on.</param>
        public LogWriter(string dir, string headr, int maxFileTime, bool flushWrite)
        {
            m_logDirectory = dir ?? ".";

            LogFileHeader = headr ?? "log-";

            m_logMaxFileTimeMin = maxFileTime;
            if (m_logMaxFileTimeMin < 1)
                m_logMaxFileTimeMin = 5;

            m_logFileLife = new TimeSpan(0, m_logMaxFileTimeMin, 0);
            m_logFileEndTime = DateTime.Now + m_logFileLife;

            m_flushWrite = flushWrite;

            Enabled = true;
        }
        // Constructor that assumes flushWrite is off.
        public LogWriter(string dir, string headr, int maxFileTime) : this(dir, headr, maxFileTime, false)
        {
        }

        public void Dispose()
        {
            this.Close();
        }

        public void Close()
        {
            Enabled = false;
            if (m_logFile != null)
            {
                m_logFile.Close();
                m_logFile.Dispose();
                m_logFile = null;
            }
        }

        public void Write(string line, params object[] args)
        {
            if (!Enabled) return;
            Write(String.Format(line, args));
        }

        public void Flush()
        {
            if (!Enabled) return;
            if (m_logFile != null)
            {
                m_logFile.Flush();
            }
        }

        public void Write(string line)
        {
            if (!Enabled) return;
            try
            {
                lock (m_logFileWriteLock)
                {
                    DateTime now = DateTime.UtcNow;
                    if (m_logFile == null || now > m_logFileEndTime)
                    {
                        if (m_logFile != null)
                        {
                            m_logFile.Close();
                            m_logFile.Dispose();
                            m_logFile = null;
                        }

                        // First log file or time has expired, start writing to a new log file
                        m_logFileEndTime = now + m_logFileLife;
                        string path = (m_logDirectory.Length > 0 ? m_logDirectory
                                    + System.IO.Path.DirectorySeparatorChar.ToString() : "")
                                + String.Format("{0}{1}.log", LogFileHeader, now.ToString("yyyyMMddHHmmss"));
                        m_logFile = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
                    }
                    if (m_logFile != null)
                    {
                        StringBuilder buff = new StringBuilder(line.Length + 25);
                        buff.Append(now.ToString("yyyyMMddHHmmssfff"));
                        // buff.Append(now.ToString("yyyyMMddHHmmss"));
                        buff.Append(",");
                        buff.Append(line);
                        buff.Append("\r\n");
                        m_logFile.Write(buff.ToString());
                        if (m_flushWrite)
                            m_logFile.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("{0}: FAILURE WRITING TO LOGFILE: {1}", _logHeader, e);
            }
        }
    }
}
