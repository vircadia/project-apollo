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

namespace Project_Apollo.Test
{
/*
    [TestFixture]
    public class ProjectApolloTestCase
    {
        // documentation on attributes: https://github.com/nunit/docs/wiki/Attributes
        // documentation on assertions: https://github.com/nunit/docs/wiki/Assertions
        [SetUp]
        public virtual void Setup()
        {
        }
        [TearDown]
        public void TearDown()
        {
        }
    }
*/

    [TestFixture]
    public class CommandLineParameterTest // : ProjectApolloTestCase
    {
        [TestCase]
        public void CommandLineBasicParsing()
        {
            string[] clParams = new string[] {
                "--LogLevel",
                "Debug",
                "--site.configuration",
                "aSiteValue"
            };
            AppParams parms = new AppParams(clParams);

            Assert.That(parms.HasParam("LogLevel"));
            Assert.That(parms.HasParam("Site.Configuration"));
            Assert.That(parms.P<string>("LogLevel").Equals("Debug"));
            Assert.That(parms.P<string>("Site.Configuration").Equals("aSiteValue"));
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
                "--site.configuration",
                "aSiteValue"
            };
            AppParams parms = new AppParams(clParams);

            Assert.That(parms.HasParam("LogLevel"));
            Assert.That(parms.HasParam("Site.Configuration"));
            Assert.That(parms.P<string>("LogLevel").Equals("Debug"));
            Assert.That(parms.P<string>("Site.Configuration").Equals("aSiteValue"));
            Assert.That(parms.P<bool>("Quiet"));
            Assert.That(!parms.P<bool>("Verbose"));
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
                "--site.configuration",
                "aSiteValue"
            };
            AppParams parms = new AppParams(clParams);

            Assert.That(parms.HasParam("LogLevel"));
            Assert.That(parms.HasParam("Site.Configuration"));
            Assert.That(parms.P<string>("LogLevel").Equals("Debug"));
            Assert.That(parms.P<string>("Site.Configuration").Equals("aSiteValue"));
            Assert.That(parms.P<bool>("Verbose"));
        }
    }
}
