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
    /// Interface to a block of parameter.
    /// This is used for both the command line parameters and for persistant parameters.
    /// </summary>
    public interface IParamBlock
    {
        // Basic parameter fetching interface which returns the parameter
        //   value of the requested parameter name.
        T P<T>(string pParamName);
        object GetObjectValue(string pParamName);
        bool HasParam(string pParamName);
    }

    /// <summary>
    /// Parameters come from four sources:
    ///     Application defined parameters with default values
    ///     Persistant site configuration parameters values
    ///     Command line overrides
    /// This routine wraps these three collections and returns
    /// the proper value stepping from 'overrides' through site
    /// configuration to the application default parameters.
    /// </summary>
    public class AppParams : IParamBlock
    {
        private readonly ParamBlock _defaultParameters;
        private readonly ParamPersistant _siteParameters;
        private readonly ParamBlock _commandLineParameters;

        /// <summary>
        /// Construct application wide parameters.
        /// First process the command line parameters (the highest priority),
        /// then add the default parameters. That will give us the value
        /// for the persistant parameter filename.
        /// Will throw an exception if there is an error anywhere.
        /// </summary>
        /// <param name="args">command line arguements</param>
        public AppParams(string[] args)
        {
            _defaultParameters = BuildDefaultParameters();

            // Build the command line parameters
            _commandLineParameters = new ParamBlock();
            MergeCommandLine(args, null, null);

            _siteParameters = new ParamPersistant(this.P<string>("ConfigFile"));
            _siteParameters.SetParameterDefaultValues();
        }

        private ParamBlock BuildDefaultParameters()
        {
            ParamBlock ret = new ParamBlock();

            ret.Add(new ParamBlock.ParameterDefn<bool>("Quiet", "Quiet console output", false));
            ret.Add(new ParamBlock.ParameterDefn<bool>("Verbose", "Excessive console output", false));
            ret.Add(new ParamBlock.ParameterDefn<bool>("ConsoleLog", "Also log to the console", true));
            ret.Add(new ParamBlock.ParameterDefn<string>("ConfigFile", "Per site configuration file", "config.json"));

            ret.Add(new ParamBlock.ParameterDefn<string>("DefaultIceServer", "IP address of ice server. If empty, set to self.", ""));

            // NOTE: on Windows10, you must add url to acl: netsh http add urlacl url=http://+:9400/ user=everyone
            ret.Add(new ParamBlock.ParameterDefn<string>("Listener.Host", "HttpListener host", "+"));
            ret.Add(new ParamBlock.ParameterDefn<int>("Listener.Port", "HttpListener port", 9400));
            ret.Add(new ParamBlock.ParameterDefn<string>("Listener.Response.Header.Server", "What to return as 'Server: header field", "1.5"));

            ret.Add(new ParamBlock.ParameterDefn<string>("Storage.Dir", "Root of entity storage", "Entities"));
            ret.Add(new ParamBlock.ParameterDefn<string>("Storage.StaticDir", "Directory of static pages served for users", "Static"));
            ret.Add(new ParamBlock.ParameterDefn<int>("Storage.IdleMinutes", "Minutes to keep entities in memory", 60));
            ret.Add(new ParamBlock.ParameterDefn<int>("Storage.FlushMinutes", "Minutes before changed data is flushed", 1));

            ret.Add(new ParamBlock.ParameterDefn<string>("Commerce.MarketplaceKey", "Public key for Marketplace access", "lksjdlkjskldjflsd"));

            ret.Add(new ParamBlock.ParameterDefn<string>("LogLevel", "One of 'warn', 'info', 'debug'", "Debug"));
            ret.Add(new ParamBlock.ParameterDefn<int>("Logger.RotateMins", "Minutes to write to log file before starting next", 60));
            ret.Add(new ParamBlock.ParameterDefn<bool>("Logger.ForceFlush", "Force a flush after each log write", true));
            ret.Add(new ParamBlock.ParameterDefn<string>("Logger.LogDirectory", "Directory to put logs into", "Logs"));


            ret.SetParameterDefaultValues();

            return ret;
        }
        
        // IParamBlock.GetObjectValue()
        public object GetObjectValue(string pParamName)
        {
            if (_commandLineParameters.HasParam(pParamName))
            {
                return _commandLineParameters.GetObjectValue(pParamName);
            }
            else
            {
                if (_siteParameters.HasParam(pParamName))
                {
                    return _siteParameters.GetObjectValue(pParamName);
                }
                else
                {
                    if (_defaultParameters.HasParam(pParamName))
                    {
                        return _defaultParameters.GetObjectValue(pParamName);
                    }
                }
            }
            return null;
        }

        // IParamBlock.HasParam()
        public bool HasParam(string pParamName)
        {
            return _commandLineParameters.HasParam(pParamName)
                || _siteParameters.HasParam(pParamName)
                || _defaultParameters.HasParam(pParamName);
        }

        // IParamBlock.P<T>()
        public T P<T>(string pParamName)
        {
            if (_commandLineParameters.HasParam(pParamName))
            {
                return _commandLineParameters.P<T>(pParamName);
            }
            else
            {
                if (_siteParameters != null && _siteParameters.HasParam(pParamName))
                {
                    return _siteParameters.P<T>(pParamName);
                }
                else
                {
                    if (_defaultParameters.HasParam(pParamName))
                    {
                        return _defaultParameters.P<T>(pParamName);
                    }
                }
            }
            return default;
        }
        /// <summary>
        /// Set the value of a site parameter.
        /// Will be persisted if the parameter is persistable.
        /// Note that the parameter value type must be 'string'.
        /// </summary>
        /// <param name="pParamName"></param>
        /// <param name="pValue"></param>
        // The
        public void SetSiteParameter(string pParamName, string pValue)
        {
            _siteParameters.SetParameterValue(pParamName, pValue);
        }

        /// <summary>
        /// Given parameters from the command line, read the parameters and set values specified
        /// </summary>
        /// <param name="args">array of command line tokens</param>
        /// <param name="firstOpParameter"> if 'true' presume the first token in the parameter line is a special value that should be assigned to the keyword "--firstparam".</param>
        /// <param name="multipleLastParameters"> if 'true' presume multiple specs at the end of the line are filenames and pack them together into a CSV string in LAST_PARAM.</param>
        /// </param>
        /// <returns></returns>
        // <param name="args">array of command line tokens</param>
        // <param name="firstOpFlag">if 'true' presume the first token in the parameter line
        // is a special value that should be assigned to the keyword "--firstparam".</param>
        // <param name="multipleLastParameters">if 'true' presume multiple specs at the end of the line
        // are filenames and pack them together into a CSV string in LAST_PARAM.</param>
        private bool MergeCommandLine(string[] args, string firstOpParameter, string multipleLastParameters)
        {
            bool ret = true;    // start out assuming parsing worked

            bool firstOpFlag = false;   // no first op
            if (!String.IsNullOrEmpty(firstOpParameter))
            {
                firstOpFlag = true;
            }
            bool multipleLast = false;
            if (!String.IsNullOrEmpty(multipleLastParameters))
            {
                multipleLast = true;
            }

            for (int ii = 0; ii < args.Length; ii++)
            {
                string para = args[ii];
                // is this a parameter?
                if (para[0] == '-')
                {
                    ii += AddCommandLineParameter(para, (ii == (args.Length - 1)) ? null : args[ii + 1]);
                }
                else
                {
                    if (ii == 0 && firstOpFlag)
                    {
                        // if the first thing is not a parameter, make like it's an op or something
                        ii += AddCommandLineParameter(firstOpParameter, args[ii + 1]);
                    }
                    else
                    {
                        if (multipleLast)
                        {
                            // Pack all remaining arguments into a comma-separated list as LAST_PARAM
                            StringBuilder multFiles = new StringBuilder();
                            for (int jj = ii; jj < args.Length; jj++)
                            {
                                if (multFiles.Length != 0)
                                {
                                    multFiles.Append(",");
                                }
                                multFiles.Append(args[jj]);
                            }
                            AddCommandLineParameter(multipleLastParameters, multFiles.ToString());

                            // Skip them all
                            ii = args.Length;
                        }
                        else
                        {
                            throw new ArgumentException("Unknown parameter " + para);
                        }
                    }
                }
            }

            return ret;
        }
        // Store the value for the parameter.
        // If we accept the value as a good value for the parameter, return 1 else 0.
        // A 'good value' is one that does not start with '-' or is not after a boolean parameter.
        // Return the number of parameters to advance the parameter line. That means, return
        //    a zero of we didn't used the next parameter and a 1 if the next parameter
        //    was used as a value so don't consider it the next parameter.
        private int AddCommandLineParameter(string pParm, string val)
        {
            // System.Console.WriteLine(String.Format("AddCommandLineParameter: parm={0}, val={1}", pParm, val));
            int ret = 1;    // start off assuming the next token is the value we're setting
            string parm = pParm.ToLower();
            // Strip leading hyphens
            while (parm[0] == '-')
            {
                parm = parm.Substring(1);
            }

            // If the boolean parameter starts with "no", turn it off rather than on.
            string positiveAssertion = "true";
            if (parm.Length > 2 && parm[0] == 'n' && parm[1] == 'o')
            {
                string maybeParm = parm.Substring(2);
                // System.Console.WriteLine(String.Format("AddCommandLineParameter: maybeParm={0}", maybeParm));
                if (_defaultParameters.TryGetParameter(maybeParm, out ParamBlock.ParameterDefnBase parmDefnX))
                {
                    if (parmDefnX.GetValueType() == typeof(Boolean))
                    {
                        // The parameter without the 'no' exists and is a boolean
                        positiveAssertion = "false";
                        parm = maybeParm;
                    }
                }
            }

            // If the next token starts with a parameter mark, it's not really a value
            if (val == null)
            {
                ret = 0;    // the next token is not used here to set the value
            }
            else
            {
                if (val[0] == '-')
                {
                    val = null; // don't use the next token as a value
                    ret = 0;    // the next token is not used here to set the value
                }
            }

            if (_defaultParameters.TryGetParameter(parm, out ParamBlock.ParameterDefnBase parmDefn))
            {
                // If the parameter is a boolean type and the next value is not a parameter,
                //      don't try to take up the next value.
                // This handles boolean flags.
                // If there is a value next (val != null) and that value is not the
                //    values 'true' or 'false' or 't' or 'f', then ignore the next value
                //    as not belonging to this flag. THis allows (and the logic above)
                //    allows:
                //        "--flag --otherFlag ...",
                //        "--flag something ...",
                //        "--flag true --otherFlag ...",
                //        "--noflag --otherflag ...",
                //        etc
                if (parmDefn.GetValueType() == typeof(Boolean))
                {
                    if (val != null)
                    {
                        string valL = val.ToLower();
                        if (valL != "true" && valL != "t" && valL != "false" && valL != "f")
                        {
                            // The value is not associated with this boolean so ignore it
                            val = null; // don't use the val token
                            ret = 0;    // the next token is not used here to set the value
                        }
                    }
                    if (val == null)
                    {
                        // If the value is assumed, use the value based on the optional 'no'
                        val = positiveAssertion;
                    }
                }
                // Set the named parameter to the passed value
                parmDefn.SetValue(val);
            }
            else
            {
                throw new ArgumentException("Unknown parameter " + parm);
            }
            return ret;
        }


    }
}
