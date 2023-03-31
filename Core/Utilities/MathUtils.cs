using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

public static class MathUtils
{
	/// <summary>
	///     Checks the point's x position, relative to the given line points,
	///     Returns negative values if <paramref name="point" /> is to the left of the line,
	///     Returns positive values if <paramref name="point" /> is to the right of the line,
	///     Returns zero if <paramref name="point" /> is on the line
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float PointSide2D( Vector2 point, Vector2 lineFrom, Vector2 lineTo ) =>
		Cross2D
		(
			point.X - lineFrom.X,
			point.Y - lineFrom.Y,
			lineTo.X - lineFrom.X,
			lineTo.Y - lineFrom.Y
		);
	
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float PointSide2D( Vector3 point, Vector3 lineFrom, Vector3 lineTo ) =>
		Cross2D
		(
			point.X - lineFrom.X,
			point.Y - lineFrom.Y,
			lineTo.X - lineFrom.X,
			lineTo.Y - lineFrom.Y
		);

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float Cross2D( float x0, float y0, float x1, float y1 ) =>
		x0 * y1 - x1 * y0;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float Lerp( float a, float b, float t ) =>
		a + ( b - a ) * float.Clamp( t, 0f, 1f );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool AlmostEquals( float a, float b, float epsilon = float.Epsilon ) =>
		MathF.Abs( a - b ) < epsilon;


	/// <summary>
	///     Flattens the two dimensional index into a one dimensional index
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static int FlattenIndex2D( int x, int y, int width ) =>
		x + y * width;
}
