using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Core.Tests
{
    [TestClass]
    [TestCategory("Middleware")]
    public class BotAdapterBracketingTest : BaseTest
    {
        /// <summary>
        /// Developer authored Middleware that looks like this:
        /// public async Task ReceiveActivity(ITurnContext context, 
        ///    MiddlewareSet.NextDelegate next)
        /// {
        ///    context.Reply("BEFORE");
        ///    await next();   // User Says Hello
        ///    context.Reply("AFTER");
        ///  }
        ///  Should result in an output that looks like:
        ///    BEFORE
        ///    ECHO:Hello
        ///    AFTER        
        /// </summary>       
        [TestMethod]
        public async Task Middlware_BracketingValidation()
        {
            await DoTest(new IMiddleware[] { new BeforeAFterMiddlware() }, (context) => {
                var response = "ECHO:" + context.Activity.AsMessageActivity().Text;
                return context.SendActivity(response);
            });
        }

        /// <summary>
        /// Exceptions thrown during the processing of an Activity should
        /// be catchable by Middleware that has wrapped the next() method. 
        /// This tests verifies that, and makes sure the order of messages
        /// coming back is correct. 
        /// </summary>       
        [TestMethod]
        public async Task Middlware_ThrowException()
        {
            await DoTest(new IMiddleware[] { new CatchExceptionMiddleware() }, async (context) => {
                string toEcho = "ECHO:" + context.Activity.AsMessageActivity().Text;
                await context.SendActivity(context.Activity.CreateReply(toEcho));
                throw new Exception("Test Exception");
            });
        }

        public class CatchExceptionMiddleware : IMiddleware
        {
            public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity(context.Activity.CreateReply("BEFORE"));
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    await context.SendActivity(context.Activity.CreateReply("CAUGHT:" + ex.Message));                    
                }

                await context.SendActivity(context.Activity.CreateReply("AFTER"));
            }

        }

        public class BeforeAFterMiddlware : IMiddleware
        {
            public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
            {
                await context.SendActivity(context.Activity.CreateReply("BEFORE"));
                await next();
                await context.SendActivity(context.Activity.CreateReply("AFTER"));
            }

        }
    }
}