﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Recognizer return value.
    /// </summary>
    public class RecognizerResult : IRecognizerConvert
    {
        /// <summary>
        /// Gets or sets original text to recognizer.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets text modified by recognizer for example by spell correction.
        /// </summary>
        [JsonProperty("alteredText")]
        public string AlteredText { get; set; }

        /// <summary>
        /// Gets or sets object with the intent as key and the confidence as value.
        /// </summary>
        [JsonProperty("intents")]
        public JObject Intents { get; set; }

        /// <summary>
        /// Gets or sets object with each top-level recognized entity as a key.
        /// </summary>
        [JsonProperty("entities")]
        public JObject Entities { get; set; }

        /// <summary>
        /// Gets or sets any extra properties to include in the results.
        /// </summary>
        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public void Convert(dynamic result)
        {
            Text = result.Text;
            AlteredText = result.AlteredText;
            Intents = result.Intents;
            Entities = result.Entities;
            Properties = result.Properties;
        }
    }
}
