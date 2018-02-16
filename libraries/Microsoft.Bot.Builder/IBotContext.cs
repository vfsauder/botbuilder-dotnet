// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Middleware;

namespace Microsoft.Bot.Builder
{
    public interface IBotContext
    {
        Bot Bot { get; }

        /// <summary>
        /// Incoming request
        /// </summary>
        IActivity Request { get; }

        /// <summary>
        /// Respones
        /// </summary>
        IList<IActivity> Responses { get; set; }

        /// <summary>
        /// Conversation reference
        /// </summary>
        ConversationReference ConversationReference { get; }

        /// <summary>
        /// Bot state 
        /// </summary>
        BotState State { get; }
    
        Intent TopIntent { get; set; }

        /// <summary>
        /// check to see if topIntent matches
        /// </summary>
        /// <param name="intentName"></param>
        /// <returns></returns>
        bool IfIntent(string intentName);

        /// <summary>
        /// Check to see if intent matches regex
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        bool IfIntent(Regex expression);

        /// <summary>
        /// Queues a new "message" responses array.
        /// </summary>
        /// <param name="text">Text of a message to send to the user.</param>
        /// <param name="speak">(Optional) SSML that should be spoken to the user on channels that support speech.</param>
        /// <returns></returns>
        BotContext Reply(string text, string speak = null);

        /// <summary>
        /// Queues a new "message" responses array.
        /// </summary>
        /// <param name="activity">Activity object to send to the user.</param>
        /// <returns></returns>
        BotContext Reply(IActivity activity);

        /// <summary>
        /// Send a reply using a templateId bound to data
        /// </summary>
        /// <param name="templateId">template Id</param>
        /// <param name="data">data object to bind to</param>
        /// <returns></returns>
        BotContext ReplyWith(string templateId, object data=null);
        
        TemplateManager TemplateManager { get; set; }
    }   

    public static partial class BotContextExtension
    {
        //public static async Task Send(this BotContext context)
        //{            
        //    await context.SendActivity(context, new List<IActivity>()).ConfigureAwait(false);
        //}

        public static BotContext ToBotContext(this IBotContext context)
        {
            return (BotContext)context; 
        }

        public static void Trace(this IBotContext context, string traceMessage, object content = null)
        {
            try
            {
                var task = GetTraceTask(context, traceMessage, content);
                if (task != null)
                {
                    Task.Run(() => task.ConfigureAwait(false));
                }
            }
            catch(Exception)
            {
                // Do not allow trace exceptions to surface
            }
        }

        public static async Task TraceAsync(this IBotContext context, string traceMessage, object content = null)
        {
            try
            {
                var task = GetTraceTask(context, traceMessage, content);
                if (task != null)
                {
                    await task.ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // Do not allow trace exceptions to surface
            }
        }

        private static Task GetTraceTask(this IBotContext context, string traceMessage, object content = null)
        {
            if (context.Bot.Logger != null)
            {
                var trace = ((Activity)context.Request).CreateTraceActivity(traceMessage, content);
                trace.RelatesTo = context.ConversationReference;
                return context.Bot.Logger.TraceAsync(trace);
            }
            return null;
        }
    }
}
