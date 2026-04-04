# PurpleHatProject

A Blazor Server application (.NET 10) with dual database support: SQLite for relational data and DynamoDB for NoSQL data.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [AWS CLI v2](https://aws.amazon.com/cli/) (for connecting to real AWS)
- EF Core tools: `dotnet tool install --global dotnet-ef`

## Getting Started

```bash
cd PurpleHatProject
dotnet run
```

The app starts at `https://localhost:<port>` (check console output for the exact URL).

SQLite is ready out of the box — migrations run automatically on startup and create a local `purplehat.db` file. DynamoDB requires a bit more setup (see below).

## Pages

| Page | Route | Purpose |
|---|---|---|
| **Home** | `/` | Landing page |
| **Counter** | `/counter` | Quick check that Blazor Server interactivity is working |
| **SQLite DB Health** | `/db-health` | Tests SQLite connectivity with a read/write round-trip |
| **DynamoDB Health** | `/dynamodb-health` | Tests DynamoDB connectivity with a read/write round-trip |

The health check pages are for development use only and should be removed before production.

## Database Setup

### SQLite (local, always works)

No setup required. The app creates a `purplehat.db` file on first run and applies EF Core migrations automatically.

**Adding a new entity:**

1. Create the entity class in `Data/`
2. Add a `DbSet` property to `ApplicationDbContext`
3. Run `dotnet ef migrations add <MigrationName>` from the `PurpleHatProject` directory
4. Restart the app (migrations apply on startup)

**Note:** The SQLite database file is excluded from git. Data is local to your machine and will be lost if the file is deleted or the app is running in a container that gets recreated.

### DynamoDB Local (development)

DynamoDB Local runs in Docker and emulates the real AWS DynamoDB service. No AWS account needed.

**Start it:**

```bash
docker run -d --name dynamodb-local -p 8000:8000 amazon/dynamodb-local
```

**Verify it's running:**

```bash
docker ps --filter name=dynamodb-local
```

The app is pre-configured to connect to `http://localhost:8000` when running in the Development environment (see `appsettings.Development.json`). Tables are created automatically by the health check page on first visit.

**Stop/restart:**

```bash
docker stop dynamodb-local
docker start dynamodb-local
```

**Note:** DynamoDB Local data is lost when the container is removed (`docker rm`). Stopping and starting the container preserves data.

### DynamoDB (real AWS)

To connect to the real AWS DynamoDB service instead of the local Docker instance:

**1. Configure AWS credentials:**

```bash
aws configure
```

You will be prompted for:
- **Access Key ID** and **Secret Access Key** — from your IAM user in the AWS console
- **Default region** — e.g. `eu-west-2`
- **Output format** — `json`

**2. Create the table (if it doesn't already exist):**

```bash
aws dynamodb create-table \
  --table-name HealthCheck \
  --attribute-definitions AttributeName=Id,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --billing-mode PAY_PER_REQUEST \
  --region eu-west-2
```

**3. Point the app at real AWS:**

Remove the `DynamoDb:ServiceUrl` section from `appsettings.Development.json`:

```json
{
  "DynamoDb": {
    "ServiceUrl": "http://localhost:8000"   <- Remove this line
  }
}
```

When `ServiceUrl` is absent, the AWS SDK automatically uses your credentials from `~/.aws/` and connects to the real service.

**4. Verify:**

Run the app and visit `/dynamodb-health`. It should show a green "Connected" badge.

## Project Structure

```
PurpleHatProject/
  Components/
    Layout/          -- NavMenu, MainLayout
    Pages/           -- Razor pages (Home, Counter, DbHealth, DynamoDbHealth)
  Data/
    ApplicationDbContext.cs   -- EF Core context for SQLite
    HealthCheckEntry.cs       -- SQLite health check entity
  Migrations/                 -- EF Core migrations
  Program.cs                  -- Service registration and middleware
  appsettings.json            -- Shared config (SQLite connection string)
  appsettings.Development.json -- Dev overrides (DynamoDB Local URL)
```

## AI Tooling Configuration

This project includes a `CLAUDE.md` file in the root directory, which configures Claude Code 
with persistent project context — covering the tech stack, infrastructure, testing approach 
and code quality expectations.

> **Note:** The project brief has been omitted from the committed version to avoid sharing 
> assessment details that may apply to other candidates.