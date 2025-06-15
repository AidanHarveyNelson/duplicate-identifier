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
        Console.WriteLine("Running UpdateItemCreationPostScan...");
        // var last_scan_date = _storageManager.GetLastScanDate();
        var items = _itemRepo.GetItemList(new InternalItemsQuery
        {
            MediaTypes = new[] { MediaType.Video },
            OrderBy = new[] { (ItemSortBy.DateCreated, SortOrder.Ascending) },
        });
        _logger.LogInformation("Updating {Count} items", items.Count);
        List<BaseItem> updated_items = new();
        var numComplete = 0;
        var count = items.Count;

        foreach (var item in items)
        {
            if (item.DateCreated > item.DateModified)
            {
                item.DateCreated = item.DateModified;
                updated_items.Add(item);

                // Update the Progress meter
                numComplete++;
                double percent = numComplete;
                percent /= count;
                percent *= 80;
                progress.Report(percent);
            }

            // if (item.DateCreated > last_scan_date)
            // {
            //     last_scan_date = item.DateCreated;
            // }
        }

        if (updated_items.Count > 0)
        {
            _itemRepo.SaveItems(updated_items, cancellationToken);
        }
        else
        {
            _logger.LogInformation("No items were updated.");
        }

        // _storageManager.StoreLastScanDate(last_scan_date);
        progress.Report(100);
        return Task.CompletedTask;
    }
}
