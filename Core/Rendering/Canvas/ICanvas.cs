namespace Flatova.Rendering;

public interface ICanvas<in TColor>
{
	public void DrawPixel( int x, int y, TColor color );
}
