# PurpleHatProject

A music player web application built with .NET 10 and Blazor Server. Users can browse and play audio tracks, manage favourites, and have their playback session remembered across visits.

## Features

### Music Player
- Track listing with title, artist, and album artwork (fallback icon when no artwork is available)
- Audio playback with play/pause, skip next/previous, and volume control with mute toggle
- Visual EQ indicator on the currently playing track
- Click any track to start playing immediately

### Favourite Tracks
- Heart icon on each track to toggle favourites
- Filter view to show only favourite tracks — skip/auto-advance respects the active filter
- Anonymous users get session-only favourites (lost on reload)
- Logged-in users get persistent favourites stored in SQLite

### User Accounts
- Simple login system with seeded users (no passwords — for demo purposes)
- User avatar with colour derived from name
- Login/logout from the navbar

### Session Persistence
- **In-session:** Playback state (track, position, volume, play/pause) survives page navigation within the same browser session
- **Cross-session:** Logged-in users have their playback state saved to DynamoDB — log out, log back in, and resume where you left off

### Other
- Dark/light theme toggle with localStorage persistence
- Responsive navbar with Bootstrap 5.3
- CI pipeline via GitHub Actions (build + test on every push/PR)

## Tech Stack

| Category | Technology | Purpose |
|---|---|---|
| Framework | .NET 10 / Blazor Server | Server-side interactive UI via SignalR |
| UI | Bootstrap 5.3 | Responsive layout, theming |
| Relational DB | SQLite via EF Core | Users, favourite tracks |
| NoSQL DB | Amazon DynamoDB | Playback session persistence |
| Audio | HTML5 Audio API via JS interop | Browser-side playback |
| Hosting | AWS EC2 | Production deployment |
| CI/CD | GitHub Actions | Automated build and test |
| Testing | xUnit + FakeItEasy + Shouldly | Unit tests for services |
| AI Tooling | Claude Code (Opus 4.6) | AI-assisted development |

## Architectural Decisions

### Why two databases?

**SQLite** handles relational data (users and their favourite tracks) where foreign key relationships and queries like "get all favourites for user X" are natural. It's embedded, requires no setup, and is more than sufficient for a single-instance deployment.

**DynamoDB** handles playback session state — a key-value lookup by user ID with fast writes on every pause. This data is ephemeral, non-relational, and benefits from DynamoDB's simple get/put model. It also demonstrates working with both relational and NoSQL databases in the same application.

### Audio serving

Audio files are served from `wwwroot/audio/` as static files rather than from S3 or another external store. This keeps the deployment self-contained with zero external dependencies for playback. Track metadata (artist, title) is parsed from filenames using the `"Artist - Title.mp3"` convention, so adding tracks is as simple as dropping files into a folder.

### Playback state — two layers

A scoped in-memory service (`PlaybackStateService`) handles navigation within the same Blazor circuit — fast, no I/O. DynamoDB (`PlaybackSessionService`) is the durable layer for cross-session restore. The component checks in-memory first, DynamoDB second.

### Favourite tracks — anonymous vs logged-in

The `TrackFavouriteService` holds favourites in a `HashSet` in memory. For anonymous users that's all there is — session-only. For logged-in users, every toggle also writes/deletes from SQLite via `IDbContextFactory`. This avoids making anonymous users hit the database while giving logged-in users full persistence.

### Track identification

Favourite tracks are stored by `AudioUrl` (the filename path) rather than `Track.Id`. The ID is assigned at runtime based on alphabetical file ordering — adding or renaming a file would shift all IDs and break existing favourites. The filename is stable.

## Test Coverage

23 unit tests covering the two services with the most meaningful logic:

**TrackService** (8 tests) — filename parsing, edge cases (no separator, multiple separators, whitespace), alphabetical ordering, non-mp3 filtering, missing directory handling.

**TrackFavouriteService** (12 tests) — toggle add/remove, anonymous vs logged-in persistence, DB round-trips using EF Core in-memory provider, `LoadForUserAsync` scoping, `Clear`, and `OnChange` event firing.

**Counter** (3 tests) — Blazor component tests via bUnit verifying interactive rendering.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for DynamoDB Local)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### Run locally

```bash
cd PurpleHatProject
dotnet run
```

SQLite is ready out of the box — migrations run on startup. For DynamoDB session persistence, start DynamoDB Local:

```bash
docker run -d --name dynamodb-local -p 8000:8000 amazon/dynamodb-local
```

The app connects to `http://localhost:8000` in the Development environment. Tables are created automatically on first use.

### Run tests

```bash
dotnet test
```

## Project Structure

```
PurpleHatProject/
  Components/
    Layout/            -- NavMenu (login, theme toggle), MainLayout
    Pages/             -- MusicPlayer, Home, health checks
  Models/              -- Track, User, FavouriteTrack
  Services/            -- TrackService, TrackFavouriteService,
                          PlaybackSessionService, PlaybackStateService,
                          UserSessionService
  Data/                -- EF Core DbContext, migrations
  wwwroot/
    audio/             -- MP3 track files (Artist - Title.mp3)
    js/                -- audioPlayer.js, theme.js
    app.css            -- Global styles with dark/light CSS variables

PurpleHatProject.Tests/
  Services/            -- TrackServiceTests, TrackFavouriteServiceTests
```

## AI Tooling

This project was built with Claude Code (Opus 4.6). A `CLAUDE.md` file in the root configures the AI assistant with project context — tech stack, code quality expectations, testing approach, and architectural constraints. This ensures consistent, explainable output aligned with the project's standards.
