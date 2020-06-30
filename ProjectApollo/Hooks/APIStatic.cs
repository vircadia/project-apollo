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
using System.Linq;
using System.IO;
using System.Text;

using Project_Apollo.Registry;
using Microsoft.VisualBasic;
using Project_Apollo.Configuration;

namespace Project_Apollo.Hooks
{
    /// <summary>
    /// Return files in the static directory.
    /// This can be used to allow some user or admin pages that
    ///     then access the APIs for user accounts or system admin.
    /// </summary>
    public class APIStatic
    {
        private static readonly string _logHeader = "[APIStatic]";

        // A collection of all the filenames in the static directory.
        // This is used to verify that any request is only for a static file.
        static readonly HashSet<string> staticFilenames = new HashSet<string>();

        [APIPath("/static/%", "GET", true)]
        public RESTReplyData get_page1(RESTRequestData pReq, List<string> pArgs)
        {
            return get_page(pReq, pArgs);
        }
        [APIPath("/static/%/%", "GET", true)]
        public RESTReplyData get_page2(RESTRequestData pReq, List<string> pArgs)
        {
            return get_page(pReq, pArgs);
        }
        [APIPath("/static/%/%/%", "GET", true)]
        public RESTReplyData get_page3(RESTRequestData pReq, List<string> pArgs)
        {
            return get_page(pReq, pArgs);
        }
        public RESTReplyData get_page(RESTRequestData pReq, List<string> pArgs)
        {
            Context.Log.Debug("{0} GET /static/: {1}", _logHeader, pReq.RawURL);
            string baseDir = Context.Params.P<string>(AppParams.P_STORAGE_STATIC_DIR);

            // If the global list of static filenames hasn't been built, build it
            lock (staticFilenames)
            {
                if (staticFilenames.Count == 0)
                {
                    AddToStaticFilenames(baseDir);
                }
            }

            RESTReplyData replyData = new RESTReplyData();  // The HTTP response info

            string afterString = String.Join(Path.DirectorySeparatorChar, pArgs);

            string filename = Path.Combine(baseDir, afterString);
            if (staticFilenames.Contains(filename.ToLower()))
            {
                if (File.Exists(filename))
                {
                    replyData.Body = File.ReadAllText(filename);
                    string exten = Path.GetExtension(filename);
                    var mimeType = exten switch
                    {
                        ".css" => "text/css",
                        ".json" => "text/json",
                        ".yaml" => "text/yaml",
                        ".html" => "text/html",
                        ".js" => "text/javascript",
                        _ => "text/text",
                    };
                    replyData.MIMEType = mimeType;
                }
                else
                {
                    Context.Log.Error("{0} File in static list but not in filesystem: {1}", _logHeader, filename);
                    replyData.Status = 401;
                }
            }
            else
            {
                Context.Log.Error("{0} Access to non-existant static file: {1}", _logHeader, filename);
                replyData.Status = 401;
            }
            return replyData;
        }

        // So we can verify that the reference is to an actual file, build
        //     all the possible static filesnames for checking requests.
        // This prevents people sneaking in "../.." into paths and getting out
        //     of the static directory
        private void AddToStaticFilenames(string pDir)
        {
            // Add all the filenames to the collection
            staticFilenames.UnionWith(Directory.EnumerateFiles(pDir).Select( dir =>
            {
                return dir.ToLower();
            }) );
            // Recurse into each of the directories and collect the files therein
            foreach (var dir in Directory.EnumerateDirectories(pDir))
            {
                AddToStaticFilenames(dir);
            }
        }
    }
}
