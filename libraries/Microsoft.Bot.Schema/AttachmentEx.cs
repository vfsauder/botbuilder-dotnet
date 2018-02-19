// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// Addition helper code for Attachment objects
    /// </summary>
    public partial class Attachment : IXmlSerializable
    {
        /// <summary>
        /// Extension data for overflow of properties
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        [XmlIgnore]
        public JObject Properties { get; set; } = new JObject();

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            this.ContentType = reader.GetAttribute(nameof(ContentType));
            this.ContentUrl = reader.GetAttribute(nameof(ContentUrl));
            this.Name = reader.GetAttribute(nameof(Name));
            this.ThumbnailUrl = reader.GetAttribute(nameof(ThumbnailUrl));
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                this.Content = reader.ReadContentAsString();
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.ContentType != null)
                writer.WriteAttributeString(nameof(ContentType), this.ContentType);
            if (this.ContentUrl != null)
                writer.WriteAttributeString(nameof(ContentUrl), this.ContentUrl);
            if (this.Name != null)
                writer.WriteAttributeString(nameof(Name), this.Name);
            if (this.ThumbnailUrl != null)
                writer.WriteAttributeString(nameof(ThumbnailUrl), this.ThumbnailUrl);
            if (this.Content != null)
            {
                writer.WriteString(this.Content.ToString());
            }
        }

    }
}
