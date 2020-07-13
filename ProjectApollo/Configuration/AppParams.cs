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

using Project_Apollo.Entities;

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
        // Definitions of strings for each parameter.
        // This exists so code doesn't have to spell the configuration name exactly
        //      right. This prevents some runtime errors.
        public static readonly string P_QUIET = "Quiet";
        public static readonly string P_VERBOSE = "Verbose";
        public static readonly string P_CONSOLELOG = "ConsoleLog";
        public static readonly string P_CONFIGFILE = "ConfigFile";
        public static readonly string P_VERSION = "Version";
        public static readonly string P_METAVERSE_SERVER_URL = "MetaverseServerUrl";
        public static readonly string P_DEFAULT_ICE_SERVER = "DefaultIceServer";

        public static readonly string P_METAVERSE_NAME = "Metaverse.Name";
        public static readonly string P_METAVERSE_NICKNAME = "Metaverse.Nickname";
        public static readonly string P_METAVERSE_INFO_FILE = "Metaverse.InfoFile";

        public static readonly string P_LISTENER_HOST = "Listener.Host";
        public static readonly string P_LISTENER_PORT = "Listener.Port";
        public static readonly string P_LISTENER_RESPONSE_HEADER_SERVER = "Listener.Response.Header.Server";
        public static readonly string P_LISTENER_CORS_PROCESSING = "Listener.CORS.Processing";

        public static readonly string P_STORAGE_DIR = "Storage.Dir";
        public static readonly string P_ENTITY_DIR = "Storage.Entity.Dir";
        public static readonly string P_STORAGE_STATIC_DIR = "Storage.StaticDir";

        public static readonly string P_ACCOUNT_AUTHTOKEN_LIFETIME_HOURS = "Account.AuthToken.Lifetime";
        public static readonly string P_ACCOUNT_AUTHTOKENEXPIRATIONCHECKSECONDS = "Account.AuthTokenExpirationCheckSeconds";
        public static readonly string P_ACCOUNT_USERNAME_FORMAT = "Account.Username.Format";
        public static readonly string P_ACCOUNT_EMAIL_FORMAT = "Account.Email.Format";

        public static readonly string P_DOMAIN_TOKENGEN_URL = "Domain.TokenGenURL";

        public static readonly string P_SESSION_IDLE_EXPIRE_SECONDS = "Session.IdleExpireSeconds";
        public static readonly string P_SESSION_IDLE_CHECK_SECONDS = "Session.SessionIdleCheckSeconds";
        public static readonly string P_SESSION_THROTTLE_ACCOUNT_CREATE = "Session.Throttle.AccountCreate";
        public static readonly string P_SESSION_THROTTLE_TOKEN_CREATE = "Session.Throttle.TokenCreate";

        public static readonly string P_REQUEST_EXPIRATION_CHECK_SECONDS = "Request.ExpirationCheckSeconds";
        public static readonly string P_CONNECTION_REQUEST_SECONDS = "Request.ConnectionRequestExpirationSeconds";

        public static readonly string P_COMMERCE_MARKETPLACEKEY = "Commerce.MarketplaceKey";

        public static readonly string P_LOGLEVEL = "LogLevel";
        public static readonly string P_LOGGER_ROTATE_MINS = "Logger.RotateMins";
        public static readonly string P_LOGGER_FORCE_FLUSH = "Logger.ForceFlush";
        public static readonly string P_LOGGER_LOG_DIR = "Logger.LogDirectory";

        public static readonly string P_DEBUG_PROCESSING = "Debug.Processing";
        public static readonly string P_DEBUG_QUERIES = "Debug.Queries";

        private readonly ParamBlock _defaultParameters;
        private ParamPersistant _siteParameters;
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
        }

        // Load the site parameters.
        // The initialization of AppParams is a two step process because this step depends on the default and command line parameters
        public void LoadSiteParameters()
        {
            _siteParameters = new ParamPersistant(EntityStorage.GenerateAbsStorageLocation(null, this.P<string>(AppParams.P_CONFIGFILE)) );
            _siteParameters.SetParameterDefaultValues();
        }

        private ParamBlock BuildDefaultParameters()
        {
            ParamBlock ret = new ParamBlock();

            ret.Add(new ParamBlock.ParameterDefn<bool>(P_QUIET, "Quiet console output", false));
            ret.Add(new ParamBlock.ParameterDefn<bool>(P_VERBOSE, "Excessive console output", false));
            ret.Add(new ParamBlock.ParameterDefn<bool>(P_CONSOLELOG, "Also log to the console", true));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_CONFIGFILE, "Per site configuration file", "config.json"));
            ret.Add(new ParamBlock.ParameterDefn<bool>(P_VERSION, "Just print out the appliction version", false));

            ret.Add(new ParamBlock.ParameterDefn<string>(P_METAVERSE_SERVER_URL, "URL for main API access. If empty, set to self", ""));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_DEFAULT_ICE_SERVER, "IP address of ice server. If empty, set to self", ""));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_METAVERSE_NAME, "Long name of the Metaverse", "Vircadia Noobie"));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_METAVERSE_NICKNAME, "Short form of the name of the Metaverse", "Noobie"));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_METAVERSE_INFO_FILE, "File of addition infor for metaverse_info request", "MetaverseInfo.json"));

            // NOTE: on Windows10, you must add url to acl: netsh http add urlacl url=http://+:9400/ user=everyone
            ret.Add(new ParamBlock.ParameterDefn<string>(P_LISTENER_HOST, "HttpListener host", "+"));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_LISTENER_PORT, "HttpListener port", 9400));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_LISTENER_RESPONSE_HEADER_SERVER, "What to return as 'Server: header field", "1.5"));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_LISTENER_CORS_PROCESSING, "CORS response header. One of 'NONE', 'ORIGIN', 'STAR'", "STAR"));

            ret.Add(new ParamBlock.ParameterDefn<string>(P_STORAGE_DIR, "Root of storage", "."));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_ENTITY_DIR, "Root of entity storage", "Entities"));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_STORAGE_STATIC_DIR, "Directory of static pages served for users", "static"));

            ret.Add(new ParamBlock.ParameterDefn<int>(P_ACCOUNT_AUTHTOKEN_LIFETIME_HOURS, "Hours that an AuthToken is allowed to live", 12));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_ACCOUNT_AUTHTOKENEXPIRATIONCHECKSECONDS, "Seconds between times checking for authtoken flushing", 60));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_ACCOUNT_USERNAME_FORMAT, "Regex for username format", @"^[0-9a-z_+-\.]+$"));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_ACCOUNT_EMAIL_FORMAT, "Regex for email format", @"^[0-9a-z_+-\.]+@[0-9a-z-\.]+$"));

            ret.Add(new ParamBlock.ParameterDefn<string>(P_DOMAIN_TOKENGEN_URL, "URL for user domain token generation", "/static/DomainTokenLogin.html"));

            ret.Add(new ParamBlock.ParameterDefn<int>(P_SESSION_IDLE_EXPIRE_SECONDS, "Time to keep idle sessions", 60 * 5));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_SESSION_IDLE_CHECK_SECONDS, "How often to check for idle sessions", 60 * 2));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_SESSION_THROTTLE_ACCOUNT_CREATE, "Account creatable per heartbeat", 2));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_SESSION_THROTTLE_TOKEN_CREATE, "Access tokens creatable per heartbeat", 2));

            ret.Add(new ParamBlock.ParameterDefn<int>(P_REQUEST_EXPIRATION_CHECK_SECONDS, "How often to check for request expiration", 60));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_CONNECTION_REQUEST_SECONDS, "Seconds that a connection_request stays active", 20));

            ret.Add(new ParamBlock.ParameterDefn<string>(P_COMMERCE_MARKETPLACEKEY, "Public key for Marketplace access", "lksjdlkjskldjflsd"));

            ret.Add(new ParamBlock.ParameterDefn<string>(P_LOGLEVEL, "One of 'warn', 'info', 'debug'", "Debug"));
            ret.Add(new ParamBlock.ParameterDefn<int>(P_LOGGER_ROTATE_MINS, "Minutes to write to log file before starting next", 24 * 60));
            ret.Add(new ParamBlock.ParameterDefn<bool>(P_LOGGER_FORCE_FLUSH, "Force a flush after each log write", true));
            ret.Add(new ParamBlock.ParameterDefn<string>(P_LOGGER_LOG_DIR, "Directory to put logs into", "Logs"));

            ret.Add(new ParamBlock.ParameterDefn<bool>(P_DEBUG_PROCESSING, "Whether to print each API request processing", false));
            ret.Add(new ParamBlock.ParameterDefn<bool>(P_DEBUG_QUERIES, "Whether to print each query request processing", false));

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
            if (_siteParameters.HasParam(pParamName))
            {
                _siteParameters.SetParameterValue(pParamName, pValue);
            }
            else {
                var newVal = new ParamBlock.ParameterDefn<string>(pParamName, "", pValue);
                newVal.AssignDefault();
                _siteParameters.Add(newVal);
            }
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
