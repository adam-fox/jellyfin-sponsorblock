# jellyfin-sponsorblock

A Jellyfin plugin that skips sponsored segments in YouTube videos using [SponsorBlock](https://sponsor.ajay.app/) data.

If you download YouTube videos and watch them in Jellyfin, you lose the SponsorBlock browser extension. This plugin brings it back: it fetches community-submitted segment data from the SponsorBlock API and feeds it into Jellyfin's native Media Segments system, so your players auto-skip sponsors, self-promotion, intros, outros, and more.

Works great with [TubeArchivist](https://www.tubearchivist.com/) (which names files by YouTube ID), but has no dependency on it. Any YouTube library works as long as the video ID is in the filename.

## Requirements

- Jellyfin 10.11+
- YouTube videos with the 11-character video ID in the filename (e.g., `dQw4w9WgXcQ.mp4`)

## Installation

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**
2. Add this repository URL:
   ```
   https://raw.githubusercontent.com/felixfoertsch/jellyfin-sponsorblock/main/manifest.json
   ```
3. Go to **Catalogue**, find **SponsorBlock**, and install it
4. Restart Jellyfin

### Manual installation

1. Download `Jellyfin.Plugin.SponsorBlock.dll` from the [latest release](https://github.com/felixfoertsch/jellyfin-sponsorblock/releases)
2. Place it in `<jellyfin-data>/plugins/SponsorBlock_1.0.0.0/`
3. Restart Jellyfin

## Setup

1. Go to **Dashboard → Plugins → SponsorBlock** and configure which categories to skip
2. Enable the **SponsorBlock** provider in your YouTube library: **Library → Manage Library → Media Segment Providers**
3. Run **Dashboard → Scheduled Tasks → Media Segment Scan**
4. Configure skip behavior per user in the plugin settings, or have each user set it in **Settings → Playback → Media Segments**

## How it works

When Jellyfin runs a Media Segment Scan, the plugin:

1. Extracts the YouTube video ID from each file's name
2. Queries the [SponsorBlock API](https://sponsor.ajay.app/) for skip segments
3. Maps them to Jellyfin Media Segment types
4. Jellyfin stores the segments and players auto-skip or show a skip button during playback

## File matching

The plugin needs to find the YouTube video ID in the filename. Two modes are available:

| Mode | Example | Description |
|---|---|---|
| **YouTube ID as Filename** (default) | `dQw4w9WgXcQ.mp4` | The filename without extension is the ID. This is how TubeArchivist names files. |
| **Custom Regex** | `Cool Video [dQw4w9WgXcQ].mp4` | A regex with one capture group extracts the ID. Default pattern: `\[([a-zA-Z0-9_-]{11})\]` |

## Category mapping

SponsorBlock has more category types than Jellyfin supports. The mapping:

| SponsorBlock | Jellyfin | Default |
|---|---|---|
| Sponsor | Commercial | enabled |
| Self-Promotion | Commercial | enabled |
| Interaction ("like and subscribe") | Commercial | disabled |
| Intro | Intro | disabled |
| Outro | Outro | disabled |
| Preview | Preview | disabled |
| Filler | Commercial | disabled |
| Non-Music (in music videos) | Commercial | disabled |

## User segment actions

The plugin settings page includes an admin panel to manage how each user's player handles segments. You can batch-apply "Default", "Ask to Skip", or "Auto Skip" to all users at once.

## Building from source

```bash
dotnet build
dotnet test
```

The plugin DLL is at `Jellyfin.Plugin.SponsorBlock/bin/Debug/net9.0/Jellyfin.Plugin.SponsorBlock.dll`.

## License

MIT
