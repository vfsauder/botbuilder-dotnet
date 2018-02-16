// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// A trace / logging activity
    /// </summary>
    public interface ITraceActivity : IActivity
    {
        /// <summary>
        /// Name of the trace event
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Open-ended value 
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Reference to the target conversation or activity to which this trace corresponds
        /// </summary>
        ConversationReference RelatesTo { get; set; }
    }
}
