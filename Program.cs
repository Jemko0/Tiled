using Tiled;

using var game = new Tiled.Tiled();
game.Run();
Main.Init();


namespace Tiled
{
    public static class Main
    {
        static readonly Tiled game = new Tiled();
        public static Tiled GetGame() => game;

        public static void Init()
        {
            game.Run();
        }
    }
}

