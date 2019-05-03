﻿//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Azure.Cosmos.ChangeFeed.LeaseManagement
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos.ChangeFeed.Exceptions;
    using Microsoft.Azure.Cosmos.ChangeFeed.Logging;
    using System.Collections.Concurrent;

    /// <summary>
    /// <see cref="DocumentServiceLeaseUpdater"/> that uses In-Memory
    /// </summary>
    internal sealed class DocumentServiceLeaseUpdaterInMemory : DocumentServiceLeaseUpdater
    {
        private const int RetryCountOnConflict = 5;
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();
        private readonly ConcurrentDictionary<string, DocumentServiceLease> container;

        public DocumentServiceLeaseUpdaterInMemory(ConcurrentDictionary<string, DocumentServiceLease> container)
        {
            this.container = container;
        }

        public override Task<DocumentServiceLease> UpdateLeaseAsync(DocumentServiceLease cachedLease, string itemId, object partitionKey, Func<DocumentServiceLease, DocumentServiceLease> updateLease)
        {
            DocumentServiceLease lease = cachedLease;
            for (int retryCount = RetryCountOnConflict; retryCount >= 0; retryCount--)
            {
                lease = updateLease(lease);
                if (lease == null)
                {
                    return null;
                }

                if (!this.container.TryGetValue(itemId, out DocumentServiceLease currentLease))
                {
                    throw new LeaseLostException(lease);
                }

                if (this.container.TryUpdate(itemId, lease, currentLease))
                {
                    return Task.FromResult(lease);
                }

                Logger.InfoFormat("Lease with token {0} update conflict. ", lease.CurrentLeaseToken);
            }

            throw new LeaseLostException(lease);
        }
    }
}
