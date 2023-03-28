using Raylib_cs;

namespace Flatova.Rendering;

public interface ICanvasRenderer
{
	public void DrawPixel( int x, int y, Color color );
}
