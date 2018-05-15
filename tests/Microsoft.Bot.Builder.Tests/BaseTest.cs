using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        public TestContext TestContext { get; set; }

        protected IEnumerable<Activity> GetTranscript()
        {
            var transcriptsRootFolder = TestUtilities.GetKey("TranscriptsRootFolder") ?? @"..\..\..\..\..\transcripts";
            var transcriptFilename = $@"{TestContext.FullyQualifiedTestClassName.Split('.').Last()}\{TestContext.TestName}.transcript";
            var transcriptPath = $@"{transcriptsRootFolder}\{transcriptFilename}";

            var transcriptContent = File.ReadAllText(transcriptPath);
            var transcript = JsonConvert.DeserializeObject<Activity[]>(transcriptContent);

            return transcript;
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
    }
}
