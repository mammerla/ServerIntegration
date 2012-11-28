/*
Copyright (c) Microsoft Corporation
All rights reserved.
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the 
License at http://www.apache.org/licenses/LICENSE-2.0 
    
THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING 
WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace mammerla.SharePointIntegration
{
    /// <summary>
    /// A static class with methods for working with URL strings.
    /// </summary>
    public static partial class UrlUtilities
    {

        /// <summary>
        /// Returns whether a URL is local to a particular machine.
        /// </summary>
        /// <param name="url">URL to test.</param>
        /// <returns>True if the URL is local to a machine.</returns>
        public static bool IsLocal(String url)
        {
            String serverBaseName = GetServerBaseName(url);

            if (serverBaseName.Equals(Environment.MachineName, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (serverBaseName.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the base name of the server in a given URL.  e.g., if you use http://myawesomeserver:52/baz.htm, returns myawesomeserver.
        /// </summary>
        /// <param name="fullUrl">URL to use.</param>
        /// <returns>Name of the server base name.</returns>
        public static String GetServerBaseName(String fullUrl)
        {
            String serverBaseName = GetBaseUrlFromFullUrl(fullUrl);

            int slashSlash = serverBaseName.IndexOf("//");

            if (slashSlash >= 0)
            {
                serverBaseName = serverBaseName.Substring(slashSlash + 2);
            }

            int lastSlash = serverBaseName.LastIndexOf("/");

            if (lastSlash > 0)
            {
                serverBaseName = serverBaseName.Substring(0, lastSlash);
            }

            int lastColon = serverBaseName.LastIndexOf(":");

            if (lastColon > 0)
            {
                serverBaseName = serverBaseName.Substring(0, lastColon);
            }

            return serverBaseName;
        }

        public static String GetServerFromUrl(String fullUrl)
        {
            fullUrl = fullUrl.ToUpper();

            if (fullUrl.Length > 8)
            {
                int nextSlash = fullUrl.IndexOf('/', 9);

                if (nextSlash >= 0)
                {
                    fullUrl = fullUrl.Substring(0, nextSlash);
                }
            }

            return fullUrl;
        }

        public static string ConvertServerRelativeUrlToFullUrl(String serverRelativeUrl)
        {
            return serverRelativeUrl;
        }

        public static String UrlEncode(String url)
        {
            url = url.Replace(" ", "%20");

            return url;
        }

        public static String UrlDecode(String url)
        {
            url = url.Replace("%20", " ");

            return url;
        }

        public static String ReplaceLocalhost(String sourceUrl, String urlWithoutLocalhost)
        {
            int index = sourceUrl.IndexOf("/localhost");

            // is this http://localhost or https://localhost
            if (index < 6 || index > 7)
            {
                return sourceUrl;
            }

            return GetBaseUrlFromFullUrl(urlWithoutLocalhost) + GetServerRelativeUrlFromFullUrl(sourceUrl);
        }

        public static String GetParentFolderUrl(String url)
        {
            int lastSlash = url.LastIndexOf("/");

            if (lastSlash == url.Length - 1)
            {
                lastSlash = url.LastIndexOf("/", lastSlash - 1);
            }

            if (lastSlash < 0)
            {
                return null;
            }

            if (url.StartsWith("http://", StringComparison.InvariantCulture) && lastSlash <= 7)
            {
                return null;
            }

            if (url.StartsWith("https://", StringComparison.InvariantCulture) && lastSlash <= 8)
            {
                return null;
            }

            return url.Substring(0, lastSlash + 1);
        }

        public static bool IsUrlForIntranet(String fullUrl)
        {
            return UrlUtilities.IsBaseUrlForIntranet(UrlUtilities.GetBaseUrlFromFullUrl(fullUrl));
        }

        public static bool IsBaseUrlForIntranet(String baseUrl)
        {
            return baseUrl.IndexOf(".") < 0;
        }

        public static bool ServerNamesAreEqual(String fullUrlA, String fullUrlB)
        {
            String serverNameA = GetCanonicalServerNameFromFullUrl(fullUrlA);
            String serverNameB = GetCanonicalServerNameFromFullUrl(fullUrlB);

            return serverNameA.Equals(serverNameB);
        }

        public static String GetObjectRelativeUrl(String baseUrl, String childObjectUrl)
        {
            if (!childObjectUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                childObjectUrl = UrlUtilities.EnsurePathStartsWithSlash(childObjectUrl);
                baseUrl = UrlUtilities.EnsurePathStartsWithSlash(baseUrl);
            }

            if (!childObjectUrl.StartsWith(baseUrl, StringComparison.InvariantCulture))
            {
                throw new InvalidOperationException();
            }

            return childObjectUrl.Substring(baseUrl.Length, childObjectUrl.Length - baseUrl.Length);
        }

        public static String GetServerNameFromFullUrl(String fullUrl)
        {
            if (fullUrl.Length > 8)
            {
                if (fullUrl.StartsWith("http://", StringComparison.InvariantCulture) || fullUrl.StartsWith("https://", StringComparison.InvariantCulture))
                {
                    int firstSlash = fullUrl.IndexOf("://") + 3;
                    int nextSlash = fullUrl.IndexOf('/', 9);

                    if (nextSlash >= 0)
                    {
                        fullUrl = fullUrl.Substring(firstSlash, nextSlash - firstSlash);
                    }
                }
            }

            return fullUrl;
        }

        public static String GetBaseUrlFromFullUrl(String fullUrl)
        {
            if (fullUrl.Length > 8)
            {
                if (fullUrl.StartsWith("http://", StringComparison.InvariantCulture) || fullUrl.StartsWith("https://", StringComparison.InvariantCulture))
                {
                    int firstSlash = fullUrl.IndexOf("://") + 3;
                    int nextSlash = fullUrl.IndexOf('/', 9);

                    if (nextSlash >= 0)
                    {
                        fullUrl = fullUrl.Substring(0, nextSlash);
                    }
                }
            }

            return fullUrl;
        }

        public static String GetCanonicalServerNameFromFullUrl(String fullUrl)
        {
            String serverName = GetServerNameFromFullUrl(fullUrl);

            return serverName.ToLower();
        }

        public static String GetSuggestedContentTypeForUrl(String url)
        {
            String extension = GetFileExtensionFromFullUrl(url);

            switch (extension.ToLower())
            {
                case "jpeg":
                case "jpg":
                    return "image/jpeg";
                
                case "html":
                case "htm":
                    return "text/html";
                
                case "xml":
                    return "text/xml";

                case "swf":
                    return "application/x-shockwave-flash";
                
                case "xap":
                    return "application/x-silverlight-app";

                case "txt":
                    return "text/plain";
       
                case "css":
                    return "text/css";
         
                case "js":
                    return "application/x-javascript";

                case "png":
                    return "image/png";

                case "mp3":
                    return "audio/mpeg";

                case "mp4":
                    return "video/mp4";
                
                case "pptx":
                    return "application/vnd.ms-powerpoint.presentation.12";
                
                case "docx":
                    return "application/vnd.ms-word.document.12";

                case "xlsx":
                    return "application/vnd.ms-excel.12";

                default:
                    return "unknown/unknown";
            }
        }

        public static bool IsRelativeUrl(String url)
        {
            if (url.Length > 8)
            {
                if (url.StartsWith("http://", StringComparison.InvariantCulture) || url.StartsWith("https://", StringComparison.InvariantCulture))
                {
                    return false;

                }
            }

            return true;
        }

        public static String GetFileUrlFromFullUrl(String fullUrl)
        {
            String fileName = null;

            if (!fullUrl.EndsWith("/"))
            {
                int end = fullUrl.Length - 1;

                int questionMark = fullUrl.LastIndexOf("?");

                if (questionMark >= 0) 
                {
                    end = questionMark - 1;
                }

                int lastSlash = fullUrl.LastIndexOf('/', end);

                fileName = fullUrl.Substring(lastSlash + 1, end - lastSlash);
            }

            return fileName;
        }

        public static String GetFileUrlAndParamsFromFullUrl(String fullUrl)
        {
            String fileName = null;

            if (!fullUrl.EndsWith("/"))
            {
                int end = fullUrl.Length - 1;

                int questionMark = fullUrl.LastIndexOf("?");

                if (questionMark >= 0)
                {
                    end = questionMark;
                }

                int lastSlash = fullUrl.LastIndexOf('/', end);

                fileName = fullUrl.Substring(lastSlash + 1, fullUrl.Length - (lastSlash + 1));
            }

            return fileName;
        }

        public static bool IsFullUrlImage(String fullUrl)
        {
            String extension = GetFileExtensionFromFullUrl(fullUrl);

            switch (extension.ToLower())
            {
                case "jpg":
                    return true;
                case "jpeg":
                    return true;
                case "png":
                    return true;
                case "gif":
                    return true;
            }

            return false;
        }

        public static String GetFileExtensionFromFullUrl(String fullUrl)
        {
            String extension = GetFileUrlFromFullUrl(fullUrl);

            if (extension == null)
            {
                return null;
            }

            int lastPeriod = extension.LastIndexOf(".");

            if (lastPeriod > 0)
            {
                extension = extension.Substring(lastPeriod + 1);
            }

            return extension.ToLower();
        }

        public static String GetBaseFileNameFromFullUrl(String fullUrl)
        {
            String fileName = GetFileUrlFromFullUrl(fullUrl);

            if (fileName == null)
            {
                return null;
            }

            int lastPeriod = fileName.LastIndexOf(".");

            if (lastPeriod > 0)
            {
                fileName = fileName.Substring(0, lastPeriod);
            }

            return fileName;
        }

        public static String GetFolderUrlFromFullUrl(String fullUrl)
        {
            fullUrl = StripQueryParams(fullUrl);

            if (!fullUrl.EndsWith("/"))
            {
                int lastSlash = fullUrl.LastIndexOf('/');

                fullUrl = fullUrl.Substring(0, lastSlash + 1);
            }

            return fullUrl;
        }

        public static String GetRelativeUrlFromBaseUrl(String fullUrl, String baseUrl)
        {
            if (!fullUrl.StartsWith(baseUrl, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException();
            }

            return fullUrl.Substring(baseUrl.Length , fullUrl.Length - (baseUrl.Length));
        }

        public static String GetServerRelativeUrlFromFullUrl(String fullUrl)
        {
            //fullUrl = fullUrl.ToUpper();

            fullUrl = StripQueryParams(fullUrl);

            if (fullUrl.Length > 8)
            {
                if (fullUrl.StartsWith("http://") || fullUrl.StartsWith("https://"))
                {
                    int nextSlash = fullUrl.IndexOf('/', 9);

                    if (nextSlash >= 0)
                    {
                        fullUrl = fullUrl.Substring(nextSlash, fullUrl.Length - nextSlash);
                    }
                }
            }

    //        Debug.Assert(fullUrl.IndexOf("//") < 0);

            return fullUrl;
        }

        public static String StripQueryParams(String url)
        {
            int lastQuestion = url.LastIndexOf("?");

            if (lastQuestion >= 0)
            {
                url = url.Substring(0, lastQuestion);
            }

            return url;
        }

        public static String EnsurePathDoesNotEndWithSlash(String url)
        {
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            return url;
        }

        public static String EnsurePathEndsWithSlash(String url)
        {
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            return url;
        }

        public static String EnsurePathDoesNotStartWithSlash(String url)
        {
            if (url.StartsWith("/"))
            {
                url = url.Substring(1);
            }

            return url;
        }


        public static String EnsurePathStartsWithSlash(String url)
        {
            if (!url.StartsWith("/"))
            {
                url = "/" + url;
            }

            return url;
        }

        public static bool UrlsAreEqual(String relativeUrlA, String relativeUrlB)
        {
            if (relativeUrlA != null && relativeUrlB == null)
            {
                return false;
            }

            if (relativeUrlB != null && relativeUrlA == null)
            {
                return false;
            }

            if (relativeUrlA == null && relativeUrlA == relativeUrlB)
            {
                return true;
            }

            try
            {
                relativeUrlA = CanonicalizeRelativeUrlForCompare(relativeUrlA);
                relativeUrlB = CanonicalizeRelativeUrlForCompare(relativeUrlB);
            }
            catch (UriFormatException)
            {
                return false;
            }

            return relativeUrlA == relativeUrlB;
        }

        public static bool FullUrlsAreEqual(String fullUrlA, String fullUrlB)
        {
            if (fullUrlA != null && fullUrlB == null)
            {
                return false;
            }

            if (fullUrlB != null && fullUrlA == null)
            {
                return false;
            }

            if (fullUrlA == null && fullUrlA == fullUrlB)
            {
                return true;
            }

            try
            {
                fullUrlA = CanonicalizeUrlForCompare(fullUrlA);
                fullUrlB = CanonicalizeUrlForCompare(fullUrlB);
            }
            catch (UriFormatException)
            {
                return false;
            }

            return fullUrlA == fullUrlB;
        }

        public static String CanonicalizeUrlForCompare(String fullUrl)
        {
            fullUrl = CanonicalizeUrl(fullUrl);
            fullUrl = fullUrl.ToUpper(CultureInfo.InvariantCulture);

            if (fullUrl.EndsWith("/"))
            {
                fullUrl = fullUrl.Substring(0, fullUrl.Length - 1);
            }

            return fullUrl;
        }

        public static String CanonicalizeRelativeUrlForCompare(String url)
        {
            url = CanonicalizeRelativeUrl(url);

            return url.ToUpper();
        }

        public static String CanonicalizeRelativeFolderUrlForCompare(String url)
        {
            url = CanonicalizeRelativeUrl(url);

            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            return url.ToUpper();
        }

        public static String CanonicalizeFolderUrl(String url)
        {
            url = CanonicalizeUrl(url);

            String fileName = GetName(url);

            if (fileName.IndexOf(".") > 0)
            {
                url = url.Substring(0, url.Length - fileName.Length);
            }
            else if (!url.EndsWith("/"))
            {
                url += "/";
            }
             
            return url;
        }

        public static String CanonicalizeRelativeUrl(String url)
        {
            url = url.Replace(" ", "%20");
            Uri uri = new Uri(url, UriKind.Relative);

            return uri.ToString();
        }

        public static String CanonicalizeUrl(String url)
        {
            if (url.StartsWith("/"))
            {
            }

            Uri uri = new Uri(url);

            return uri.AbsoluteUri;
        }

        public static String TryCanonicalizeUrl(String url)
        {
            if (url.StartsWith("/"))
            {

            }

            Uri uri = null;

            try
            {
                uri = new Uri(url);
            }
            catch (UriFormatException)
            {
                return null;
            }

            return uri.AbsoluteUri;
        }

        public static bool IsFolderUrl(String url)
        {
            if (url == null)
            {
                return false;
            }

            return url.EndsWith("/");
        }

        public static bool IsFullUrl(String url)
        {
            if (url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (url.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static String GetFirstFolderName(String url)
        {
            while (url.StartsWith("/"))
            {
                url = url.Substring(1, url.Length - 1);
            }

            int firstSlash = url.IndexOf("/");

            if (firstSlash < 0)
            {
                return null;
            }

            return url.Substring(0, firstSlash);
        }

        public static String GetName(String url)
        {
            int lastSlash = url.LastIndexOf("/");

            if (lastSlash < 0)
            {
                return url;
            }
            else if (lastSlash == 0)
            {
                return String.Empty;
            }

            if (lastSlash == url.Length - 1)
            {
                lastSlash = url.LastIndexOf("/", lastSlash - 1);

                if (UrlUtilities.IsFullUrl(url) && lastSlash < 9)
                {
                    return String.Empty;
                }
            }

            String name = url.Substring(lastSlash + 1, url.Length - (lastSlash + 1));

            if (name.EndsWith("/"))
            {
                name = name.Substring(0, name.Length - 1);
            }

            return name;
        }

        /// <summary>
        /// Returns the name of the folder in the URL.  e.g., if the URL is http://contoso.sharepoint.com/foo/baz/test.htm, returns "baz".
        /// </summary>
        /// <param name="url">URL to find a folder name for.</param>
        /// <returns>Folder component of the URL.</returns>
        public static String GetLastFolderName(String url)
        {
            int lastSlash = url.LastIndexOf("/");

            if (lastSlash < 0)
            {
                return null;
            }

            int firstSlash = url.LastIndexOf("/", lastSlash - 1);

            if (firstSlash < 0)
            {
                return null;
            }

            return url.Substring(firstSlash + 1, lastSlash - (firstSlash + 1));
        }

    }
}
