﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// When added, this middleware will send typing activities back to the user when a Message activity
    /// is receieved to let them know that the bot has receieved the message and is working on the response.
    /// You can specify a delay in milliseconds before the first typing activity is sent and then a frequency,
    /// also in milliseconds which determines how often another typing activity is sent. Typing activities
    /// will continue to be sent until your bot sends another message back to the user.
    /// </summary>
    public class ShowTypingMiddleware : IMiddleware
    {
        private readonly TimeSpan _delay;
        private readonly TimeSpan _period;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowTypingMiddleware"/> class.
        /// </summary>
        /// <param name="delay">Initial delay before sending first typing indicator. Defaults to 500ms.</param>
        /// <param name="period">Rate at which additional typing indicators will be sent. Defaults to every 2000ms.</param>
        public ShowTypingMiddleware(int delay = 500, int period = 2000)
        {
            if (delay < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be greater than or equal to zero");
            }

            if (period <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(period), "Repeat period must be greater than zero");
            }

            _delay = TimeSpan.FromMilliseconds(delay);
            _period = TimeSpan.FromMilliseconds(period);
        }

        public async Task OnTurn(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            CancellationTokenSource cts = null;
            try
            {
                // If the incoming activity is a MessageActivity, start a timer to periodically send the typing activity
                if (context.Activity.Type == ActivityTypes.Message)
                {
                    cts = new CancellationTokenSource();
                    cancellationToken.Register(() => cts.Cancel());

                    // do not await task - we want this to run in thw background and we wil cancel it when its done
                    var task = Task.Run(() => SendTypingAsync(context, _delay, _period, cts.Token), cancellationToken);
                }

                await next(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (cts != null)
                {
                    cts.Cancel();
                }
            }
        }

        private static async Task SendTypingAsync(ITurnContext context, TimeSpan delay, TimeSpan period, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await SendTypingActivityAsync(context, cancellationToken).ConfigureAwait(false);
                    }

                    // if we happen to cancel when in the delay we will get a TaskCanceledException
                    await Task.Delay(period, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                // do nothing
            }
        }

        private static async Task SendTypingActivityAsync(ITurnContext context, CancellationToken cancellationToken)
        {
            // create a TypingActivity, associate it with the conversation and send immediately
            var typingActivity = new Activity
            {
                Type = ActivityTypes.Typing,
                RelatesTo = context.Activity.RelatesTo,
            };
            await context.SendActivity(typingActivity, cancellationToken).ConfigureAwait(false);
        }
    }
}
