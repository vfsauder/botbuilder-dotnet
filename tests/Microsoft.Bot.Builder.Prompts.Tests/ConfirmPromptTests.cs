// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Confirm Prompts")]
    public class ConfirmPromptTests
    {
        private TestFlow Test(IList<Activity> transcript, Func<ITurnContext, Task> middleware)
        {
            TestAdapter adapter = new TestAdapter()
                .Use(new ConversationState<TestState>(new MemoryStorage()));

            var testFlow = new TestFlow(adapter, middleware);
            foreach (var activity in transcript)
            {
                if (string.Equals("bot", activity.From?.Role, System.StringComparison.InvariantCultureIgnoreCase))
                {
                    testFlow.AssertReply(activity);
                }
                else
                {
                    testFlow.Send(activity);
                }
            }

            return testFlow;
        }

        private Activity BotMessage(string text)
        {
            var message = MessageFactory.Text(text);
            message.From = new ChannelAccount { Role = "bot" };
            return message;
        }

        private IDictionary<string, IList<Activity>> transcriptFiles = new Dictionary<string, IList<Activity>>();

        public ConfirmPromptTests()
        {
            var confirmPromptTestTranscript = new List<Activity> {
                { MessageFactory.Text("hello") },
                { BotMessage("Gimme:") },
                { MessageFactory.Text("tyest tnot") },
                { BotMessage(PromptStatus.NotRecognized.ToString()) },
                { MessageFactory.Text(".. yes please ") },
                { BotMessage("True") },
                { MessageFactory.Text(".. no thank you") },
                { BotMessage("False") }
            };
            transcriptFiles.Add("ConfirmPromptTests.ConfirmPrompt_Test", confirmPromptTestTranscript);

            var confirmPromptValidatorTranscript = new List<Activity> {
                { MessageFactory.Text("hello") },
                { BotMessage("Gimme:") },
                { MessageFactory.Text(" yes you xxx") },
                { BotMessage(PromptStatus.NotRecognized.ToString()) },
                { MessageFactory.Text(" no way you xxx") },
                { BotMessage(PromptStatus.NotRecognized.ToString()) },
                { MessageFactory.Text(" yep") },
                { BotMessage("True") },
                { MessageFactory.Text(" nope") },
                { BotMessage("False") }
            };
            transcriptFiles.Add("ConfirmPromptTests.ConfirmPrompt_Validator", confirmPromptValidatorTranscript);
        }

        [TestMethod]
        public async Task ConfirmPrompt_Test()
        {
            var transcript = transcriptFiles["ConfirmPromptTests.ConfirmPrompt_Test"];

            Func<ITurnContext, Task> middleware = async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var testPrompt = new ConfirmPrompt(Culture.English);
                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await testPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await testPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                    {
                        Assert.IsNotNull(confirmResult.Text);
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    }
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            };

            var testFlow = Test(transcript, middleware);
            await testFlow.StartTest();
        }

        [TestMethod]
        public async Task ConfirmPrompt_Validator()
        {
            var transcript = transcriptFiles["ConfirmPromptTests.ConfirmPrompt_Validator"];

            Func<ITurnContext, Task> middleware = async (context) =>
            {
                var state = ConversationState<TestState>.Get(context);
                var confirmPrompt = new ConfirmPrompt(Culture.English, async (ctx, result) =>
                {
                    if (ctx.Activity.Text.Contains("xxx"))
                        result.Status = PromptStatus.NotRecognized;
                });

                if (!state.InPrompt)
                {
                    state.InPrompt = true;
                    await confirmPrompt.Prompt(context, "Gimme:");
                }
                else
                {
                    var confirmResult = await confirmPrompt.Recognize(context);
                    if (confirmResult.Succeeded())
                        await context.SendActivity($"{confirmResult.Confirmation}");
                    else
                        await context.SendActivity(confirmResult.Status.ToString());
                }
            };

            var testFlow = Test(transcript, middleware);
            await testFlow.StartTest();
        }

    }
}