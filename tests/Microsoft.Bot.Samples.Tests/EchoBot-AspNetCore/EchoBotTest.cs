// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Bot.Samples.Echo.AspNetCore;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Samples.Tests.EchoBot_AspNetCore
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
            
            await flow.Test(activities, (expected, actual) => {
                Assert.AreEqual(expected.Type, actual.Type, "Type should match");
                if (expected.Type == ActivityTypes.Message)
                {
                    Assert.AreEqual(expected.AsMessageActivity().Text, actual.AsMessageActivity().Text);
                }
            }).StartTest();
        }
    }
}
