﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public delegate Task NextDelegate(CancellationToken cancellationToken);

    /// <summary>
    /// Represents middleware that can operate on incoming activities.
    /// </summary>
    /// <remarks>A <see cref="BotAdapter"/> passes incoming activities from the user's 
    /// channel to the middleware's <see cref="OnTurn(ITurnContext, NextDelegate)"/>
    /// method.
    /// <para>You can add middleware objects to your adapter’s middleware collection. The
    /// adapter processes and directs incoming activities in through the bot middleware 
    /// pipeline to your bot’s logic and then back out again. As each activity flows in 
    /// and out of the bot, each piece of middleware can inspect or act upon the activity, 
    /// both before and after the bot logic runs.</para>
    /// <para>For each activity, the adapter calls middleware in the order in which you 
    /// added it.</para>
    /// </remarks>
    /// <example>
    /// This defines middleware that sends "before" and "after" messages
    /// before and after the adapter calls the bot's 
    /// <see cref="IBot.OnTurn(ITurnContext)"/> method.
    /// <code>
    /// public class SampleMiddleware : IMiddleware
    /// {
    ///     public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
    ///     {
    ///         context.SendActivity("before");
    ///         await next().ConfigureAwait(false);
    ///         context.SendActivity("after");
    ///     }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IBot"/>
    public interface IMiddleware
    {
        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Middleware calls the <paramref name="next"/> delegate to pass control to 
        /// the next middleware in the pipeline. If middleware doesn’t call the next delegate,
        /// the adapter does not call any of the subsequent middleware’s request handlers or the 
        /// bot’s receive handler, and the pipeline short circuits.
        /// <para>The <paramref name="context"/> provides information about the 
        /// incoming activity, and other data needed to process the activity.</para>
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        Task OnTurn(ITurnContext context, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken));
    }
}
