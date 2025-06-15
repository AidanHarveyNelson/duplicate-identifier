using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duplicate.Identifier.Configuration;
using Duplicate.Identifier.Db;
using Duplicate.Identifier.Migrations;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Chapters;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;

namespace Duplicate.Identifier;

/// <summary>
/// The main plugin.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration> // , IHasWebPages
{
    private readonly ILibraryManager _libraryManager;
    private readonly ILogger<Plugin> _logger;
    private readonly string _dbPath;
    private readonly string _duplicateDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{T}"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    public Plugin(
        IApplicationPaths applicationPaths,
        ILogger<Plugin> logger,
        IXmlSerializer xmlSerializer,
        ILibraryManager libraryManager)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;

        _libraryManager = libraryManager;
        _logger = logger;

        var pluginDirName = "duplicate";

        _duplicateDirectory = Path.Join(applicationPaths.DataPath, pluginDirName);

        _dbPath = Path.Join(applicationPaths.DataPath, pluginDirName, "duplicate.db");

        // Create the base & cache directories (if needed).
        if (!Directory.Exists(_duplicateDirectory))
        {
            Directory.CreateDirectory(_duplicateDirectory);
        }

        // Initialize database, restore timestamps if available.
        try
        {
            using var db = new DuplicateDbContext(_dbPath);
            db.ApplyMigrations();
        }
        catch (Exception ex)
        {
            logger.LogWarning("Error initializing database: {Exception}", ex);
        }
    }

    /// <summary>
    /// Gets the path to the database.
    /// </summary>
    public string DbPath => _dbPath;

    /// <inheritdoc />
    public override string Name => "Duplicate Identifier";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("D1120F4F-53A5-48E7-82F0-2A4C348D3A1B");

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <summary>
    /// Gets Base path for storing duplicate information.
    /// </summary>
    public string BasePath => _duplicateDirectory;

    // Currently commented out as the plugin does not have custom settings.
    // public IEnumerable<PluginPageInfo> GetPages()
    // {
    //     return
    //     [
    //         new PluginPageInfo
    //         {
    //             Name = Name,
    //             EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
    //         }
    //     ];
    // }

    /// <summary>
    /// Gets the last scan time from the database.
    /// </summary>
    /// <returns>Returns the Timestamp of the last scan results.</returns>
    internal DateTime GetLastScanTime()
    {
        using var db = new DuplicateDbContext(_dbPath);

        var lastScan = db.DbScanResults.OrderByDescending(s => s.FinishTime)
            .FirstOrDefault();
        Console.WriteLine($"Last scan time: {lastScan?.FinishTime}");

        if (lastScan == null)
        {
            return DateTime.MinValue;
        }

        return lastScan.LastModificationDate;
    }

    /// <summary>
    /// Stores the last scan results in the database.
    /// </summary>
    /// <param name="lastModificationDate">The last modification date of the scan.</param>
    internal void WriteScanResult(DateTime lastModificationDate)
    {
        using var db = new DuplicateDbContext(_dbPath);
        var blah = new DbScanResults(DateTime.Now, lastModificationDate);
        db.DbScanResults.Add(blah);
        db.SaveChanges();
    }

    /// <summary>
    /// Gets the ingestion times for a list of item IDs from the database.
    /// </summary>
    /// <param name="itemdIds">List of item IDs to retrieve ingestion times for.</param>
    /// <returns>A dictionary mapping item IDs to their ingestion times.</returns>
    internal Dictionary<string, DateTime> GetItemsIngestionTimes(List<string> itemdIds)
    {
        using var db = new DuplicateDbContext(_dbPath);
        var itemIngestionTimes = db.DbItemIngestion
            .Where(i => itemdIds.Contains(i.ItemId))
            .ToDictionary(i => i.ItemId, i => i.IngestionTime);

        return itemIngestionTimes;
    }

    /// <summary>
    /// Gets the ingestion times for a list of item IDs from the database.
    /// </summary>
    /// <param name="itemId">List of item IDs to retrieve ingestion times for.</param>
    /// <returns>A dictionary mapping item IDs to their ingestion times.</returns>
    internal DateTime? GetItemIngestionTime(string itemId)
    {
        using var db = new DuplicateDbContext(_dbPath);
        var itemIngestionTime = db.DbItemIngestion
            .Where(i => i.ItemId == itemId)
            .ToDictionary(i => i.ItemId, i => i.IngestionTime);

        if (itemIngestionTime.Count == 0)
        {
            return null;
        }

        return itemIngestionTime.First().Value;
    }

    /// <summary>
    /// Gets the ingestion times for a list of item IDs from the database.
    /// </summary>
    /// <param name="itemIds">List of item IDs to retrieve ingestion times for.</param>
    /// <param name="ingestionTime">The time when the items were ingested.</param>
    internal void SaveItemsIngestionTimes(List<string> itemIds, DateTime ingestionTime)
    {
        using var db = new DuplicateDbContext(_dbPath);
        foreach (var itemId in itemIds)
        {
            db.DbItemIngestion.Add(new DbItemIngestion(itemId, ingestionTime));
        }

        db.SaveChanges();
    }

    /// <summary>
    /// Gets the ingestion times for a list of item IDs from the database.
    /// </summary>
    /// <param name="itemId">List of item IDs to retrieve ingestion times for.</param>
    /// <param name="ingestionTime">The time when the items were ingested.</param>
    internal void SaveItemIngestionTime(string itemId, DateTime ingestionTime)
    {
        using var db = new DuplicateDbContext(_dbPath);
        db.DbItemIngestion.Add(new DbItemIngestion(itemId, ingestionTime));

        db.SaveChanges();
    }
}
