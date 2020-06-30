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
using System.Linq;
using System.Reflection;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Project_Apollo.Configuration
{
    /// <summary>
    /// Persistant ParamBlock reads and stores the configuration information
    /// in a JSON file.
    /// Parameters in the file are either key/value pairs of string/string
    /// or, the value can be a parameter definition block.
    /// <pre>
    ///     { "site.configuration": "config.json",
    ///       "site.dist.enable": {
    ///             "description": "set to 'true' to enable distribution",
    ///             "type": "System.Boolean",
    ///             "value": "true"
    ///       }
    ///     }
    /// </pre>
    /// Note that the value is given as a string. The string is converted
    /// to the specified type.
    /// </summary>
    public class ParamPersistant : ParamBlock
    {
        private static readonly string _logHeader = "[ParamPersistant]";
        private readonly string _filename;

        /// <summary>
        /// Read in parameter file.
        /// The configuration file is a flat specification of key/value pairs -- multi-level,
        ///     nested parameter definitions is not implemented!!
        /// NOTE: the value types are REQUIRED to match the type found in AppParams or runtime error will occur.
        /// </summary>
        /// <param name="pFilename"></param>
        public ParamPersistant(string pFilename) : base()
        {
            _filename = pFilename;
            if (File.Exists(_filename))
            {
                Context.Log.Info("{0} Reading configuration file {1}", _logHeader, _filename);
                try
                {
                    JObject configs = JObject.Parse(File.ReadAllText(_filename));
                    foreach (var kvp in configs.Properties())
                    {
                        string paramName = kvp.Name;
                        JToken paramJValue = kvp.Value;
                        switch (paramJValue.Type)
                        {
                            case JTokenType.String:
                                this.Add(new ParameterDefn<string>(paramName, "", paramJValue.Value<string>()));
                                // Context.Log.Debug("{0} Setting {1} => {2}", _logHeader, paramName, paramJValue.Value < string>());
                                break;
                            case JTokenType.Boolean:
                                this.Add(new ParameterDefn<bool>(paramName, "", paramJValue.Value<bool>()));
                                // Context.Log.Debug("{0} Setting {1} => {2}", _logHeader, paramName, paramJValue.Value <bool>());
                                break;
                            case JTokenType.Integer:
                                this.Add(new ParameterDefn<int>(paramName, "", paramJValue.Value<int>()));
                                // Context.Log.Debug("{0} Setting {1} => {2}", _logHeader, paramName, paramJValue.Value <int>());
                                break;
                            default:
                                Context.Log.Error("{0} Attempt to define parameter of different type: name={1}, type{2}",
                                            _logHeader, paramName, Enum.GetName(typeof(JTokenType), paramJValue.Type));
                                // TODO: not implemented because the type is hard to specify if creation of ParameterDefn
                                // Either that or rework ParameterDefn so there is a version that takes the type as a parameter
                                string typeName = (string)paramJValue["type"];
                                Type theType = SearchForType(typeName);
                                string desc = (string)paramJValue["description"];
                                // this.Add(new ParameterDefn<theType>(paramName, desc, paramJValue.Value<string>()));
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Context.Log.Error("{0} Exception parsing/processing config file {1}: {2}", _logHeader, _filename, e);
                }
            }
            else
            {
                Context.Log.Info("{0} Could not open config file {1}", _logHeader, _filename);
            }
        }

        /// <summary>
        /// Given a name of a type, search the world for the descriptor of that type.
        /// </summary>
        /// <param name="pTypeName"></param>
        /// <returns></returns>
        private Type SearchForType(string pTypeName) {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
            {
                var theType = assembly.GetType(pTypeName);
                if (theType != null)
                {
                    return theType;
                }
            }
            return null;
        }

        public void Persist()
        {
            Context.Log.Error("{0} Attempt to persist site parameters which is unimplemented!!", _logHeader);
        }
    }
}
