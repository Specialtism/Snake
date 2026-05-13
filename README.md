# WPF Snake 🐍

A classic Snake game built with C# and WPF targeting .NET 10. Choose from multiple game modes with increasing difficulty and compete for the highest scores with local leaderboards.

## Table of Contents
- [Features](#features)
- [Getting Started](#getting-started)
- [How to Play](#how-to-play)
- [Game Modes](#game-modes)
- [Technologies](#technologies)

## Features

- **Multiple Game Modes:** Choose your challenge level
- **Local Leaderboards:** Track your top 3 scores per game mode
- **Power-ups:** Dynamic items that speed up or slow down your snake
- **Score Tracking:** Persistent storage of your best performances
- **Smooth Controls:** Responsive arrow key controls

## Getting Started

### Prerequisites
- Visual Studio 2026 or later
- .NET 10 SDK

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/Snake.git
   cd Snake
   ```

2. **Open in Visual Studio**
   - Open `Snake.sln` in Visual Studio

3. **Build and Run**
   - Select `Debug` or `Release` configuration
   - Press `F5` or click **Start**

## How to Play

- **Move Snake:** Use Arrow Keys (↑ ↓ ← →)
- **Objective:** Eat food to grow and score points without hitting walls or yourself
- **Power-ups:** 
  - 🟢 Green with Blue Cracks: Speed boost

## Game Modes

| Mode | Description | Grid Size |
|------|-------------|-----------|
| **Classic** | Traditional snake gameplay | 20x20 |
| **Large Grid** | Massive arena where food pick-ups randomly speed you up, slow you down, or behave regularly and has chance for speed boost spawn | 50x50 |
| **Classic Hard** | Intense classic mode with sppawning random speed boosts | 20x20 |

## Technologies

- **Language:** C# 13
- **Framework:** WPF (.NET 10)
- **Platform:** Windows Desktop

## Project Structure

```
Snake/
├── Assets/           # Icons and graphics
├── Views/            # XAML UI components
├── ViewModels/       # Business logic and state management
├── Models/           # Core game logic
└── README.md
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Author

