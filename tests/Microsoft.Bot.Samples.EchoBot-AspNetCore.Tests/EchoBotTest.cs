// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Samples.Echo.AspNetCore.Tests
{
    [TestClass]
    [TestCategory("Samples")]
    [TestCategory("EchoBot")]
    public class EchoBotTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task EchoBot_Test()
        {
            var activities = TranscriptUtilities.GetFromTestContext(TestContext);
            var conversationReference = activities.First().GetConversationReference();

            // This adapter needs a custom conversation reference
            // to match the echobot behavior
            TestAdapter adapter = new TestAdapter(conversationReference);

            var flow = new TestFlow(adapter, new EchoBot());
            
            await flow.Test(activities).StartTest();
        }
    }
}
