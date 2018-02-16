// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Microsoft.Bot.Builder.Middleware.MiddlewareSet;

namespace Microsoft.Bot.Builder.Middleware
{
    /// <summary>
    /// Middleware that automatically sends received and sent Activities to the logger
    /// </summary>
    public class LoggerMiddleware : IMiddleware, IReceiveActivity, ISendActivity
    {
        private Bot _bot;
        
        /// <summary>
        /// Creates a new instance of the LoggerMiddleware to peform logging for received and sent Activities
        /// </summary>
        /// <param name="bot">The bot</param>
        public LoggerMiddleware(Bot bot)
        {
            this._bot = bot;
        }

        /// <summary>
        /// Trace the received activity
        /// </summary>
        /// <param name="context">The bot context</param>
        /// <param name="next">Next item in the middleware chain</param>
        public async Task ReceiveActivity(IBotContext context, NextDelegate next)
        {
            if (this._bot.Logger != null && context.Request != null)
            {
                await this._bot.Logger.TraceAsync(context.Request).ConfigureAwait(false);
            }
            await next().ConfigureAwait(false);
        }

        /// <summary>
        /// Trace all outgoing activities
        /// </summary>
        /// <param name="context">The bot context</param>
        /// <param name="activities">The list of activities that are being sent</param>
        /// <param name="next">Next item in the middleware chain</param>
        public async Task SendActivity(IBotContext context, IList<IActivity> activities, NextDelegate next)
        {
            if (this._bot.Logger != null && activities != null)
            {
                foreach (var activity in activities)
                {
                    await this._bot.Logger.TraceAsync(activity).ConfigureAwait(false);
                }
            }
            await next().ConfigureAwait(false);
        }
    }
}
