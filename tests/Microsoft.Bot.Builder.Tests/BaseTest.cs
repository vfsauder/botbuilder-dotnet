using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        private TestContext testContext;

        public TestContext TestContext
        {
            get { return testContext; }
            set { testContext = value; }
        }

        public abstract IDictionary<string, IList<Activity>> TranscriptFiles { get; }

        protected IList<Activity> GetTranscript()
        {
            var key = TestContext.FullyQualifiedTestClassName.Split('.').Last() + "." + TestContext.TestName;
            return TranscriptFiles[key];
        }

        protected Task DoTest<T>(Func<ITurnContext, Task> middlewareHandler) where T: class, new()
        {
            var transcript = GetTranscript();
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<T>(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, middlewareHandler);
            foreach (var activity in transcript)
            {
                if (string.Equals("bot", activity.From?.Role, StringComparison.InvariantCultureIgnoreCase))
                {
                    testFlow.AssertReply(activity);
                }
                else
                {
                    testFlow.Send(activity);
                }
            }
            return testFlow.StartTest();
        }
        protected Activity BotMessage(string text)
        {
            var message = MessageFactory.Text(text);
            message.From = new ChannelAccount { Role = "bot" };
            return message;
        }

    }
}
