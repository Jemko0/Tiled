Tiled.Program.Init();


namespace Tiled
{
    public static class Program
    {
        static readonly Main game = new Main();
        public static Main GetGame() => game;

        public static void Init()
        {
            game.Run();
        }
    }
}

