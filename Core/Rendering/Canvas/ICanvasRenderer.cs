using System.Numerics;
using Raylib_cs;

namespace Flatova.Rendering;

/// <summary>
///     Implements a renderer which renders primitive shapes on the canvas
/// </summary>
public interface ICanvasRenderer<in TColor>
{
	void Clear();

	void DrawDepthTriangle( Vector3 p1, Vector3 p2, Vector3 p3, TColor color );

	void DrawDepthPixel( int x, int y, float depth, TColor color );

	void DrawDepthLine( Vector3 from, Vector3 to, TColor color );
}
