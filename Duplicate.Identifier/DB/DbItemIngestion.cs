// Copyright (C) 2024 Intro-Skipper contributors <intro-skipper.org>
// SPDX-License-Identifier: GPL-3.0-only.

using System;

namespace Duplicate.Identifier.Db;

/// <summary>
/// Represents an ingestion record in the database.
/// </summary>
public class DbItemIngestion
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbItemIngestion"/> class.
    /// </summary>
    /// <param name="itemId">The Item GUID.</param>
    /// <param name="ingestionTime">The type of analysis that was used to determine this segment.</param>
    public DbItemIngestion(Guid itemId, DateTime ingestionTime)
    {
        ItemId = itemId;
        IngestionTime = ingestionTime;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbItemIngestion"/> class.
    /// </summary>
    public DbItemIngestion()
    {
    }

    /// <summary>
    /// Gets or sets the episode id.
    /// </summary>
    public Guid ItemId { get; set; }

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public DateTime IngestionTime { get; set; }
}
