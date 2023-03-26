using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Flatova;

public class RenderDevice
{
	public RenderDevice( Resolution resolution ) =>
		_resolution = resolution;

	public void RenderTransformed( TransformedMesh renderingObject, Camera camera )
	{
		Matrix4x4 worldMatrix = renderingObject.GetWorldMatrix();

		ReadOnlySpan<Vector3> vertices = renderingObject.Mesh.Vertices;

		for ( int index = 0; index < vertices.Length - 1; index++ )
		{
			Vector3 vertexA = vertices[ index ];
			Vector3 vertexB = vertices[ index + 1 ];

			Vector3 worldVertexA = Vector3.Transform( vertexA, worldMatrix );
			Vector3 worldVertexB = Vector3.Transform( vertexB, worldMatrix );

			Vector2 projectedA = camera.WorldToScreen( worldVertexA, _resolution );
			Vector2 projectedB = camera.WorldToScreen( worldVertexB, _resolution );

			DrawCircleV( projectedA, 5, Color.RED );
			DrawCircleV( projectedB, 5, Color.RED );

			RenderLine( projectedA, projectedB, Color.WHITE );
		}
	}

	public void RenderLine( Vector2 pointA, Vector2 pointB, Color color )
	{
		if ( Vector2.Distance( pointA, pointB ) < 2f )
		{
			DrawPixelV( pointA, color );
			DrawPixelV( pointA, color );

			return;
		}
		// else

		Vector2 midPoint = ( pointA + pointB ) * .5f;

		DrawPixel( ( int )MathF.Round( midPoint.X ), ( int )MathF.Round( midPoint.Y ), color );

		RenderLine( pointA, midPoint, color );
		RenderLine( midPoint, pointB, color );
	}

	readonly Resolution _resolution;
}
