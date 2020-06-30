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

namespace Project_Apollo.Configuration
{
    /// <summary>
    /// A basic block of parameters
    /// </summary>
    public class ParamBlock : IParamBlock
    {
        private static readonly string _logHeader = "[ParamBlock]";

        private readonly Dictionary<string, ParameterDefnBase> _params = new Dictionary<string, ParameterDefnBase>();

        public ParamBlock()
        {
        }

        /// <summary>
        /// Add this parameter to the defintion.
        /// Internally, names are alway lower case.
        /// </summary>
        /// <param name="pDefn"></param>
        public void Add(ParameterDefnBase pDefn)
        {
            _params.Add(pDefn.name.ToLower(), pDefn);
        }

         // Base parameter definition that gets and sets parameter values via a string
        public abstract class ParameterDefnBase
        {
            public string name;         // string name of the parameter
            public string desc;         // a short description of what the parameter means
            public abstract Type GetValueType();
            public ParamBlock context; // context for setting and getting values
            public ParameterDefnBase(string pName, string pDesc)
            {
                name = pName;
                desc = pDesc;
            }
            // Set the parameter value to the default
            public abstract void AssignDefault();
            // Get the value as a string
            public abstract string GetValue();
            public abstract object GetObjectValue();
            // Set the value to this string value
            public abstract void SetValue(string valAsString);
        }

        // Specific parameter definition for a parameter of a specific type.
        public class ParameterDefn<T> : ParameterDefnBase
        {
            public T defaultValue;
            public T value;
            public override Type GetValueType()
            {
                return typeof(T);
            }
            public ParameterDefn(string pName, string pDesc, T pDefault)
                : base(pName, pDesc)
            {
                defaultValue = pDefault;
            }
            public T Value()
            {
                return value;
            }
            public override void AssignDefault()
            {
                value = defaultValue;
            }
            public override string GetValue()
            {
                string ret = String.Empty;
                if (value != null)
                {
                    ret = value.ToString();
                }
                return ret;
            }
            public override object GetObjectValue()
            {
                return value;
            }
            public override void SetValue(String valAsString)
            {
                // Find the 'Parse' method on that type
                System.Reflection.MethodInfo parser;
                try
                {
                    parser = GetValueType().GetMethod("Parse", new Type[] { typeof(String) });
                }
                catch
                {
                    parser = null;
                }
                if (parser != null)
                {
                    // Parse the input string
                    try
                    {
                        T setValue = (T)parser.Invoke(GetValueType(), new Object[] { valAsString });
                        // System.Console.WriteLine("SetValue: setting value on {0} to {1}", this.name, setValue);
                        // Store the parsed value
                        value = setValue;
                        // Context.Log.Debug("{0} SetValue. {1} = {2}", _logHeader, name, setValue);
                    }
                    catch (Exception e)
                    {
                        Context.Log.Error("{0} Failed parsing parameter value '{1}': '{2}'", _logHeader, valAsString, e.ToString());
                    }
                }
                else
                {
                    // If there is not a parser, try doing a conversion
                    try
                    {
                        T setValue = (T)Convert.ChangeType(valAsString, GetValueType());
                        value = setValue;
                        // Context.Log.Debug("{0} SetValue. Converter. {1} = {2}", _logHeader, name, setValue);
                    }
                    catch (Exception e)
                    {
                        Context.Log.Error("{0} Conversion failed for {1}: {2}", _logHeader, this.name, e.ToString());
                    }
                }
            }
            // Create a description for this parameter that can be used in a list of parameters.
            // For better listings, there is a special 'separator' parameter that is just for the description.
            //      These separator parameters start with an equal sign ('=').
            const int LEADER = 20;
            public override string ToString()
            {
                StringBuilder buff = new StringBuilder();
                bool hasValue = true;
                // If the name start with "=", it's a formatting separator
                if (name.StartsWith("="))
                {
                    hasValue = false;
                    buff.Append(name.Substring(1));
                }
                else
                {
                    buff.Append("--");
                    buff.Append(name);
                    buff.Append(": ");
                }
                // Provide tab like padding between the name and the description
                if (buff.Length < LEADER)
                {
                    buff.Append("                                        ".Substring(0, LEADER - buff.Length));
                }
                buff.Append(desc);
                // Add the type and the default value of the parameter
                if (hasValue)
                {
                    buff.Append(" (");
                    buff.Append("Type=");
                    switch (GetValueType().ToString())
                    {
                        case "System.Boolean": buff.Append("bool"); break;
                        case "System.Int32": buff.Append("int"); break;
                        case "System.Float": buff.Append("float"); break;
                        case "System.Double": buff.Append("double"); break;
                        case "System.String": buff.Append("string"); break;
                        default: buff.Append(GetValueType().ToString()); break;
                    }
                    buff.Append(",Default=");
                    buff.Append(GetValue());
                    buff.Append(")");
                }

                return buff.ToString();
            }
        }

        /// <summary>
        /// Try to get the definition of the parameter.
        /// Internally, parameter names are all lower case so that's not a problem
        /// </summary>
        /// <param name="pParamName">name of the parameter</param>
        /// <param name="pDefn">returns the definition block for this parameter</param>
        /// <returns>'true' if the parameter was found</returns>
        public bool TryGetParameter(string pParamName, out ParameterDefnBase pDefn)
        {
            string paramNameLower = pParamName.ToLower();
            return _params.TryGetValue(paramNameLower, out pDefn);
        }

        // Return a value for the parameter.
        // This is used by most callers to get parameter values.
        // Note that it outputs a console message if not found. Not found means that the caller
        //     used the wrong string name.
        public T P<T>(string paramName)
        {
            T ret = default;
            if (TryGetParameter(paramName, out ParameterDefnBase pbase))
            {
                if (pbase is ParameterDefn<T> pdef)
                {
                    ret = pdef.Value();
                }
                else
                {
                    Context.Log.Error("{0} Fetched unknown parameter. Param={1}", _logHeader, paramName);
                }
            }
            return ret;
        }

        public object GetObjectValue(string pParamName)
        {
            object ret = null;
            if (TryGetParameter(pParamName, out ParameterDefnBase pbase))
            {
                ret = pbase.GetObjectValue();
            }
            return ret;
        }

        public bool HasParam(string pParamName)
        {
            return _params.ContainsKey(pParamName.ToLower());
        }

        // Find the named parameter and set its value.
        // Returns 'false' if the parameter could not be found.
        public bool SetParameterValue(string paramName, string valueAsString)
        {
            bool ret = false;
            if (TryGetParameter(paramName, out ParameterDefnBase parm))
            {
                parm.SetValue(valueAsString);
                ret = true;
            }
            return ret;
        }

        // Pass through the settable parameters and set the default values.
        public void SetParameterDefaultValues()
        {
            foreach (ParameterDefnBase parm in _params.Values)
            {
                parm.context = this;
                parm.AssignDefault();
            }
        }


    }
}
