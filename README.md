# PropertyPulse

A cross-platform real estate CRM app built with .NET MAUI and ASP.NET Core.

Agents use the app to manage properties, track leads, and schedule site visits. Managers can see all listings and follow team performance on a dashboard.

## Features

- Login with Agent and Manager roles (JWT authentication)
- Property listings with search, filters, photos, and location
- Lead management with stages from New to Closed, plus notes
- One-tap call, email, and WhatsApp sharing
- Site visit scheduling with reminders
- Manager dashboard with charts
- Offline support with automatic sync
- Light and dark themes, English and Sinhala

## Tech Stack

- **App:** .NET MAUI (.NET 10), C#, XAML, MVVM (CommunityToolkit.Mvvm), SQLite
- **API:** ASP.NET Core Web API, Entity Framework Core, PostgreSQL
- **Other:** Docker, xUnit, GitHub Actions

## Project Structure

```
src/
  PropertyPulse.App             .NET MAUI app
  PropertyPulse.Api             Web API
  PropertyPulse.Domain          Entities and enums
  PropertyPulse.Infrastructure  Database and repositories
  PropertyPulse.Shared          Shared DTOs
tests/
  PropertyPulse.Api.Tests
  PropertyPulse.App.Tests
```

## How to Run

### 1. Install the requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- .NET MAUI workload. Run this once:
  ```bash
  dotnet workload install maui
  ```
- [Docker Desktop](https://www.docker.com/products/docker-desktop/), which runs the API and database
- For Android only: [JDK 21](https://learn.microsoft.com/java/openjdk/download) and the Android SDK (API 36)

### 2. Get the code

```bash
git clone https://github.com/ChathuniNupearachchi/PropertyPulse.git
cd PropertyPulse
```

### 3. Start the API and database

Open Docker Desktop and wait until it shows **Engine running**. Then run:

```bash
docker compose up -d
```

This starts PostgreSQL and the API. On the first run, it creates the database and adds sample data.

To check that it works, open **http://localhost:5000/swagger** in your browser. You should see the API endpoints.

### 4. Run the app

**On Windows:**

```bash
dotnet build src/PropertyPulse.App -t:Run -f net10.0-windows10.0.19041.0
```

**On Android** (start an emulator, or connect a phone with USB debugging turned on):

```bash
dotnet build src/PropertyPulse.App -t:Run -f net10.0-android
```

The first build takes a few minutes.

> Always include `-f` when building the app on Windows. Without it, the build also tries iOS and macOS, which need a Mac.

**API address by device:**

| Device           | API address                                                             |
| ---------------- | ----------------------------------------------------------------------- |
| Windows          | `http://localhost:5000`                                                 |
| Android emulator | `http://10.0.2.2:5000`                                                  |
| Android phone    | `http://<your-computer-IP>:5000` (phone and computer on the same Wi-Fi) |

### 5. Sign in

Use one of the sample accounts:

| Role    | Email                      | Password       |
| ------- | -------------------------- | -------------- |
| Manager | `manager@propertypulse.lk` | `Password123!` |
| Agent   | `agent1@propertypulse.lk`  | `Password123!` |

Managers see the Dashboard tab. Agents don't.

### 6. Run the tests

```bash
dotnet test tests/PropertyPulse.Api.Tests
dotnet test tests/PropertyPulse.App.Tests
```
