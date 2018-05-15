// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Tests;
using Microsoft.Recognizers.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Prompts.Tests
{
    [TestClass]
    [TestCategory("Prompts")]
    [TestCategory("Confirm Prompts")]
    public class ConfirmPromptTests : BaseTest
    {
        [TestMethod]
        public async Task ConfirmPrompt_Test()
        {
            await DoTest<TestState>(async (context) =>
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
            });
        }

        [TestMethod]
        public async Task ConfirmPrompt_Validator()
        {
            await DoTest<TestState>(async (context) =>
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
            });
        }

        public override IDictionary<string, IList<Activity>> TranscriptFiles { get; }

        public ConfirmPromptTests()
        {
            TranscriptFiles = new Dictionary<string, IList<Activity>>();
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
            TranscriptFiles.Add("ConfirmPromptTests.ConfirmPrompt_Test", confirmPromptTestTranscript);

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
            TranscriptFiles.Add("ConfirmPromptTests.ConfirmPrompt_Validator", confirmPromptValidatorTranscript);
        }
    }
}