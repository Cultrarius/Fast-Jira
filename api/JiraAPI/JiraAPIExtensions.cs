﻿// Code generated by Microsoft (R) AutoRest Code Generator 0.16.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Fast_Jira
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Rest;
    using Models;

    /// <summary>
    /// Extension methods for JiraAPI.
    /// </summary>
    public static partial class JiraAPIExtensions
    {
            /// <summary>
            /// Get issue
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='issueIdOrKey'>
            /// The ID or key of the issue.
            /// </param>
            /// <param name='fields'>
            /// </param>
            /// <param name='fieldsByKeys'>
            /// Whether fields in `fields` are referenced by keys rather than IDs. This
            /// parameter is useful where fields have been added by a connect app and a
            /// field's key may differ from its ID.
            /// </param>
            /// <param name='expand'>
            /// Use [expand](#expansion) to include additional information about the
            /// issues in the response. This parameter accepts a comma-separated list.
            /// Expand options include:
            /// 
            /// *  `renderedFields` Returns field values rendered in HTML format.
            /// *  `names` Returns the display name of each field.
            /// *  `schema` Returns the schema describing a field type.
            /// *  `transitions` Returns all possible transitions for the issue.
            /// *  `editmeta` Returns information about how each field can be edited.
            /// *  `changelog` Returns a list of recent updates to an issue, sorted by
            /// date, starting from the most recent.
            /// *  `versionedRepresentations` Returns a JSON array for each version of a
            /// field's value, with the highest number representing the most recent
            /// version. Note: When included in the request, the `fields` parameter is
            /// ignored.
            /// </param>
            /// <param name='properties'>
            /// A list of issue properties to return for the issue. This parameter accepts
            /// a comma-separated list. Allowed values:
            /// 
            /// *  `*all` Returns all issue properties.
            /// *  Any issue property key, prefixed with a minus to exclude.
            /// 
            /// Examples:
            /// 
            /// *  `*all` Returns all properties.
            /// *  `*all,-prop1` Returns all properties except `prop1`.
            /// *  `prop1,prop2` Returns `prop1` and `prop2` properties.
            /// 
            /// This parameter may be specified multiple times. For example,
            /// `properties=prop1,prop2&amp; properties=prop3`.
            /// </param>
            /// <param name='updateHistory'>
            /// Whether the project in which the issue is created is added to the user's
            /// **Recently viewed** project list, as shown under **Projects** in Jira.
            /// This also populates the [JQL issues search](#api-rest-api-3-search-get)
            /// `lastViewed` field.
            /// </param>
            public static IssueBean GetIssue(this IJiraAPI operations, string issueIdOrKey, string fields = default(string), bool? fieldsByKeys = false, string expand = default(string), string properties = default(string), bool? updateHistory = false)
            {
                return Task.Factory.StartNew(s => ((IJiraAPI)s).GetIssueAsync(issueIdOrKey, fields, fieldsByKeys, expand, properties, updateHistory), operations, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Get issue
            /// </summary>
            /// <param name='operations'>
            /// The operations group for this extension method.
            /// </param>
            /// <param name='issueIdOrKey'>
            /// The ID or key of the issue.
            /// </param>
            /// <param name='fields'>
            /// </param>
            /// <param name='fieldsByKeys'>
            /// Whether fields in `fields` are referenced by keys rather than IDs. This
            /// parameter is useful where fields have been added by a connect app and a
            /// field's key may differ from its ID.
            /// </param>
            /// <param name='expand'>
            /// Use [expand](#expansion) to include additional information about the
            /// issues in the response. This parameter accepts a comma-separated list.
            /// Expand options include:
            /// 
            /// *  `renderedFields` Returns field values rendered in HTML format.
            /// *  `names` Returns the display name of each field.
            /// *  `schema` Returns the schema describing a field type.
            /// *  `transitions` Returns all possible transitions for the issue.
            /// *  `editmeta` Returns information about how each field can be edited.
            /// *  `changelog` Returns a list of recent updates to an issue, sorted by
            /// date, starting from the most recent.
            /// *  `versionedRepresentations` Returns a JSON array for each version of a
            /// field's value, with the highest number representing the most recent
            /// version. Note: When included in the request, the `fields` parameter is
            /// ignored.
            /// </param>
            /// <param name='properties'>
            /// A list of issue properties to return for the issue. This parameter accepts
            /// a comma-separated list. Allowed values:
            /// 
            /// *  `*all` Returns all issue properties.
            /// *  Any issue property key, prefixed with a minus to exclude.
            /// 
            /// Examples:
            /// 
            /// *  `*all` Returns all properties.
            /// *  `*all,-prop1` Returns all properties except `prop1`.
            /// *  `prop1,prop2` Returns `prop1` and `prop2` properties.
            /// 
            /// This parameter may be specified multiple times. For example,
            /// `properties=prop1,prop2&amp; properties=prop3`.
            /// </param>
            /// <param name='updateHistory'>
            /// Whether the project in which the issue is created is added to the user's
            /// **Recently viewed** project list, as shown under **Projects** in Jira.
            /// This also populates the [JQL issues search](#api-rest-api-3-search-get)
            /// `lastViewed` field.
            /// </param>
            /// <param name='cancellationToken'>
            /// The cancellation token.
            /// </param>
            public static async Task<IssueBean> GetIssueAsync(this IJiraAPI operations, string issueIdOrKey, string fields = default(string), bool? fieldsByKeys = false, string expand = default(string), string properties = default(string), bool? updateHistory = false, CancellationToken cancellationToken = default(CancellationToken))
            {
                using (var _result = await operations.GetIssueWithHttpMessagesAsync(issueIdOrKey, fields, fieldsByKeys, expand, properties, updateHistory, null, cancellationToken).ConfigureAwait(false))
                {
                    return _result.Body;
                }
            }

    }
}