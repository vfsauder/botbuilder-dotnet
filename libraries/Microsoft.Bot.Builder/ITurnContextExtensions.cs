﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.TraceExtensions
{
    public static class ITurnContextExtensions
    {
        /// <summary>
        /// Send a TraceActivity to transcript and/or emulator if attached
        /// </summary>
        /// <param name="turnContext"></param>
        /// <param name="name">Name of the operation</param>
        /// <param name="value">value of the operation</param>
        /// <param name="valueType">valueType if helpful to identify the value schema (default is value.GetType().Name)</param>
        /// <param name="label">descritive label of context. (Default is calling function name)</param>
        /// <returns></returns>
        public static Task<ResourceResponse> TraceActivity(this ITurnContext turnContext, string name, object value = null, string valueType = null, [CallerMemberName] string label = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return turnContext.SendActivity(turnContext.Activity.CreateTrace(name, value, valueType, label), cancellationToken);
        }
    }
}
