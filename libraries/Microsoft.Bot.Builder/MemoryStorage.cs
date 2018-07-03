﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// Models IStorage around a dictionary 
    /// </summary>
    public class MemoryStorage : IStorage
    {
        private static readonly JsonSerializer StateJsonSerializer = new JsonSerializer() { TypeNameHandling = TypeNameHandling.All };

        private readonly Dictionary<string, JObject> _memory;
        private readonly object _syncroot = new object();
        private int _eTag = 0;

        public MemoryStorage(Dictionary<string, JObject> dictionary = null)
        {
            _memory = dictionary ?? new Dictionary<string, JObject>();
        }
                
        public Task Delete(string[] keys, CancellationToken cancellationToken)
        {
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    _memory.Remove(key);
                }
            }

            return Task.CompletedTask;
        }

        public Task<IDictionary<string, object>> Read(string[] keys, CancellationToken cancellationToken)
        {
            var storeItems = new Dictionary<string, object>(keys.Length);
            lock (_syncroot)
            {
                foreach (var key in keys)
                {
                    if (_memory.TryGetValue(key, out var state))
                    {
                        if (state != null)
                        {
                            storeItems.Add(key, state.ToObject<object>(StateJsonSerializer));
                        }
                    }
                }
            }

            return Task.FromResult<IDictionary<string, object>>(storeItems);
        }
        
        public Task Write(IDictionary<string, object> changes, CancellationToken cancellationToken)
        {
            lock (_syncroot)
            {
                foreach (var change in changes)
                {
                    var newValue = change.Value;

                    var oldStateETag = default(string);

                    if (_memory.TryGetValue(change.Key, out var oldState))
                    {
                        if (oldState.TryGetValue("eTag", out var eTagToken))
                        {
                            oldStateETag = eTagToken.Value<string>();
                        }
                    }
                    
                    var newState = JObject.FromObject(newValue, StateJsonSerializer);

                    // Set ETag if applicable
                    if (newValue is IStoreItem newStoreItem)
                    {
                        if (oldStateETag != null
                                &&
                           newStoreItem.eTag != "*"
                                &&
                           newStoreItem.eTag != oldStateETag)
                        {
                            throw new Exception($"Etag conflict.\r\n\r\nOriginal: {newStoreItem.eTag}\r\nCurrent: {oldStateETag}");
                        }

                        newState["eTag"] = (_eTag++).ToString();
                    }

                    _memory[change.Key] = newState;
                }
            }

            return Task.CompletedTask;
        }
    }
}
