﻿using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rocket.API.Configuration;
using Rocket.Core.Configuration.JsonNetBase;

namespace Rocket.Core.Configuration.Xml
{
    public class XmlConfiguration : JsonNetConfigurationBase
    {
        private const string ConfigRoot = "Config";

        protected override string FileEnding => "xml";
        public override string Name => "Xml";

        protected override void LoadFromFile(string file)
        {
            string xml = File.ReadAllText(file);
            LoadFromXml(xml);
        }

        public void LoadFromXml(string xml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            string json = JsonConvert.SerializeXmlNode(doc);

            Node = JObject.Parse(json, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore,
                LineInfoHandling = LineInfoHandling.Ignore
            })
                          .GetValue(ConfigRoot)
                          .DeepClone();

            IsLoaded = true;
        }

        protected override void SaveToFile(string file)
        {
            File.WriteAllText(file, ToXml());
        }

        public override IConfigurationElement Clone()
        {
            var config = new XmlConfiguration();
            config.LoadFromXml(ToXml());
            return config;
        }

        public string ToXml()
        {
            JToken clone = Node.DeepClone();
            string json = clone.ToString();

            //Appends <Config></Config>
            XmlDocument parent = new XmlDocument();
            XmlDeclaration xmlDeclaration = parent.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = parent.DocumentElement;
            parent.InsertBefore(xmlDeclaration, root);
            XmlElement configElement = parent.CreateElement(string.Empty, ConfigRoot, string.Empty);
            parent.AppendChild(configElement);

            XmlDocument jsonDocument = JsonConvert.DeserializeXmlNode(json);
            foreach (XmlNode elem in jsonDocument)
            {
                var copied = parent.ImportNode(elem, true);
                configElement.AppendChild(copied);
            }

            //Convert XmlDocument to String with a format
            StringBuilder sb = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };
            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                parent.Save(writer);
            }
            return sb.ToString();
        }
    }

}