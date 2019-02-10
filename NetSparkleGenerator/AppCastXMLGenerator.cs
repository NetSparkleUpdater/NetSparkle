using System;
using System.Collections.Generic;
using System.Xml;

namespace NetSparkleGenerator
{
    class AppCastXMLGenerator
    {
        public static string xmlns = "http://www.andymatuschak.org/xml-namespaces/sparkle";

        public string Title;
        public string Link;
        public string Description;
        public string Language;
        public List<AppCastXMLItem> Items;

        public AppCastXMLGenerator(string title, string link = "", string description = "", string language = "en")
        {
            Title = title;
            Link = link;
            Description = description;
            Language = language;
            Items = new List<AppCastXMLItem>();
        }

        public void AddItem(AppCastXMLItem item)
        {
            Items.Add(item);
        }

        public XmlDocument GenerateAppCast()
        {
            var document = new XmlDocument();
            var writer = document.CreateNavigator().AppendChild();

            WriteStartDocument(writer);
            WriteStartChannel(writer);

            foreach (var item in Items)
            {
                item.WriteItem(writer);
            }

            WriteEndChannel(writer);
            WriteEndDocument(writer);

            writer.Flush();
            writer.Close();

            return document;
        }

        private void WriteStartDocument(XmlWriter writer)
        {
            writer.WriteStartDocument(true);
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteAttributeString("xmlns", "sparkle", null, xmlns);
        }

        private void WriteEndDocument(XmlWriter writer)
        {
            writer.WriteEndElement(); //rss
            writer.WriteEndDocument();
        }

        private void WriteStartChannel(XmlWriter writer)
        {
            writer.WriteStartElement("channel");
            writer.WriteElementString("title", Title);

            if (!String.IsNullOrEmpty(Link))
            {
                writer.WriteElementString("link", Link);
            }

            if (!String.IsNullOrEmpty(Description))
            {
                writer.WriteElementString("description", Description);
            }

            writer.WriteElementString("language", Language);
        }

        private void WriteEndChannel(XmlWriter writer)
        {
            writer.WriteEndElement(); //channel
        }

    }
}
