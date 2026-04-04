# Project Context

## Developer Profile
Senior .NET software engineer (~7 years experience). Practical, action-oriented.
Prefers clear, direct communication — plain language over academic terminology.

## Tech Stack
- .NET 10 / C#
- Blazor Web App (Auto rendering mode — be mindful of server vs WebAssembly context)
- Entity Framework Core with Pomelo MySQL provider
- AWS DynamoDB (production) + DynamoDB Local via Docker (local dev)
- xUnit for testing
- FakeItEasy for mocking
- Shouldly for assertions

## Infrastructure
- Hosted on AWS EC2
- Elastic IP: (please ask)
- MySQL database (scaffolded, EF Core migrations)
- DynamoDB (scaffolded, dual config for local vs prod)
- Docker used for local DynamoDB emulation
- CI/CD via GitHub Actions (configured and working)

## Local vs Production Config
- DynamoDB local: points to Docker container endpoint
- DynamoDB prod: points to AWS DynamoDB service
- Environment switching should be handled via appsettings.json / environment variables

## Testing Approach
- TDD-first — write tests before implementation
- xUnit + FakeItEasy + Shouldly throughout
- Unit tests scaffolded and ready

## IDE
JetBrains Rider

## Code Preferences
- SOLID principles
- Async/await throughout (especially EF Core and DynamoDB calls)
- Minimal, readable code over clever code
- No over-engineering — practical solutions first

## Current Status
Infrastructure complete. Awaiting project brief to begin feature work.
Update this file with the brief and any specific requirements once received.

## Project Brief
<!-- Paste the brief here once received -->