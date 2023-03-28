using System.Numerics;
using System.Runtime.CompilerServices;
using Flatova.Geometry;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Flatova.Rendering;

public class RenderDevice
{
	public RenderDevice( Resolution resolution ) =>
		_resolution = resolution;

	public void RenderObject( WorldObject renderingObject, Camera camera )
	{
		Matrix4x4 worldMatrix = renderingObject.GetWorldMatrix();

		ReadOnlySpan<Face> triangles = renderingObject.Mesh.Triangles;

		for ( int index = 0; index < triangles.Length; index++ )
		{
			Face face = triangles[ index ];

			// Vertices 
			( Vector3 first, Vector3 second, Vector3 third ) = renderingObject.Mesh.GetFaceVertices( face );

			Vector2 projectedFirst = camera.VertexToScreen( first, worldMatrix, _resolution );
			Vector2 projectedSecond = camera.VertexToScreen( second, worldMatrix, _resolution );
			Vector2 projectedThird = camera.VertexToScreen( third, worldMatrix, _resolution );

			Color drawColor = index % 2 == 0 ?
				Color.WHITE : new Color( 150, 150, 150, 150 );

			RenderScreenTriangle3D( projectedFirst, projectedSecond, projectedThird, drawColor );
		}
	}

	public void RenderLine3D( Vector3 a, Vector3 b, Color color, Camera camera )
	{
		Vector2 projectedA = camera.WorldToScreen( a, _resolution );
		Vector2 projectedB = camera.WorldToScreen( b, _resolution );

		RenderScreenLine( projectedA, projectedB, color );
	}

	public void RenderPixel3D( Vector3 worldPosition, Color color, Camera camera )
	{
		Vector2 projectedPixel = camera.WorldToScreen( worldPosition, _resolution );

		DrawPixel( ( int )projectedPixel.X, ( int )projectedPixel.Y, color );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void RenderLineFrom2D( Vector3 from, Vector3 relativeOffset, Color color, Camera camera ) =>
		RenderLine3D( from, from + relativeOffset, color, camera );

	readonly Resolution _resolution;

	static void RenderScreenTriangle3D( Vector2 p1, Vector2 p2, Vector2 p3, Color color )
	{
		// Sort points using Y axis with order from top to bottom:
		// p1 -> p2 -> p3
		SortScreenPointsByY( ref p1, ref p2, ref p3 );

		// Where P2 is to the right of the P1 P3 line
		if ( MathUtils.PointSide2D( p2, p1, p3 ) > 0 )
			RenderTriangleWhereP2RightSide( p1, p2, p3, color );

		// Where P2 is to the left of the P1 P3 line
		else
			RenderTriangleWhereP2LeftSide( p1, p2, p3, color );
	}

	static void RenderTriangleWhereP2LeftSide( Vector2 p1, Vector2 p2, Vector2 p3, Color color )
	{
		for ( int y = ( int )p1.Y; y <= ( int )p3.Y; y++ )
			if ( y < p2.Y )
				RenderScreenScanLine( y, p1, p2, p1, p3, color );
			else
				RenderScreenScanLine( y, p2, p3, p1, p3, color );
	}

	static void RenderTriangleWhereP2RightSide( Vector2 p1, Vector2 p2, Vector2 p3, Color color )
	{
		for ( int y = ( int )p1.Y; y <= ( int )p3.Y; y++ )
			if ( y < p2.Y )
				RenderScreenScanLine( y, p1, p3, p1, p2, color );
			else
				RenderScreenScanLine( y, p1, p3, p2, p3, color );
	}

	static void SortScreenPointsByY( ref Vector2 p1, ref Vector2 p2, ref Vector2 p3 )
	{
		if ( p1.Y > p2.Y )
			( p1, p2 ) = ( p2, p1 );

		if ( p2.Y > p3.Y )
			( p2, p3 ) = ( p3, p2 );

		if ( p1.Y > p2.Y )
			( p1, p2 ) = ( p2, p1 );
	}


	// TODO: moved RenderScreenLine method into another class
	static void RenderScreenScanLine( int y, Vector2 lineAStart, Vector2 lineAEnd, Vector2 lineBStart, Vector2 lineBEnd, Color color )
	{
		float startXStepPerY = Math.Abs( lineAStart.Y - lineAEnd.Y ) > float.Epsilon ? ( y - lineAStart.Y ) / ( lineAEnd.Y - lineAStart.Y ) : 1;
		float endXStepPerY = Math.Abs( lineBStart.Y - lineBEnd.Y ) > float.Epsilon ? ( y - lineBStart.Y ) / ( lineBEnd.Y - lineBStart.Y ) : 1;

		int startX = ( int )MathUtils.Lerp( lineAStart.X, lineAEnd.X, startXStepPerY );
		int endX = ( int )MathUtils.Lerp( lineBStart.X, lineBEnd.X, endXStepPerY );

		for ( int x = startX; x < endX; x++ )
			DrawPixel( x, y, color );
	}


	// using Bresenham's Line drawing Algorithm
	static void RenderScreenLine( Vector2 pointA, Vector2 pointB, Color color )
	{
		int x0 = ( int )pointA.X;
		int y0 = ( int )pointA.Y;

		int x1 = ( int )pointB.X;
		int y1 = ( int )pointB.Y;

		int dx = Math.Abs( x1 - x0 );
		int dy = Math.Abs( y1 - y0 );

		int sx = x0 < x1 ? 1 : -1;
		int sy = y0 < y1 ? 1 : -1;

		int errorTerm = dx - dy;

		while ( true )
		{
			DrawPixel( x0, y0, color );

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
}
