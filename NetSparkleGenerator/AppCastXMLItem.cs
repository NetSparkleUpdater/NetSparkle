using System;
using System.Xml;

namespace NetSparkleGenerator
{
    class AppCastXMLItem
    {
        public string Title;
        public string DownloadLink;
        public string Version;
        public DateTime? PublicationDate;
        public string ShortVersion;
        public string ReleaseNotesLink;
        public string Length;
        public string Type;
        public string DsaSignature;

        public AppCastXMLItem(string title, string downloadLink, string version, DateTime? pubDate, string shortVersion = "", string releaseNotesLink = "", string length = "0", string type = "application/octet-stream", string dsaSignature = "")
        {
            Title = title;
            DownloadLink = downloadLink;
            Version = version;
            PublicationDate = pubDate;
            ShortVersion = shortVersion;
            ReleaseNotesLink = releaseNotesLink;
            Length = length;
            Type = type;
            DsaSignature = dsaSignature;
        }

        public void WriteItem(XmlWriter writer)
        {
            writer.WriteStartElement("item");
            writer.WriteElementString("title", Title);

            if (!String.IsNullOrEmpty(ReleaseNotesLink))
            {
                writer.WriteElementString("sparkle", "releaseNotesLink", AppCastXMLGenerator.xmlns, ReleaseNotesLink);
            }

            if (PublicationDate.HasValue && PublicationDate.Value != DateTime.MinValue && PublicationDate.Value != DateTime.MaxValue)
            {
                //writer.WriteElementString("pubDate", PublicationDate.Value.ToString("r"));
                writer.WriteElementString("pubDate", PublicationDate.Value.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture));
            }

            if (!String.IsNullOrEmpty(DownloadLink))
            {
                writer.WriteStartElement("enclosure");
                writer.WriteAttributeString("url", DownloadLink);
                writer.WriteAttributeString("sparkle", "version", AppCastXMLGenerator.xmlns, Version);

                if (!String.IsNullOrEmpty(ShortVersion))
                {
                    writer.WriteAttributeString("sparkle", "shortVersionString", AppCastXMLGenerator.xmlns, ShortVersion);
                }

                writer.WriteAttributeString("length", Length);
                writer.WriteAttributeString("type", Type);

                if (!String.IsNullOrEmpty(DsaSignature))
                {
                    writer.WriteAttributeString("sparkle", "dsaSignature", AppCastXMLGenerator.xmlns, DsaSignature);
                }
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
