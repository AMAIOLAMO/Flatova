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

		foreach ( TriangleIndex triangleIndex in triangleIndices )
		{
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
			if ( IsAnyProjectedTriangleCulling( projectedFirst, projectedSecond, projectedThird ) )
			{
				Raylib.DrawText( "Clipping", 0, 100, 17, Color.WHITE );

				continue;
			}

			projectedFirst = _resolution.MapProjectedDepth( projectedFirst );
			projectedSecond = _resolution.MapProjectedDepth( projectedSecond );
			projectedThird = _resolution.MapProjectedDepth( projectedThird );

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
		}
	}


	public void RenderWorldCircle( Vector3 worldCenterPosition, float radius, Color color, Camera camera )
	{
		// TODO: Render Filled circles
	}

	public void RenderWorldLine3D( Vector3 worldFromPosition, Vector3 worldToPosition, Color color, Camera camera )
	{
		Vector3 projectedA = camera.WorldProjectDepth( worldFromPosition );
		Vector3 projectedB = camera.WorldProjectDepth( worldToPosition );

		if ( IsProjectedPointCulled( projectedA ) && IsProjectedPointCulled( projectedB ) )
			return;

		Vector3 screenA = _resolution.MapProjectedDepth( projectedA );
		Vector3 screenB = _resolution.MapProjectedDepth( projectedB );

		_canvasRenderer.DrawDepthLine( screenA, screenB, color );
	}

	public void RenderWorldPixel( Vector3 worldPosition, Color color, Camera camera )
	{
		Vector3 projectedPosition = camera.WorldProjectDepth( worldPosition );

		if ( IsProjectedPointCulled( projectedPosition ) )
			return;

		Vector3 screenPoint = _resolution.MapProjectedDepth( projectedPosition );
		_canvasRenderer.DrawDepthPixel( ( int )screenPoint.X, ( int )screenPoint.Y, screenPoint.Z, color );
	}

	readonly Resolution _resolution;

	readonly ICanvasRenderer<Color> _canvasRenderer;

	static bool IsProjectedPointCulled( Vector3 projectedPoint ) =>
		projectedPoint.X is < -1f or > 1f ||
		projectedPoint.Y is < -1f or > 1f ||
		projectedPoint.Z is < 0f or > 1f;


	static bool IsAnyProjectedTriangleCulling( Vector3 projectedFirst, Vector3 projectedSecond, Vector3 projectedThird )
	{
		if ( projectedFirst.Z < 0f || projectedSecond.Z < 0f || projectedThird.Z < 0f )
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
