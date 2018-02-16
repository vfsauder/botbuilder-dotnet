using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Middleware
{
    public class StorageLogger : ILogger
    {
        private const string LogKeyRoot = @"botlog";
        private IStorage _storage;

        public StorageLogger(IStorage storage)
        {
            this._storage = storage;
        }

        public async Task TraceAsync(IActivity activity)
        {
            var ts = DateTime.UtcNow;

            var item = new StoreItem();
            item["timestamp"] = ts;
            item["activity"] = activity;

            var changes = new StoreItems();
            changes[ConversationKey(activity, ts)] = item;

            await this._storage.Write(changes).ConfigureAwait(false);
        }

        private static string ConversationKey(IActivity activity, DateTime ts)
        {
            return $"{LogKeyRoot}_{activity.ChannelId}_{activity.Conversation.Id}_{ts.ToString("o")}_{activity.Id}.json";
        }
    }
}
