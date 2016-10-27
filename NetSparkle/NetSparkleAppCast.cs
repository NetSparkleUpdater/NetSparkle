using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace NetSparkle
{
    /// <summary>
    /// An app-cast 
    /// </summary>
    public class NetSparkleAppCast
    {
        private const string itemNode = "item";
        private const string enclosureNode = "enclosure";
        private const string releaseNotesLinkNode = "sparkle:releaseNotesLink";
        private const string descriptionNode = "description";
        private const string versionAttribute = "sparkle:version";
        private const string dsaSignature = "sparkle:dsaSignature";
        private const string urlAttribute = "url";
        private const string pubDateNode = "pubDate";

        private readonly Sparkle _sparkle;
        private readonly NetSparkleConfiguration _config;
        private readonly String _castUrl;
        private readonly List<NetSparkleAppCastItem> _items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="config">the current configuration</param>
        public NetSparkleAppCast(string castUrl, Sparkle sparkle, NetSparkleConfiguration config)
        {
            _sparkle = sparkle;
            _config = config;
            _castUrl = castUrl;

            _items = new List<NetSparkleAppCastItem>();
        }

        private string TryReadSignature()
        {
            try
            {
                var signaturestream = _sparkle.GetWebContentStream(_castUrl + ".dsa");
                var signature = string.Empty;
                using (StreamReader reader = new StreamReader(signaturestream, Encoding.ASCII))
                {
                    return reader.ReadToEnd().Trim();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Download castUrl resource and parse it
        /// </summary>
        public bool Read()
        {
            try
            {
                var inputstream = _sparkle.GetWebContentStream(_castUrl);
                var signature = TryReadSignature();
                return ReadStream(inputstream, signature);
            }
            catch (Exception e)
            {
                Debug.WriteLine("netsparkle: error reading app cast {0}: {1} ", _castUrl, e.Message);
                return false;
            }
        }

        private bool ReadStream(Stream inputstream, String signature)
        {
            if (inputstream == null)
            {
                Debug.WriteLine("netsparkle: Cannot read response from URL " + _castUrl);
                return false;
            }

            // inputstream needs to be copied. WebResponse can't ne positionized back
            var memorystream = new MemoryStream();
            inputstream.CopyTo(memorystream);
            memorystream.Position = 0;

            // checking signature
            var signatureNeeded = _sparkle.DSAVerificator.SignatureNeeded();
            if (signatureNeeded && _sparkle.DSAVerificator.VerifyDSASignature(signature, memorystream) == ValidationResult.Invalid)
            {
                Debug.WriteLine("netsparkle: Signature check of appcast failed");
                return false;
            }
            memorystream.Position = 0;

            // parse xml
            using (XmlTextReader reader = new XmlTextReader(memorystream))
            {
                Parse(reader);
                return true;
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
                        case itemNode:
                            currentItem = new NetSparkleAppCastItem()
                            {
                                AppVersionInstalled = _config.InstalledVersion,
                                AppName = _config.ApplicationName
                            };
                            break;
                        case releaseNotesLinkNode:
                            if (currentItem != null)
                            {
                                currentItem.ReleaseNotesDSASignature = reader.GetAttribute(dsaSignature);
                                currentItem.ReleaseNotesLink = reader.ReadString().Trim();
                            }
                            break;
                        case descriptionNode:
                            if (currentItem != null)
                            {
                                currentItem.Description = reader.ReadString().Trim();
                            }
                            break;
                        case enclosureNode:
                            if (currentItem != null)
                            {
                                currentItem.Version = reader.GetAttribute(versionAttribute);
                                currentItem.DownloadLink = reader.GetAttribute(urlAttribute);
                                currentItem.DownloadDSASignature = reader.GetAttribute(dsaSignature);
                            }
                            break;
                        case pubDateNode:
                            if (currentItem != null)
                            {
                                string dt = reader.ReadString().Trim();
                                try
                                {
                                    currentItem.PublicationDate = DateTime.ParseExact(dt, "ddd, dd MMM yyyy HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (FormatException ex)
                                {
                                    Debug.WriteLine("netsparkle: Cannot parse item datetime " + dt + " with message " + ex.Message);
                                }
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
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
            var signatureNeeded = _sparkle.DSAVerificator.SignatureNeeded();

            return _items.Where((item) => {
                // filter smaler versions
                if (new Version(item.Version).CompareTo(installed) <= 0)
                    return false;
                // filter versions without signature if we need signatures. But accept version without downloads.
                if (signatureNeeded && string.IsNullOrEmpty(item.DownloadDSASignature) && !string.IsNullOrEmpty(item.DownloadLink))
                    return false;
                // accept everthing else
                return true;
            }).ToArray();
        }
    }
}
