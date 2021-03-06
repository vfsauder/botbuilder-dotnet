﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Tuple class containing an HTTP Status Code and a JSON Serializable
    /// object. The HTTP Status code is, in the invoke activity scenario, what will
    /// be set in the resulting POST. The Body of the resulting POST will be 
    /// the JSON Serialized content from the Body property. 
    /// </summary>
    public class InvokeResponse
    {
        /// <summary>
        /// The POST that is generated in response to the incoming Invoke Activity
        /// will have the HTTP Status code specificied by this field.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// The POST that is generated in response to the incoming Invoke Activity
        /// will have a body generated by JSON serializing the object in the Body field.
        /// </summary>
        public object Body { get; set; }
    }
}
