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

using Project_Apollo.Configuration;

using NUnit.Framework;
using Project_Apollo.Logging;

namespace Project_Apollo.Test
{
    [TestFixture]
    public class CommandLineParameterTest
    {
        [SetUp]
        public void TestSetup()
        {
            Context.Log = new ConsoleLogger();
        }

        [TestCase]
        public void CommandLineBasicParsing()
        {
            string[] clParams = new string[] {
                "--LogLevel",
                "Debug",
                "--ConfigFile",
                "aSiteValue"
            };
            AppParams parms = new AppParams(clParams);

            Assert.That(parms.HasParam("LogLevel"));
            Assert.That(parms.HasParam("ConfigFile"));
            Assert.That(parms.P<string>(AppParams.P_LOGLEVEL).Equals("Debug"));
            Assert.That(parms.P<string>(AppParams.P_CONFIGFILE).Equals("aSiteValue"));
        }
        [TestCase]
        public void CommandLineBooleanNegation()
        {
            string[] clParams = new string[] {
                "--LogLevel",
                "Debug",
                "--quiet",
                "true",
                "--noverbose",
                "--ConfigFile",
                "aSiteValue"
            };
            AppParams parms = new AppParams(clParams);

            Assert.That(parms.HasParam("LogLevel"));
            Assert.That(parms.HasParam("ConfigFile"));
            Assert.That(parms.P<string>(AppParams.P_LOGLEVEL).Equals("Debug"));
            Assert.That(parms.P<string>(AppParams.P_CONFIGFILE).Equals("aSiteValue"));
            Assert.That(parms.P<bool>(AppParams.P_QUIET));
            Assert.That(!parms.P<bool>(AppParams.P_VERBOSE));
        }
        [TestCase]
        public void CommandLineBooleanWithoutValue()
        {
            string[] clParams = new string[] {
                "--LogLevel",
                "Debug",
                "--quiet",
                "true",
                "--verbose",
                "--ConfigFile",
                "aSiteValue"
            };
            AppParams parms = new AppParams(clParams);

            Assert.That(parms.HasParam("LogLevel"));
            Assert.That(parms.HasParam("ConfigFile"));
            Assert.That(parms.P<string>(AppParams.P_LOGLEVEL).Equals("Debug"));
            Assert.That(parms.P<string>(AppParams.P_CONFIGFILE).Equals("aSiteValue"));
            Assert.That(parms.P<bool>(AppParams.P_VERBOSE));
        }
    }
}
