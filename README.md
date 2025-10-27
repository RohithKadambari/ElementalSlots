# Slot Machine Game

- Start with 1000 credits
-  Use SPIN button to play
- Adjust bet with BET +/- buttons
  - 3+ Scatter symbols trigger Free Spins


## Scripts
- GameController: Main game logic and win calculation
- ReelController: Reel animations and symbol generation
- UIController: UI management and button handling
- AnalyticsManager: Event tracking with JSON output

## Bonus System
Free Spins triggered by 3+ Scatter symbols:
- 3 free spins awarded,when showed anywhere on the screen, trigger spin coroutine 
- Wins accumulated during bonus
  

## Analytics
Events tracked in JSON format:
- Game start/end
- Spin results
- Bonus triggers
- Bet changes



