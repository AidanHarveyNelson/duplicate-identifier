# Duplicate Identifier Plugin for Jellyfin

The Duplicate Identifier plugin tracks ingestion times of media items in your Jellyfin library, allowing you to identify duplicate content and preserve original creation dates when content is rescanned or moved.

## Installation

### Stable Version

1. In Jellyfin, go to Dashboard -> Plugins -> Catalog
2. Install "Duplicate Identifier"
3. Restart Jellyfin

### Manual Installation

1. Download the desired zip file from the [releases page](https://github.com/yourusername/duplicate-identifier/releases)
2. Extract the contents to your Jellyfin's plugin directory
   * For Windows: `%APPDATA%\Jellyfin\plugins\Duplicate.Identifier`
   * For Linux: `/var/lib/jellyfin/plugins/Duplicate.Identifier`
   * For macOS: `~/.local/share/jellyfin/plugins/Duplicate.Identifier`
   * For Docker: `/config/plugins/Duplicate.Identifier`
3. Restart Jellyfin

### Development Builds

Development builds can be obtained directly from the GitHub Actions artifacts. These builds may be unstable but contain the latest features and bug fixes.

## Features

- Tracks when media items are first added to your Jellyfin library
- Preserves original DateCreated values when media is rescanned or moved
- Prevents duplicate detection issues when reorganizing your library
- Works with all video media types in Jellyfin

## How It Works

The plugin stores a unique identifier for each media item along with its original ingestion date in a SQLite database. When new items are added during a library scan, their ingestion dates are saved. When existing items are rescanned or moved, the plugin restores their original ingestion dates.

This ensures that rescanned or reorganized media doesn't appear as new content in your "Recently Added" sections.

## Building From Source

1. Clone this repository
2. Open the solution in your IDE of choice (Visual Studio, VS Code, etc.)
3. Build the solution
4. Copy the resulting DLL files to your Jellyfin plugins directory

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the GNU General Public License v3.0 - see the LICENSE file for details.

## Donations

If you like this plugin and want to support its development, you can [buy me a coffee](https://www.buymeacoffee.com/yourusername).
