// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Http;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core
{
    public class BotFrameworkPaths
    {
        public BotFrameworkPaths()
        {
            this.BasePath = "/api";
            this.MessagesPath = "/messages";
            this.ExternalEventsPath = "/events";
        }

        /// <summary>
        /// Gets or sets the base path at which the bot's endpoints should be exposed.
        /// </summary>
        /// <value>
        /// A <see cref="PathString"/> that represents the base URL at which the bot should be exposed. The default is: <code>api/</code>
        /// </value>
        public PathString BasePath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which the bot framework messages are expected to be delivered.
        /// </summary>
        /// <value>
        /// A <see cref="PathString"/> that represents the URL at which the bot framework messages are expected to be delivered. The default is: <code>&lt;base-path&gt;/messages</code>
        /// </value>
        public PathString MessagesPath { get; set; }

        /// <summary>
        /// Gets or sets the path, relative to the <see cref="BasePath"/>, at which external events are expected to be delivered.
        /// </summary>
        /// <value>
        /// A <see cref="PathString"/> that represents the URL at which external events will be received. The default is: <code>&lt;base-path&gt;/events</code>
        /// </value>
        /// <remarks>
        /// This path is only utilized if <see cref="BotFrameworkOptions.EnableExternalEventsEndpoint">the external events endpoint has been enabled</see>.
        /// </remarks>
        /// <seealso cref="BotFrameworkOptions.EnableExternalEventsEndpoint"/>
        public PathString ExternalEventsPath { get; set; }
    }
}