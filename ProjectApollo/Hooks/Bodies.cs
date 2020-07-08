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
        public bool Failure;    // 'true' if request failed
        public string Status;
        // NOTE: 'Data' is an object that  will be serialized by JSON!
        public object Data;
        private readonly Dictionary<string, object> _additionalFields = new Dictionary<string, object>();

        public ResponseBody()
        {
            this.RespondSuccess();  // assume success
        }
        public ResponseBody RespondSuccess()
        {
            Status = "success";
            Failure = false;
            return this;
        }

        public ResponseBody RespondFailure(string pMsg, string pMsg2 = null)
        {
            Status = "fail";
            if (!String.IsNullOrEmpty(pMsg))
            {
                this.ErrorData("error", pMsg);
            }
            if (!String.IsNullOrEmpty(pMsg2))
            {
                this.ErrorData("errorInfo", pMsg2);
            }
            return this;
        }
        public void ErrorData(string pBase, string pMessage)
        {
            if (Data == null)
            {
                Data = new Dictionary<string, string>();
            }
            if (Data is Dictionary<string, string> dictData)
            {
                dictData.Add(pBase, pMessage);
            }
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
