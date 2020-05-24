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
using Project_Apollo.Logging;
using Project_Apollo.Registry;

using NUnit.Framework;

namespace Project_Apollo.Test
{
    [TestFixture]
    public class PathTests
    {
        [TestCase("/api/v1/user/profile", "GET", true)]
        [TestCase("/api/v1/domains/temporary", "POST", true)]
        [TestCase("/api/v1/USER/profile", "GET", true)]
        [TestCase("/api/v1/johnny", "GET", false)]
        [TestCase("/api/v2/USER/profile", "GET", false)]
        [TestCase("/api/v1/user/notfound", "GET", false)]
        [TestCase("/api/", "GET", false)]
        [TestCase("/api/v1", "GET", false)]
        [TestCase("/api/v1", "PUT", false)]
        [TestCase("/api/v1/", "GET", false)]
        public void FindPathTest(string pPath, string pMethod, bool pResult)
        {
            Context.Params = new AppParams(new string[] { });
            Context.Log = new ConsoleLogger();
            Context.Log.SetLogLevel("Info");

            APIRegistry registry = new APIRegistry();

            APIPath foundAPIPath = registry.FindPathProcessor(pPath, pMethod, out List<string> oArguments);
            Assert.That(oArguments.Count == 0, "FindPathProcessor found arguments should be zero");
            Assert.That((foundAPIPath != null) == pResult, "FindPathProcessor did/didn't find " + pPath);
        }

        [TestCase("/api/v1/domains/cheese/public_key", "PUT", "cheese")]
        public void PathWithArgTest(string pPath, string pMethod, string pResult)
        {
            Context.Params = new AppParams(new string[] { });
            Context.Log = new ConsoleLogger();
            Context.Log.SetLogLevel("Info");

            APIRegistry registry = new APIRegistry();

            APIPath foundAPIPath = registry.FindPathProcessor(pPath, pMethod, out List<string> oArguments);
            Assert.That(foundAPIPath != null, "Didn't find path for " + pPath);
            Assert.That(oArguments.Count == 1, "FindPathProcessor did not find argument");
            Assert.That(oArguments[0] == pResult, "FindPathProcessor did not collect argument value");
        }
    }
}
