using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace NetSparkle
{
    /// <summary>
    /// An app-cast 
    /// </summary>
    public class NetSparkleAppCast
    {
        private const string ItemNode = "item";
        private const string EnclosureNode = "enclosure";
        private const string ReleaseNotesLinkNode = "sparkle:releaseNotesLink";
        private const string VersionAttribute = "sparkle:version";
        private const string DasSignature = "sparkle:dsaSignature";
        private const string UrlAttribute = "url";
        private const string PubDateNode = "pubDate";

        private readonly NetSparkleConfiguration _config;
        private readonly String _castUrl;
        private readonly List<NetSparkleAppCastItem> _items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="config">the current configuration</param>
        public NetSparkleAppCast(string castUrl, NetSparkleConfiguration config)
        {
            _config = config;
            _castUrl = castUrl;

            _items = new List<NetSparkleAppCastItem>();
        }

        /// <summary>
        /// Download castUrl resource and parse it
        /// </summary>
        public void Read()
        {
            if (_castUrl.StartsWith("file://")) //handy for testing
            {
                var path = _castUrl.Replace("file://", "");
                using (var reader = XmlReader.Create(path))
                {
                    Parse(reader);
                }
            }
            else
            {
                // build a http web request stream
                WebRequest request = WebRequest.Create(_castUrl);
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                // TODO: disable ssl check if _config.TrustEverySSL

                // request the cast and build the stream
                WebResponse response = request.GetResponse();
                using (Stream inputstream = response.GetResponseStream())
                {
                    if (inputstream == null)
                    {
                        Debug.WriteLine("Cannot read response from URL " + _castUrl);
                        return;
                    }
                    using (XmlTextReader reader = new XmlTextReader(inputstream))
                    {
                        Parse(reader);
                    }
                }
            }

        }

        private void Parse(XmlReader reader)
        {
            NetSparkleAppCastItem currentItem = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case ItemNode:
                            currentItem = new NetSparkleAppCastItem()
                            {
                                AppVersionInstalled = _config.InstalledVersion,
                                AppName = _config.ApplicationName
                            };
                            break;
                        case ReleaseNotesLinkNode:
                            if (currentItem != null)
                            {
                                currentItem.ReleaseNotesLink = reader.ReadString().Trim();
                            }
                            break;
                        case EnclosureNode:
                            if (currentItem != null)
                            {
                                currentItem.Version = reader.GetAttribute(VersionAttribute);
                                currentItem.DownloadLink = reader.GetAttribute(UrlAttribute);
                                currentItem.DSASignature = reader.GetAttribute(DasSignature);
                            }
                            break;
                        case PubDateNode:
                            if (currentItem != null)
                            {
                                string dt = reader.ReadString().Trim();
                                try
                                {
                                    currentItem.PublicationDate = DateTime.ParseExact(dt, "ddd, dd MMM yyyy HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (FormatException ex)
                                {
                                    Debug.WriteLine("Cannot parse item datetime " + dt + " with message " + ex.Message);
                                }
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case ItemNode:
                            _items.Add(currentItem);
                            break;
                    }
                }
            }

            // sort versions reserve order
            _items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        }

        /// <summary>
        /// Returns sorted list of updates between current and latest. Installed is not included.
        /// </summary>
        /// <returns></returns>
        public NetSparkleAppCastItem[] GetUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);

            return _items.Where(item => new Version(item.Version).CompareTo(installed) > 0).ToArray();
        }
    }
}
