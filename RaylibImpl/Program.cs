using Flatova;
using Raylib_cs;

namespace HelloWorld;

internal static class Program
{
	public static void Main()
	{
		Raylib.InitWindow( 1000, 600, "Hello World" );

		Raylib.SetTargetFPS( 60 );

		IApplication application = new SoftEngine();

		application.Initialize();

		while ( !Raylib.WindowShouldClose() )
		{
			application.Update();

			Raylib.BeginDrawing();

			application.Draw();

			Raylib.EndDrawing();
		}

		Raylib.CloseWindow();
	}
}
