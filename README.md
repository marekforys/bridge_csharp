# Bridge C#

A simple C# console application for simulating the card game Bridge.

## Features
- Models for cards, players, and deck
- Game manager for dealing cards to four players
- Console output with Unicode suit icons and compact rank symbols
- Hands displayed grouped by suit, each on a single line
- Unit tests for core logic using xUnit

## Getting Started

### Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version 6.0 or later)

### Setup
1. Clone the repository:
   ```sh
   git clone <repo-url>
   cd bridge_csharp
   ```
2. Restore dependencies and build:
   ```sh
   dotnet build
   ```

### Running the Application
To run the Bridge game simulation:
```sh
dotnet run --project BridgeGameApp
```

### Running Tests
To run the unit tests:
```sh
dotnet test BridgeGameApp.Tests
```

## Project Structure
- `BridgeGameApp/Models` - Card, Player, Deck models
- `BridgeGameApp/GameLogic` - GameManager for game flow
- `BridgeGameApp/Program.cs` - Entry point and console output
- `BridgeGameApp.Tests` - xUnit test project

## Example Output
```
North's hand:
♣: A Q J T 9 7 3
♦: 8
♥: A 4 3
♠: K 8
...
```

## License
MIT
