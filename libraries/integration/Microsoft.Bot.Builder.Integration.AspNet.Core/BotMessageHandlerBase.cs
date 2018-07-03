﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Integration.AspNet.Core.Handlers
{
    public abstract class BotMessageHandlerBase
    {
        public static readonly JsonSerializer BotMessageSerializer = JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter> { new Iso8601TimeSpanConverter() }
        });

        public BotMessageHandlerBase()
        {
        }

        public async Task HandleAsync(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            if (request.Method != HttpMethods.Post)
            {
                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                return;
            }

            if (request.ContentLength == 0)
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;

                return;
            }

            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaTypeHeaderValue)
                    ||
                mediaTypeHeaderValue.MediaType != "application/json")
            {
                response.StatusCode = (int)HttpStatusCode.NotAcceptable;

                return;
            }

            var requestServices = httpContext.RequestServices;
            var botFrameworkAdapter = requestServices.GetRequiredService<BotFrameworkAdapter>();
            var bot = requestServices.GetRequiredService<IBot>();

            try
            {
                // TODO wire up cancellation

                var invokeResponse = await ProcessMessageRequestAsync(
                    request,
                    botFrameworkAdapter,
                    bot.OnTurn,
                    default(CancellationToken));

                if (invokeResponse == null)
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    response.ContentType = "application/json";
                    response.StatusCode = invokeResponse.Status;

                    using (var writer = new StreamWriter(response.Body))
                    {
                        using (var jsonWriter = new JsonTextWriter(writer))
                        {
                            BotMessageSerializer.Serialize(jsonWriter, invokeResponse.Body);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        protected abstract Task<InvokeResponse> ProcessMessageRequestAsync(HttpRequest request, BotFrameworkAdapter botFrameworkAdapter, Func<ITurnContext, Task> botCallbackHandler, CancellationToken cancellationToken);
    }
}