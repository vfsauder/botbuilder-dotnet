﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder
{
    public class StateSettings
    { 
        public bool LastWriterWins { get; set; } = true;
    }

    /// <summary>
    /// Base class which manages details of automatic loading and saving of bot state.
    /// </summary>
    /// <typeparam name="TState">The type of the bot state object.</typeparam>
    public class BotState<TState> : IMiddleware
        where TState : class, new()
    {
        private readonly StateSettings _settings;
        private readonly IStorage _storage;
        private readonly Func<ITurnContext, string> _keyDelegate;
        private readonly string _propertyName;

        /// <summary>
        /// Creates a new <see cref="BotState{TState}"/> middleware object.
        /// </summary>
        /// <param name="storage">The storage provider to use.</param>
        /// <param name="propertyName">The name to use to load or save the state object.</param>
        /// <param name="keyDelegate"></param>
        /// <param name="settings">The state persistance options to use.</param>
        public BotState(IStorage storage, string propertyName, Func<ITurnContext, string> keyDelegate, StateSettings settings = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            _keyDelegate = keyDelegate ?? throw new ArgumentNullException(nameof(keyDelegate));
            _settings = settings ?? new StateSettings();
        }

        /// <summary>
        /// Processess an incoming activity.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>This middleware loads the state object on the leading edge of the middleware pipeline
        /// and persists the state object on the trailing edge.
        /// </remarks>
        public async Task OnTurn(ITurnContext context, NextDelegate next, CancellationToken cancellationToken)
        {
            await ReadToContextService(context, cancellationToken).ConfigureAwait(false);
            await next(cancellationToken).ConfigureAwait(false);
            await WriteFromContextService(context, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task ReadToContextService(ITurnContext context, CancellationToken cancellationToken)
        {
            var key = _keyDelegate(context);
            var items = await _storage.Read(new[] { key }, cancellationToken);
            var state = items.Where(entry => entry.Key == key).Select(entry => entry.Value).OfType<TState>().FirstOrDefault();
            if (state == null)
                state = new TState();
            context.Services.Add(_propertyName, state);
        }

        protected virtual async Task WriteFromContextService(ITurnContext context, CancellationToken cancellationToken)
        {
            var state = context.Services.Get<TState>(_propertyName);
            await Write(context, state, cancellationToken);
        }

        /// <summary>
        /// Reads state from storage.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>If successful, the task result contains the state object, read from storage.</remarks>
        public virtual async Task<TState> Read(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var key = _keyDelegate(context);
            var items = await _storage.Read(new[] { key }, cancellationToken).ConfigureAwait(false);
            var state = items.Where(entry => entry.Key == key).Select(entry => entry.Value).OfType<TState>().FirstOrDefault();

            if (state == null)
            {
                state = new TState();
            }

            return state;
        }

        /// <summary>
        /// Writes state to storage.
        /// </summary>
        /// <param name="context">The context object for this turn.</param>
        /// <param name="state">The state object.</param>
        public virtual async Task Write(ITurnContext context, TState state, CancellationToken cancellationToken = default(CancellationToken))
        {
            var changes = new Dictionary<string, object>();

            if (state == null)
                state = new TState();
            var key = _keyDelegate(context);

            changes.Add(key, state);

            if (_settings.LastWriterWins)
            {
                foreach (var item in changes)
                {
                    if (item.Value is IStoreItem valueStoreItem)
                    {
                        valueStoreItem.eTag = "*";
                    }
                }
            }

            await _storage.Write(changes, cancellationToken).ConfigureAwait(false);
        }
    }
}
