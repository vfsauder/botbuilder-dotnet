﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Ai.QnA
{
    /// <summary>
    /// Provides access to a QnA Maker knowledge base.
    /// </summary>
    public class QnAMaker
    {
        private static readonly HttpClient DefaultHttpClient = new HttpClient();
        private readonly HttpClient _httpClient;

        private readonly QnAMakerEndpoint _endpoint;
        private readonly QnAMakerOptions _options;

        /// <summary>
        /// Creates a new <see cref="QnAMaker"/> instance.
        /// </summary>
        /// <param name="endpoint">The endpoint of the knowledge base to query.</param>
        /// <param name="options">The options for the QnA Maker knowledge base.</param>
        /// <param name="customHttpClient">An alternate client with which to talk to QnAMaker.
        /// If null, a default client is used for this instance.</param>
        public QnAMaker(QnAMakerEndpoint endpoint, QnAMakerOptions options = null, HttpClient customHttpClient = null)
        {
            _httpClient = customHttpClient ?? DefaultHttpClient;

            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

            if (string.IsNullOrEmpty(endpoint.KnowledgeBaseId))
            {
                throw new ArgumentException(nameof(endpoint.KnowledgeBaseId));
            }
            if (string.IsNullOrEmpty(endpoint.Host))
            {
                throw new ArgumentException(nameof(endpoint.Host));
            }
            if (string.IsNullOrEmpty(endpoint.EndpointKey))
            {
                throw new ArgumentException(nameof(endpoint.EndpointKey));
            }

            _options = options ?? new QnAMakerOptions();

            if (_options.ScoreThreshold == 0)
            {
                _options.ScoreThreshold = 0.3F;
            }
            if (_options.Top == 0)
            {
                _options.Top = 1;
            }
            if (_options.ScoreThreshold < 0 || _options.ScoreThreshold > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.ScoreThreshold), "Score threshold should be a value between 0 and 1");
            }
            if (_options.Top < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.Top), "Top should be an integer greater than 0");
            }

            if (_options.StrictFilters == null)
            {
                _options.StrictFilters = new Metadata[] { };
            }
            if (_options.MetadataBoost == null)
            {
                _options.MetadataBoost = new Metadata[] { };
            }
        }

        /// <summary>
        /// Generates an answer from the knowledge base.
        /// </summary>
        /// <param name="question">The user question to be queried against your knowledge base.</param>
        /// <returns>A list of answers for the user query, sorted in decreasing order of ranking score.</returns>
        public async Task<QueryResult[]> GetAnswers(string question)
        {
            var requestUrl = $"{_endpoint.Host}/knowledgebases/{_endpoint.KnowledgeBaseId}/generateanswer";

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

            string jsonRequest = JsonConvert.SerializeObject(new
            {
                question,
                top = _options.Top,
                strictFilters = _options.StrictFilters,
                metadataBoost = _options.MetadataBoost
            }, Formatting.None);

            request.Content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");

            bool isLegacyProtocol = (_endpoint.Host.EndsWith("v2.0") || _endpoint.Host.EndsWith("v3.0"));

            if (isLegacyProtocol)
            {
                request.Headers.Add("Ocp-Apim-Subscription-Key", _endpoint.EndpointKey);
            }
            else
            {
                request.Headers.Add("Authorization", $"EndpointKey {_endpoint.EndpointKey}");
            }

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                var results = isLegacyProtocol ?
                    ConvertLegacyResults(JsonConvert.DeserializeObject<InternalQueryResults>(jsonResponse))
                        :
                    JsonConvert.DeserializeObject<QueryResults>(jsonResponse);

                foreach (var answer in results.Answers)
                {
                    answer.Score = answer.Score / 100;
                }
                return results.Answers.Where(answer => answer.Score > _options.ScoreThreshold).ToArray();
            }

            return null;
        }

        // The old version of the protocol returns the id in a field called qnaId the
        // following classes and helper function translate this old structure
        private QueryResults ConvertLegacyResults(InternalQueryResults legacyResults)
        {
            return new QueryResults
            {
                Answers = legacyResults.Answers
                    .Select(answer => new QueryResult
                    {
                        // The old version of the protocol returns the "id" in a field called "qnaId"
                        Id = answer.QnaId,
                        Answer = answer.Answer,
                        Metadata = answer.Metadata,
                        Score = answer.Score,
                        Source = answer.Source,
                        Questions = answer.Questions
                    })
                    .ToArray()
            };
        }

        private class InternalQueryResult : QueryResult
        {
            [JsonProperty(PropertyName = "qnaId")]
            public int QnaId { get; set; }
        }

        private class InternalQueryResults
        {
            /// <summary>
            /// The answers for a user query,
            /// sorted in decreasing order of ranking score.
            /// </summary>
            [JsonProperty("answers")]
            public InternalQueryResult[] Answers { get; set; }
        }
    }

    /// <summary>
    /// Defines an endpoint used to connect to a QnA Maker Knowledge base.
    /// </summary>
    public class QnAMakerEndpoint
    {
        /// <summary>
        /// The knowledge base ID.
        /// </summary>
        public string KnowledgeBaseId { get; set; }

        /// <summary>
        /// The endpoint key for the knowledge base.
        /// </summary>
        public string EndpointKey { get; set; }

        /// <summary>
        /// The host path. For example "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0"
        /// </summary>
        public string Host { get; set; }
    }

    /// <summary>
    /// Defines options for the QnA Maker knowledge base.
    /// </summary>
    public class QnAMakerOptions
    {
        public QnAMakerOptions()
        {
            ScoreThreshold = 0.3f;
        }

        /// <summary>
        /// The minimum score threshold, used to filter returned results.
        /// </summary>
        /// <remarks>Scores are normalized to the range of 0.0 to 1.0 
        /// before filtering.</remarks>
        public float ScoreThreshold { get; set; }

        /// <summary>
        /// The number of ranked results you want in the output.
        /// </summary>
        public int Top { get; set; }

        public Metadata[] StrictFilters { get; set; }
        public Metadata[] MetadataBoost { get; set; }
    }

    [Serializable]
    public class Metadata
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    /// <summary>
    /// Represents an individual result from a knowledge base query.
    /// </summary>
    public class QueryResult
    {
        /// <summary>
        /// The list of questions indexed in the QnA Service for the given answer.
        /// </summary>
        [JsonProperty("questions")]
        public string[] Questions { get; set; }

        /// <summary>
        /// The answer text.
        /// </summary>
        [JsonProperty("answer")]
        public string Answer { get; set; }

        /// <summary>
        /// The answer's score, from 0.0 (least confidence) to
        /// 1.0 (greatest confidence).
        /// </summary>
        [JsonProperty("score")]
        public float Score { get; set; }

        /// <summary>
        /// Metadata that is associated with the answer
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public Metadata[] Metadata { get; set; }

        /// <summary>
        /// The source from which the QnA was extracted
        /// </summary>
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }

        /// <summary>
        /// The index of the answer in the knowledge base. V3 uses
        /// 'qnaId', V4 uses 'id'.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
    }

    /// <summary>
    /// Contains answers for a user query.
    /// </summary>
    public class QueryResults
    {
        /// <summary>
        /// The answers for a user query,
        /// sorted in decreasing order of ranking score.
        /// </summary>
        [JsonProperty("answers")]
        public QueryResult[] Answers { get; set; }
    }
}
