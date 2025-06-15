// Copyright (C) 2024 Intro-Skipper contributors <intro-skipper.org>
// SPDX-License-Identifier: GPL-3.0-only.

using System;

namespace Duplicate.Identifier.Db;

/// <summary>
/// Represents a scan results record in the database.
/// </summary>
public class DbScanResults
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanResults"/> class.
    /// </summary>
    /// <param name="finishTime">The time when the scan finished.</param>
    /// <param name="lastModificationDate">The last modification date.</param>
    public DbScanResults(DateTime finishTime, DateTime lastModificationDate)
    {
        FinishTime = finishTime;
        LastModificationDate = lastModificationDate;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbScanResults"/> class.
    /// </summary>
    public DbScanResults()
    {
    }

    /// <summary>
    /// Gets or sets the ID of the scan result.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the time when the scan finished.
    /// </summary>
    public DateTime FinishTime { get; set; }

    /// <summary>
    /// Gets or sets the last modification date.
    /// </summary>
    public DateTime LastModificationDate { get; set; }
}
