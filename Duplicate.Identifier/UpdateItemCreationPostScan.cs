using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;

namespace Duplicate.Identifier;

/// <summary>
/// Class UpdateItemCreation.
/// </summary>
public class UpdateItemCreationPostScan : ILibraryPostScanTask
{
    /// <summary>
    /// The _library manager.
    /// </summary>
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<UpdateItemCreationPostScan> _logger;
    private readonly IItemRepository _itemRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateItemCreationPostScan" /> class.
    /// </summary>
    /// <param name="libraryManager">The library manager.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="itemRepo">The item repository.</param>
    public UpdateItemCreationPostScan(
        ILibraryManager libraryManager,
        ILogger<UpdateItemCreationPostScan> logger,
        IItemRepository itemRepo)
    {
        _libraryManager = libraryManager;
        _logger = logger;
        _itemRepo = itemRepo;
    }

    /// <summary>
    /// Runs the specified progress.
    /// </summary>
    /// <param name="progress">The progress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Task.</returns>
    public Task Run(IProgress<double> progress, CancellationToken cancellationToken)
    {
        // Set the episode IDs for the analyzed items
        var previousScan = Plugin.Instance!.GetLastScanTime();
        var newScanDate = Plugin.Instance!.GetLastScanTime();
        Console.WriteLine("Running UpdateItemCreationPostScan...");
        Console.WriteLine("Previous scan time: " + previousScan);

        var new_processed = false;
        var offset = 0;
        var limit = 100;
        List<BaseItem> updated_items = new List<BaseItem>();
        while (!new_processed)
        {
            var query = new InternalItemsQuery
            {
                MediaTypes = new[] { MediaType.Video },
                OrderBy = new[] { (ItemSortBy.DateCreated, SortOrder.Descending) },
                Limit = limit,
            };

            query.StartIndex = offset;

            var items = _itemRepo.GetItemList(query);
            _logger.LogInformation("Updating {Count} items", items.Count);

            if (items.Count == 0)
            {
                _logger.LogInformation("No items found to update.");
                break;
            }

            foreach (var item in items)
            {
                if (newScanDate == previousScan)
                {
                    newScanDate = item.DateCreated;
                }

                if (item.DateCreated <= previousScan)
                {
                    _logger.LogInformation("Currently at the previous scan step");
                    new_processed = true;
                    break;
                }

                var uniqueItemId = string.Join("|", item.ProviderIds.Select(kv => kv.Key + "=" + kv.Value).ToArray());
                var previousModification = Plugin.Instance!.GetItemIngestionTime(uniqueItemId);
                if (previousModification == null)
                {
                    Plugin.Instance!.SaveItemIngestionTime(uniqueItemId, item.DateCreated);
                    continue;
                }
                else
                {
                    item.DateCreated = previousModification.Value;
                    updated_items.Add(item);
                }
            }

            offset += limit;
        }

        if (updated_items.Count > 0)
        {
            _itemRepo.SaveItems(updated_items, cancellationToken);
        }

        progress.Report(100);
        Plugin.Instance!.WriteScanResult(newScanDate);
        return Task.CompletedTask;
    }
}
