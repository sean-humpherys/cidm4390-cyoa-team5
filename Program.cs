// Entry point for the Choose Your Own Adventure game.
// Keep this file tiny: all game logic lives in Game.cs (and the other classes)
// so the MSTest project can test it. Code written here can't be unit tested.

Game game = new Game();
game.StartGame();
