using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;

namespace NetSparkle
{
    /// <summary>
    /// An app-cast 
    /// </summary>
    public class NetSparkleAppCast
    {
        private NetSparkleConfiguration _config;
        private String _castUrl;

        private const String itemNode = "item";
        private const String enclosureNode = "enclosure";
        private const String releaseNotesLinkNode = "sparkle:releaseNotesLink";
        private const String versionAttribute = "sparkle:version";
        private const String dasSignature = "sparkle:dsaSignature";
        private const String urlAttribute = "url";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="config">the current configuration</param>
        public NetSparkleAppCast(string castUrl, NetSparkleConfiguration config)
        {
            _config = config;
            _castUrl = castUrl;
        }

        /// <summary>
        /// Gets the latest version
        /// </summary>
        /// <returns>the AppCast item corresponding to the latest version</returns>
        public NetSparkleAppCastItem GetLatestVersion()
        {
            NetSparkleAppCastItem latestVersion = null;

            if (_castUrl.StartsWith("file://")) //handy for testing
            {
                var path = _castUrl.Replace("file://", "");
                using (var reader = XmlReader.Create(path))
                {
                    latestVersion = ReadAppCast(reader, latestVersion);
                }
            }
            else
            {
                // build a http web request stream
                WebRequest request = HttpWebRequest.Create(_castUrl);
                request.UseDefaultCredentials = true;

                // request the cast and build the stream
                WebResponse response = request.GetResponse();
                using (Stream inputstream = response.GetResponseStream())
                {
                    using (XmlTextReader reader = new XmlTextReader(inputstream))
                    {
                        latestVersion = ReadAppCast(reader, latestVersion);
                    }
                }
            }

            latestVersion.AppName = _config.ApplicationName;
            latestVersion.AppVersionInstalled = _config.InstalledVersion;
            return latestVersion;
        }

        private static NetSparkleAppCastItem ReadAppCast(XmlReader reader,
                                                         NetSparkleAppCastItem latestVersion)
        {
            NetSparkleAppCastItem currentItem = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            {
                                currentItem = new NetSparkleAppCastItem();
                                break;
                            }
                        case releaseNotesLinkNode:
                            {
                                currentItem.ReleaseNotesLink = reader.ReadString().Trim();
                                break;
                            }
                        case enclosureNode:
                            {
                                currentItem.Version = reader.GetAttribute(versionAttribute);
                                currentItem.DownloadLink = reader.GetAttribute(urlAttribute);
                                currentItem.DSASignature = reader.GetAttribute(dasSignature);

                                break;
                            }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            {
                                if (latestVersion == null)
                                    latestVersion = currentItem;
                                else if (currentItem.CompareTo(latestVersion) > 0)
                                {
                                    latestVersion = currentItem;
                                }
                                break;
                            }
                    }
                }
            }
            return latestVersion;
        }
    }
}
