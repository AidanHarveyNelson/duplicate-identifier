// Copyright (C) 2024 Intro-Skipper contributors <intro-skipper.org>
// SPDX-License-Identifier: GPL-3.0-only.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Duplicate.Identifier.Db;

/// <summary>
/// Plugin database.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DuplicateDbContext"/> class.
/// </remarks>
public class DuplicateDbContext : DbContext
{
    private readonly string _dbPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateDbContext"/> class.
    /// </summary>
    /// <param name="dbPath">The path to the SQLite database file.</param>
    public DuplicateDbContext(string dbPath)
    {
        _dbPath = dbPath;
        DbItemIngestion = Set<DbItemIngestion>();
        DbScanResults = Set<DbScanResults>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateDbContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    public DuplicateDbContext(DbContextOptions<DuplicateDbContext> options) : base(options)
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        _dbPath = System.IO.Path.Join(path, "duplicate.db");
        DbItemIngestion = Set<DbItemIngestion>();
        DbScanResults = Set<DbScanResults>();
    }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> containing the ingestion time.
    /// </summary>
    public DbSet<DbItemIngestion> DbItemIngestion { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="DbSet{TEntity}"/> containing the ingestion time.
    /// </summary>
    public DbSet<DbScanResults> DbScanResults { get; set; }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DbItemIngestion>(entity =>
        {
            entity.ToTable("ItemIngestion");
            entity.HasKey(s => new { s.ItemId });

            entity.HasIndex(e => e.ItemId);

            entity.Property(e => e.IngestionTime)
                  .HasDefaultValue(null)
                  .IsRequired();
        });

        modelBuilder.Entity<DbScanResults>(entity =>
        {
            entity.ToTable("ScanResults");
            entity.HasKey(s => new { s.Id });

            entity.HasIndex(e => e.Id);

            entity.Property(e => e.FinishTime)
                  .HasDefaultValue(null)
                  .IsRequired();

            entity.Property(e => e.LastModificationDate)
                  .HasDefaultValue(null)
                  .IsRequired();
        });
    }

    /// <summary>
    /// Applies any pending migrations to the database.
    /// </summary>
    public void ApplyMigrations()
    {
        // If database doesn't exist or can't connect, create it with migrations
        if (!Database.CanConnect())
        {
            Database.Migrate();
            return;
        }

        // If migrations table exists, apply pending migrations normally
        if (Database.GetAppliedMigrations().Any())
        {
            Database.Migrate();
            return;
        }

        // For databases without migration history
        RebuildDatabase();
    }

    /// <summary>
    /// Rebuilds the database while preserving valid ingestions and season information.
    /// </summary>
    public void RebuildDatabase()
    {
        // Backup existing data
        List<DbItemIngestion> ingestions = [];
        List<DbScanResults> scanResults = [];

        try
        {
            using var db = new DuplicateDbContext(_dbPath);
            ingestions = [.. db.DbItemIngestion.AsEnumerable()]; // .Where(s => s.ItemId().Valid)];
            scanResults = [.. db.DbScanResults.AsEnumerable()];
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read database data", ex);
        }
        finally
        {
            // Delete old database
            Database.EnsureDeleted();

            // Create new database with proper migration history
            Database.Migrate();
        }

        // Restore the data
        if (ingestions.Count > 0 || scanResults.Count > 0)
        {
            using var db = new DuplicateDbContext(_dbPath);
            if (ingestions.Count > 0)
            {
                db.DbItemIngestion.AddRange(ingestions);
            }

            if (scanResults.Count > 0)
            {
                db.DbScanResults.AddRange(scanResults);
            }

            db.SaveChanges();
        }
    }
}
