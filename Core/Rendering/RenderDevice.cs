using System.Numerics;
using Flatova.Geometry;
using Raylib_cs;

namespace Flatova.Rendering;

public class RenderDevice : IRenderDevice<Color>
{
	public RenderDevice( Resolution resolution, ICanvasRenderer<Color> canvasRenderer )
	{
		_resolution = resolution;
		_canvasRenderer = canvasRenderer;
	}

	public void Clear() =>
		_canvasRenderer.Clear();

	public void RenderObject( WorldObject renderingObject, Camera camera )
	{
		Matrix4x4 worldMatrix = renderingObject.GetWorldMatrix();

		TriangleIndex[] triangleIndices = renderingObject.Mesh.TriangleIndices;

		for ( int index = 0; index < triangleIndices.Length; index++ )
		{
			TriangleIndex triangleIndex = triangleIndices[ index ];
			// Vertices 

			Triangle3D localTriangle = renderingObject.Mesh.GetTriangleFromIndex( triangleIndex );

			Triangle3D worldTriangle = localTriangle.Transform( worldMatrix );

			Vector3 worldFaceNormal = worldTriangle.GetNormal();
			Vector3 cameraDirectionTowardsFace = worldTriangle.First - camera.Transform.Position;

			float faceFacingTowardsCamera = worldFaceNormal.Dot( cameraDirectionTowardsFace );

			// backface culling
			if ( faceFacingTowardsCamera > 0 )
				continue;

			Vector3 projectedFirst = camera.WorldProjectDepth( worldTriangle.First );
			Vector3 projectedSecond = camera.WorldProjectDepth( worldTriangle.Second );
			Vector3 projectedThird = camera.WorldProjectDepth( worldTriangle.Third );


			// TODO: Temporary Clipping
			if ( IsAnyProjectedTriangleClipping( projectedFirst, projectedSecond, projectedThird ) )
			{
				Raylib.DrawText( "Clipping", 0, 100, 17, Color.WHITE );

				continue;
			}

			projectedFirst = camera.MapProjectedDepthToResolution( projectedFirst, _resolution );
			projectedSecond = camera.MapProjectedDepthToResolution( projectedSecond, _resolution );
			projectedThird = camera.MapProjectedDepthToResolution( projectedThird, _resolution );

			// Simple Phong Shading
			// TODO: After implementing materials, move this entire thing into materials
			Vector3 worldFaceCenter = worldTriangle.GetCenter();

			var lightPosition = new Vector3( 4, 10, 2 );

			Vector3 lightDirection = ( lightPosition - worldFaceCenter ).Normalize();

			const float AMBIENT_LIGHTING_STRENGTH = 0.2f;
			float phongShadingStrength = float.Min( float.Max( 0f, lightDirection.Dot( worldFaceNormal ) ) + AMBIENT_LIGHTING_STRENGTH, 1f );
			byte shadingByteColor = ( byte )( phongShadingStrength * 255f );

			var faceColor = new Color( shadingByteColor, shadingByteColor, shadingByteColor, ( byte )255 );

			_canvasRenderer.DrawDepthTriangle( projectedFirst, projectedSecond, projectedThird, faceColor );

			// Debug: Renders face normals (though, should be rendering normals regardless of able to see it)
			RenderWorldLine3D( worldFaceCenter, worldFaceCenter + worldFaceNormal, Color.GREEN, camera );
		}
	}


	public void RenderWorldLine3D( Vector3 a, Vector3 b, Color color, Camera camera )
	{
		Vector3 projectedA = camera.WorldProjectDepth( a );
		Vector3 projectedB = camera.WorldProjectDepth( b );

		if ( IsProjectedPointClipping( projectedA ) && IsProjectedPointClipping( projectedB ) )
		{
			Raylib.DrawText( "Line Clipping", 0, 120, 17, Color.WHITE );

			return;
		}

		Vector3 screenA = camera.MapProjectedDepthToResolution( projectedA, _resolution );
		Vector3 screenB = camera.MapProjectedDepthToResolution( projectedB, _resolution );

		_canvasRenderer.DrawDepthLine( screenA, screenB, color );
	}

	readonly Resolution _resolution;

	readonly ICanvasRenderer<Color> _canvasRenderer;


	static bool IsProjectedPointClipping( Vector3 projectedPoint ) =>
		projectedPoint.X is < -1f or > 1f ||
		projectedPoint.Y is < -1f or > 1f ||
		projectedPoint.Z is < -1f or > 1f;

	static bool IsAnyProjectedTriangleClipping( Vector3 projectedFirst, Vector3 projectedSecond, Vector3 projectedThird )
	{
		if ( projectedFirst.Z < -1f || projectedSecond.Z < -1f || projectedThird.Z < -1f )
			return true;

		if ( projectedFirst.Z > 1f || projectedSecond.Z > 1f || projectedThird.Z > 1f )
			return true;


		if ( projectedFirst.X < -1f && projectedSecond.X < -1f && projectedThird.X < -1f )
			return true;

		if ( projectedFirst.X > 1f && projectedSecond.X > 1f && projectedThird.X > 1f )
			return true;


		if ( projectedFirst.Y < -1f && projectedSecond.Y < -1f && projectedThird.Y < -1f )
			return true;

		if ( projectedFirst.Y > 1f && projectedSecond.Y > 1f && projectedThird.Y > 1f )
			return true;

		return false;
	}
}
