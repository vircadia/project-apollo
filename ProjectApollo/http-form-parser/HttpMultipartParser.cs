/// HttpUtils.HttpMultipartParser
/// 
/// Copyright (c) 2012 Lorenzo Polidori
/// 
/// This software is distributed under the terms of the MIT License reproduced below.
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
/// and associated documentation files (the "Software"), to deal in the Software without restriction, 
/// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
/// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
/// subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial 
/// portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
/// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
/// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
/// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
/// 

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Project_Apollo;

/// <summary>
/// HttpMultipartParser
/// Reads a multipart http data stream and returns the file name, content type and file content.
/// Also, it returns any additional form parameters in a Dictionary.
/// </summary>
namespace HttpUtils
{
    public class HttpMultipartParser
    {
        public HttpMultipartParser(Stream stream, string filePartName)
        {
            FilePartName = filePartName;
            this.Parse(stream, Encoding.UTF8);
        }

        public HttpMultipartParser(Stream stream, Encoding encoding, string filePartName)
        {
            FilePartName = filePartName;
            this.Parse(stream, encoding);
        }

        private void Parse(Stream stream, Encoding encoding)
        {
            this.Success = false;

            // Read the stream into a byte array
            byte[] data = Misc.ToByteArray(stream);

            // Copy to a string for header parsing
            string content = encoding.GetString(data);

            // The first line should contain the delimiter
            int delimiterEndIndex = content.IndexOf("\r\n");

            if (delimiterEndIndex > -1)
            {
                string delimiter = content.Substring(0, content.IndexOf("\r\n"));

                string[] sections = content.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string s in sections)
                {
                    if (s.Contains("Content-Disposition"))
                    {
                        // If we find "Content-Disposition", this is a valid multi-part section
                        // Now, look for the "name" parameter
                        Match nameMatch = new Regex(@"(?<=name\=\"")(.*?)(?=\"")").Match(s);
                        string name = nameMatch.Value.Trim().ToLower();

                        // Look for Content-Type
                        Regex re = new Regex(@"(?<=Content\-Type:)(.*?)(?=\r\n)");
                        Match contentTypeMatch = re.Match(s);

                        // Look for filename
                        re = new Regex(@"(?<=filename\=\"")(.*?)(?=\"")");
                        Match filenameMatch = re.Match(s);

                        // Did we find the required values?
                        if (nameMatch.Success || filenameMatch.Success)
                        {
                            // Set properties
                            string contentType = contentTypeMatch.Success
                                            ? contentTypeMatch.Value.Trim() : "text/text";
                            string filename = filenameMatch.Success
                                            ? filenameMatch.Value.Trim() : name;

                            int blockStartIndex = content.IndexOf(s);
                            int startIndex = s.IndexOf("\r\n\r\n") + 4;
                            int endIndex = s.Length - 2;        // assuming it ends with "\r\n"
                            int contentLength = endIndex - startIndex;

                            if (contentType.Contains("octet"))
                            {
                                // there is binary here
                                byte[] binBody = new byte[contentLength];
                                Buffer.BlockCopy(data, startIndex+blockStartIndex, binBody, 0, contentLength);
                                MultipartBinBodies.Add(filename, binBody);
                                // Context.Log.Debug("HttpMultipartParser: binary in {0}", s);
                            }
                            else
                            {
                                string textBody = s.Substring(startIndex, contentLength);
                                MultipartBodies.Add(filename, textBody);
                            }
                            MultipartContentTypes.Add(filename, contentType);
                            // Context.Log.Debug("HttpMultipartParser: n={0}, fn={1}, ct={2}, len={3}",
                            //                   name, filename, contentType, contentLength); 
                        }
                    }
                }

                // If some data has been successfully received, set success to true
                if (MultipartBodies.Count != 0)
                    this.Success = true;
            }
        }

        public IDictionary<string, string> MultipartContentTypes = new Dictionary<string, string>();
        public IDictionary<string, string> MultipartBodies = new Dictionary<string, string>();
        public IDictionary<string, byte[]> MultipartBinBodies = new Dictionary<string, byte[]>();

        public string FilePartName
        {
            get;
            private set;
        }

        public bool Success
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }

        public int UserId
        {
            get;
            private set;
        }
    }
}
