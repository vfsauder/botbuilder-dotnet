// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// ConversationAccount extension
    /// </summary>
    public partial class ConversationAccount : IXmlSerializable
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
            this.Id = reader.GetAttribute(nameof(Id));
            this.Name = reader.GetAttribute(nameof(Name));
            if (bool.TryParse(reader.GetAttribute(nameof(IsGroup)), out bool isGroup))
                IsGroup = isGroup;
            bool isEmptyElement = reader.IsEmptyElement;
            reader.ReadStartElement();
            if (!isEmptyElement)
            {
                reader.ReadEndElement();
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            if (this.Id != null)
                writer.WriteAttributeString(nameof(Id), this.Id);
            if (this.Name != null)
                writer.WriteAttributeString(nameof(Name), this.Name);
            if (this.IsGroup != null)
                writer.WriteAttributeString(nameof(IsGroup), this.IsGroup.ToString());
        }
    }
}
