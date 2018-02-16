// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Middleware
{
    public interface ILogger
    {
        Task TraceAsync(IActivity activity);
    }
}
