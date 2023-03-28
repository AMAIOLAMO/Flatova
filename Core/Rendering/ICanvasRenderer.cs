namespace Flatova.Rendering;

public interface ICanvasRenderer<in TColor>
{
	public void DrawPixel( int x, int y, TColor color );
}
