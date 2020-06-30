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

using Newtonsoft.Json;

namespace Project_Apollo.Hooks
{
    public class Bodies
    {
    }

    /// <summary>
    /// All the Metaverse responses are a JSON string with two top
    /// level fields of "status" and "data".
    /// Wrap some of the manipulations of this structure.
    /// </summary>
    public class ResponseBody
    {
        public string Status;
        // NOTE: 'Data' is an object that  will be serialized by JSON!
        public object Data;
        private readonly Dictionary<string, object> _additionalFields = new Dictionary<string, object>();

        public ResponseBody()
        {
            Status = "success"; // assume success
        }
        public ResponseBody(object pDataContents) {
            Data = pDataContents;
        }

        public ResponseBody(string pStatus, object pDataContents) {
            Status = pStatus;
            Data = pDataContents;
        }

        public ResponseBody RespondSuccess()
        {
            Status = "success";
            return this;
        }

        public ResponseBody RespondFailure()
        {
            Status = "fail";
            return this;
        }
        public void ErrorData(string pBase, string pMessage)
        {
            Data = new Dictionary<string, string>()
            {
                { pBase, pMessage }
            };
        }

        public void AddExtraTopLevelField(string pName, object pValue)
        {
            _additionalFields.Add(pName, pValue);
        }

        // If an instance is assigned to a string variable, the cast causes conversion to string
        public static implicit operator string(ResponseBody rb) => rb.ToString();
        // Convert the body data into a JSON string
        public override string ToString()
        {
            Dictionary<string, object> respBody = new Dictionary<string, object>
            {
                { "status", Status }
            };
            foreach (var kvp in _additionalFields)
            {
                respBody.Add(kvp.Key, kvp.Value);
            }
            if (Data != null)
            {
                respBody.Add("data", Data);
            }
            return JsonConvert.SerializeObject(respBody, Formatting.None, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
