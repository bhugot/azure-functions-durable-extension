﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask
{
    /// <summary>
    /// A EventSource Listener, provides callbacks to DurableTask EventSource events
    /// that we use to capture their data and log it in Linux App Service plans.
    /// </summary>
    internal class EventSourceListener : EventListener
    {
        private readonly LinuxAppServiceLogger logger;

        /// <summary>
        /// Create an EventSourceListener for logging Durable-Extension EventSource
        /// data in Linux.
        /// </summary>
        /// <param name="logger">An LinuxAppService logger configured for the host linux instance.</param>
        public EventSourceListener(LinuxAppServiceLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Gets called for every EventSource in the process, this method allows us to determine
        /// if the listener will subscribe to an EventSource provider. We only listen to DurableTask
        /// and DurableTask-Extension EventSource providers.
        /// </summary>
        /// <param name="eventSource">An instance of EventSource.</param>
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // "7DA4779A-152E-44A2-A6F2-F80D991A5BEE" is the old DurableTask-Core event source,
            // so we provide extra logic to ignore it.
            if ((eventSource.Name == "DurableTask-Core"
                  && eventSource.Guid != new Guid("7DA4779A-152E-44A2-A6F2-F80D991A5BEE")) ||
                eventSource.Name == "DurableTask-AzureStorage" ||
                eventSource.Name == "WebJobs-Extensions-DurableTask" ||
                eventSource.Name == "DurableTask-SqlServer")
            {
                this.EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
            }
        }

        /// <summary>
        /// Gets called after every EventSource event, that event's data and log it
        /// using the appropiate strategy for our host linux plan.
        /// </summary>
        /// <param name="eventData">The EventSource event data, for logging.</param>
        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            this.logger.Log(eventData);
        }
    }
}
