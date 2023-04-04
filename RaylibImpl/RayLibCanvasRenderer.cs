using System.Numerics;
using Raylib_cs;

namespace Flatova.Rendering;

public class RayLibCanvasRenderer : ICanvasRenderer<Color>
{
	public RayLibCanvasRenderer( Resolution resolution, ICanvas<Color> canvas )
	{
		_canvas = canvas;
		_resolution = resolution;

		_depthMap = new DepthMap( _resolution );
	}

	public void Clear() =>
		_depthMap.Clear();

	public void DrawDepthTriangle( Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		// Sort points using Y axis with order from top to bottom:
		// p1 -> p2 -> p3
		SortScreenPointsByY( ref p1, ref p2, ref p3 );

		// Where P2 is to the right of the P1 P3 line
		if ( MathUtils.LineSide2D( p2, p1, p3 ) > 0 )
			DrawTriangleP2RightSide( p1, p2, p3, color );

		// Where P2 is to the left of the P1 P3 line
		else
			DrawTriangleP2LeftSide( p1, p2, p3, color );
	}

	public void DrawDepthPixel( int x, int y, float depth, Color color )
	{
		if ( !_depthMap.IsCloser( x, y, depth ) )
			return;
		// else

		_canvas.DrawPixel( x, y, color );

		_depthMap.SetDepth( x, y, depth );
	}


	// using Bresenham's Line drawing Algorithm
	public void DrawDepthLine( Vector3 from, Vector3 to, Color color )
	{
		int x0 = ( int )from.X;
		int y0 = ( int )from.Y;

		int x1 = ( int )to.X;
		int y1 = ( int )to.Y;

		int dx = Math.Abs( x1 - x0 );
		int dy = Math.Abs( y1 - y0 );

		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;

		int errorTerm = dx - dy;

		while ( true )
		{
			// float percentage = ( x0 - from.X ) / ( to.X - from.X );

			// float depth = MathUtils.Lerp( from.Z, to.Z, percentage );

			_canvas.DrawPixel( x0, y0, color );
			// DrawDepthPixel( x0, y0, depth, color );

			if ( x0 == x1 && y0 == y1 )
				break;
			// else

			int errorTermDoubled = errorTerm * 2;

			if ( errorTermDoubled > -dy )
			{
				errorTerm -= dy;
				x0 += sx;
			}

			if ( errorTermDoubled < dx )
			{
				errorTerm += dx;
				y0 += sy;
			}
		}
	}

	public void DrawDepthScanLine( int y, Vector3 lineAStart, Vector3 lineAEnd, Vector3 lineBStart, Vector3 lineBEnd, Color color )
	{
		float lineAxStepPerY = !MathUtils.AlmostEquals( lineAStart.Y, lineAEnd.Y ) ? ( y - lineAStart.Y ) / ( lineAEnd.Y - lineAStart.Y ) : 1;
		float lineBxStepPerY = !MathUtils.AlmostEquals( lineBStart.Y, lineBEnd.Y ) ? ( y - lineBStart.Y ) / ( lineBEnd.Y - lineBStart.Y ) : 1;

		int startX = ( int )MathUtils.LerpClamped( lineAStart.X, lineAEnd.X, lineAxStepPerY );
		int endX = ( int )MathUtils.LerpClamped( lineBStart.X, lineBEnd.X, lineBxStepPerY );

		// starting Z & ending Z
		float startDepth = MathUtils.LerpClamped( lineAStart.Z, lineAEnd.Z, lineAxStepPerY );
		float endDepth = MathUtils.LerpClamped( lineBStart.Z, lineBEnd.Z, lineBxStepPerY );

		for ( int x = startX; x < endX; x++ )
		{
			if ( !_resolution.Contains( x, y ) )
				continue;

			float percentage = ( x - startX ) / ( float )( endX - startX );

			float currentDepth = MathF.Abs( MathUtils.LerpClamped( startDepth, endDepth, percentage ) );

			DrawDepthPixel( x, y, currentDepth, color );
		}
	}

	readonly DepthMap   _depthMap;
	readonly Resolution _resolution;

	readonly ICanvas<Color> _canvas;

	void SortScreenPointsByY( ref Vector3 p1, ref Vector3 p2, ref Vector3 p3 )
	{
		if ( p1.Y > p2.Y )
			( p1, p2 ) = ( p2, p1 );

		if ( p2.Y > p3.Y )
			( p2, p3 ) = ( p3, p2 );

		if ( p1.Y > p2.Y )
			( p1, p2 ) = ( p2, p1 );
	}

	void DrawTriangleP2LeftSide( Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		for ( int y = ( int )p1.Y; y <= ( int )p3.Y; y++ )
			if ( y < p2.Y )
				DrawDepthScanLine( y, p1, p2, p1, p3, color );
			else
				DrawDepthScanLine( y, p2, p3, p1, p3, color );
	}

	void DrawTriangleP2RightSide( Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		for ( int y = ( int )p1.Y; y <= ( int )p3.Y; y++ )
			if ( y < p2.Y )
				DrawDepthScanLine( y, p1, p3, p1, p2, color );
			else
				DrawDepthScanLine( y, p1, p3, p2, p3, color );
	}
}
