# ReturnToPunchcards (Clocking System)

- **API**: .NET 8 Minimal API (EF Core + SQLite) in `Clocking.Api/`
- **Reader Gateway**: Node/TypeScript (simulator + optional NFC) in `ReaderGateway/`
- **Web Frontend**: React/Vite (planned) in `web-frontend/`

## Dev quick start
1. `cd Clocking.Api && dotnet ef database update && dotnet run`
2. `cd ../ReaderGateway && npm run dev:sim` (type a UID like `04AABBCCDD22`)
