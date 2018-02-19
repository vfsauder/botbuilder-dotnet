// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Bot.Schema
{
    /// <summary>
    /// The referenced conversation has been updated
    /// </summary>
    public interface IConversationUpdateActivity : IActivity
    {
        /// <summary>
        /// Members added to the conversation
        /// </summary>
        List<ChannelAccount> MembersAdded { get; set; }

        /// <summary>
        /// Members removed from the conversation
        /// </summary>
        List<ChannelAccount> MembersRemoved { get; set; }

        /// <summary>
        /// The conversation's updated topic name
        /// </summary>
        string TopicName { get; set; }

        /// <summary>
        /// True if prior history of the channel is disclosed
        /// </summary>
        bool? HistoryDisclosed { get; set; }
    }
}
