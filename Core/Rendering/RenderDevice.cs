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

		foreach ( Face triangleIndex in triangles )
		{
			// Vertices 
			( Vector3 first, Vector3 second, Vector3 third ) = renderingObject.Mesh.GetTriangleVertices( triangleIndex );

			Vector2 projectedFirst = camera.VertexToScreen( first, worldMatrix, _resolution );
			Vector2 projectedSecond = camera.VertexToScreen( second, worldMatrix, _resolution );
			Vector2 projectedThird = camera.VertexToScreen( third, worldMatrix, _resolution );

			RenderScreenLine( projectedFirst, projectedSecond, Color.WHITE );
			RenderScreenLine( projectedSecond, projectedThird, Color.WHITE );
			RenderScreenLine( projectedThird, projectedFirst, Color.WHITE );

			DrawCircleV( projectedFirst, 5, Color.RED );
			DrawCircleV( projectedSecond, 5, Color.RED );
			DrawCircleV( projectedThird, 5, Color.RED );
		}
	}

	public void RenderLine( Vector3 a, Vector3 b, Color color, Camera camera )
	{
		Vector2 projectedA = camera.WorldToScreen( a, _resolution );
		Vector2 projectedB = camera.WorldToScreen( b, _resolution );

		RenderScreenLine( projectedA, projectedB, color );
	}

	public void RenderPixel( Vector3 worldPosition, Color color, Camera camera )
	{
		Vector2 projectedPixel = camera.WorldToScreen( worldPosition, _resolution );

		DrawPixel( ( int )projectedPixel.X, ( int )projectedPixel.Y, color );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public void RenderLineFrom( Vector3 from, Vector3 relativeOffset, Color color, Camera camera ) =>
		RenderLine( from, from + relativeOffset, color, camera );

	readonly Resolution _resolution;

	// TODO: moved RenderScreenLine method into another class

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
