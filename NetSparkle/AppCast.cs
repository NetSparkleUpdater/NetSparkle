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
    public class AppCast
    {
        private const string itemNode = "item";
        private const string enclosureNode = "enclosure";
        private const string releaseNotesLinkNode = "sparkle:releaseNotesLink";
        private const string descriptionNode = "description";
        private const string versionAttribute = "sparkle:version";
        private const string dsaSignature = "sparkle:dsaSignature";
        private const string criticalAttribute = "sparkle:criticalUpdate";
        private const string lengthAttribute = "length";
        private const string urlAttribute = "url";
        private const string pubDateNode = "pubDate";

        private readonly Sparkle _sparkle;
        private readonly Configuration _config;
        private readonly String _castUrl;
        private readonly List<AppCastItem> _items;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="castUrl">the URL of the appcast file</param>
        /// <param name="sparkle">The <see cref="Sparkle"/> instance to use</param>
        /// <param name="config">the current configuration</param>
        public AppCast(string castUrl, Sparkle sparkle, Configuration config)
        {
            _sparkle = sparkle;
            _config = config;
            _castUrl = castUrl;

            _items = new List<AppCastItem>();
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
                _sparkle.ReportDiagnosticMessage(string.Format("error reading app cast {0}: {1} ", _castUrl, e.Message));
                return false;
            }
        }

        private bool ReadStream(Stream inputstream, String signature)
        {
            if (inputstream == null)
            {
                _sparkle.ReportDiagnosticMessage("Cannot read response from URL " + _castUrl);
                return false;
            }

            // inputstream needs to be copied. WebResponse can't be positioned back
            var memorystream = new MemoryStream();
            inputstream.CopyTo(memorystream);
            memorystream.Position = 0;

            // checking signature
            var signatureNeeded = _sparkle.DSAChecker.SignatureNeeded();
            if (signatureNeeded && _sparkle.DSAChecker.VerifyDSASignature(signature, memorystream) == ValidationResult.Invalid)
            {
                _sparkle.ReportDiagnosticMessage("Signature check of appcast failed");
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
            AppCastItem currentItem = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case itemNode:
                            currentItem = new AppCastItem()
                            {
                                AppVersionInstalled = _config.InstalledVersion,
                                AppName = _config.ApplicationName,
                                UpdateSize = 0,
                                IsCriticalUpdate = false
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
                                string length = reader.GetAttribute(lengthAttribute);
                                if (length != null)
                                {
                                    int size = 0;
                                    if (int.TryParse(length, out size))
                                    {
                                        currentItem.UpdateSize = size;
                                    }
                                    else
                                    {
                                        currentItem.UpdateSize = 0;
                                    }
                                }
                                bool isCritical = false;
                                string critical = reader.GetAttribute(criticalAttribute);
                                if (critical != null && critical == "true" || critical == "1")
                                {
                                    isCritical = true;
                                }
                                currentItem.IsCriticalUpdate = isCritical;
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
                                    _sparkle.ReportDiagnosticMessage("Cannot parse item datetime " + dt + " with message " + ex.Message);
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

            // sort versions in reverse order
            _items.Sort((item1, item2) => -1 * item1.CompareTo(item2));
        }

        /// <summary>
        /// Returns sorted list of updates between current and latest. Installed is not included.
        /// </summary>
        public AppCastItem[] GetUpdates()
        {
            Version installed = new Version(_config.InstalledVersion);
            var signatureNeeded = _sparkle.DSAChecker.SignatureNeeded();

            return _items.Where((item) => {
                // filter smaller versions
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
