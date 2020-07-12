﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Fast_Jira.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Microsoft.Rest;
    using Microsoft.Rest.Serialization;

    public partial class IssueBean
    {
        /// <summary>
        /// Initializes a new instance of the IssueBean class.
        /// </summary>
        public IssueBean() { }

        /// <summary>
        /// Initializes a new instance of the IssueBean class.
        /// </summary>
        public IssueBean(string expand = default(string), string id = default(string), string self = default(string), string key = default(string), IDictionary<string, object> properties = default(IDictionary<string, object>), IDictionary<string, string> names = default(IDictionary<string, string>), IDictionary<string, object> fields = default(IDictionary<string, object>))
        {
            Expand = expand;
            Id = id;
            Self = self;
            Key = key;
            Properties = properties;
            Names = names;
            Fields = fields;
        }

        /// <summary>
        /// Expand options that include additional issue details in the
        /// response.
        /// </summary>
        [JsonProperty(PropertyName = "expand")]
        public string Expand { get; private set; }

        /// <summary>
        /// The ID of the issue.
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; }

        /// <summary>
        /// The URL of the issue details.
        /// </summary>
        [JsonProperty(PropertyName = "self")]
        public string Self { get; private set; }

        /// <summary>
        /// The key of the issue.
        /// </summary>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; private set; }

        /// <summary>
        /// Details of the issue properties identified in the request.
        /// </summary>
        [JsonProperty(PropertyName = "properties")]
        public IDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// The ID and name of each field present on the issue.
        /// </summary>
        [JsonProperty(PropertyName = "names")]
        public IDictionary<string, string> Names { get; private set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "fields")]
        public IDictionary<string, object> Fields { get; set; }

    }
}