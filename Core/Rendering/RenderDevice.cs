using System.Numerics;
using Flatova.Geometry;
using Flatova.World;
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

	public void RenderScene( Scene scene )
	{
		foreach ( WorldObject worldObject in scene.WorldObjects )
			RenderObject( worldObject, scene.Camera );
	}

	public void RenderObject( WorldObject renderingObject, Camera camera )
	{
		Matrix4x4 worldMatrix = renderingObject.GetWorldMatrix();

		TriangleIndex[] triangleIndices = renderingObject.Mesh.TriangleIndices;

		var clippedTriangles = new Queue<Triangle3D>();

		foreach ( TriangleIndex triangleIndex in triangleIndices )
		{
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

			if ( IsAnyProjectedTriangleCulling( projectedFirst, projectedSecond, projectedThird ) )
			{
				Raylib.DrawText( "Clipping", 0, 100, 17, Color.WHITE );

				continue;
			}

			var projectedTriangle = new Triangle3D( projectedFirst, projectedSecond, projectedThird );

			clippedTriangles.Clear();
			clippedTriangles.Enqueue( projectedTriangle );

			foreach ( Plane3D planeToClip in _projectionPlanesToClip )
				planeToClip.ClipTrianglesIntoQueue( clippedTriangles );

			foreach ( Triangle3D clippedTriangle in clippedTriangles )
			{
				projectedFirst = _resolution.MapProjectedDepth( clippedTriangle.First );
				projectedSecond = _resolution.MapProjectedDepth( clippedTriangle.Second );
				projectedThird = _resolution.MapProjectedDepth( clippedTriangle.Third );

				// Simple Phong Shading
				// TODO: After implementing materials, move this entire thing into materials
				Color faceColor = GetFaceColor( worldTriangle );
				
				_canvasRenderer.DrawDepthTriangle( projectedFirst, projectedSecond, projectedThird, faceColor );
			}
		}
	}


	public void RenderWorldRect( Vector3 worldCenterPosition, Vector2 size, Color color, Camera camera )
	{
		Vector3 projectedCenterPosition = camera.WorldProjectDepth( worldCenterPosition );

		if ( IsProjectedPointCulled( projectedCenterPosition ) )
			return;
		// else

		Vector3 screenRectCenterPosition = _resolution.MapProjectedDepth( projectedCenterPosition );

		Vector2 halfSize = size * .5f;

		var screenRectCenterPosition2D = new Vector2( screenRectCenterPosition.X, screenRectCenterPosition.Y );

		Vector2 topLeft = screenRectCenterPosition2D - halfSize;
		Vector2 bottomRight = screenRectCenterPosition2D + halfSize;

		for ( int x = ( int )topLeft.X; x < ( int )bottomRight.X; x++ )
		{
			for ( int y = ( int )topLeft.Y; y < ( int )bottomRight.Y; y++ )
				if ( _resolution.Contains( x, y ) )
					_canvasRenderer.DrawDepthPixel( x, y, projectedCenterPosition.Z, color );
		}
	}

	public void RenderWorldLineSegment( Vector3 worldFromPosition, Vector3 worldToPosition, Color color, Camera camera )
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

	public void RenderWorldTriangleLine( Vector3 a, Vector3 b, Vector3 c, Color color, Camera camera )
	{
		Vector3 projectedFirst = camera.WorldProjectDepth( a );
		Vector3 projectedSecond = camera.WorldProjectDepth( b );
		Vector3 projectedThird = camera.WorldProjectDepth( c );

		if ( IsAnyProjectedTriangleCulling( projectedFirst, projectedSecond, projectedThird ) )
			return;

		projectedFirst = _resolution.MapProjectedDepth( projectedFirst );
		projectedSecond = _resolution.MapProjectedDepth( projectedSecond );
		projectedThird = _resolution.MapProjectedDepth( projectedThird );

		_canvasRenderer.DrawDepthLine( projectedFirst, projectedSecond, color );
		_canvasRenderer.DrawDepthLine( projectedSecond, projectedThird, color );
		_canvasRenderer.DrawDepthLine( projectedThird, projectedFirst, color );
	}

	static readonly Plane3D[] _projectionPlanesToClip =
	{
		// near and far plane (front and back plane)
		new( Vector3.Zero, Vector3.UnitZ ),
		new( Vector3.UnitZ, -Vector3.UnitZ ),

		// up and down plane
		new( -Vector3.UnitY *.5f, Vector3.UnitY ),
		new( Vector3.UnitY * .5f, -Vector3.UnitY ),

		// left and right plane
		new( -Vector3.UnitX *.5f, Vector3.UnitX ),
		new( Vector3.UnitX *.5f, -Vector3.UnitX )
	};

	readonly Resolution _resolution;

	readonly ICanvasRenderer<Color> _canvasRenderer;

	static Color GetFaceColor( Triangle3D worldTriangle )
	{
		Vector3 worldFaceNormal = worldTriangle.GetNormal();
		Vector3 worldFaceCenter = worldTriangle.GetCenter();

		var lightPosition = new Vector3( 4, 10, 2 );

		Vector3 lightDirection = ( lightPosition - worldFaceCenter ).Normalize();

		const float AMBIENT_LIGHTING_STRENGTH = 0.2f;
		float phongShadingStrength = float.Min( float.Max( 0f, lightDirection.Dot( worldFaceNormal ) ) + AMBIENT_LIGHTING_STRENGTH, 1f );
		int shadingColor = ( int )( phongShadingStrength * 255f );

		var faceColor = new Color( shadingColor, shadingColor, shadingColor, 255 );

		return faceColor;
	}


	static bool IsProjectedPointCulled( Vector3 projectedPoint ) =>
		projectedPoint.X is < -1f or > 1f ||
		projectedPoint.Y is < -1f or > 1f ||
		projectedPoint.Z is < 0f or > 1f;


	static bool IsAnyProjectedTriangleCulling( Vector3 projectedFirst, Vector3 projectedSecond, Vector3 projectedThird )
	{
		// if ( projectedFirst.Z < 0f || projectedSecond.Z < 0f || projectedThird.Z < 0f )
		// 	return true;

		if ( projectedFirst.Z >= 1f || projectedSecond.Z >= 1f || projectedThird.Z >= 1f )
			return true;


		if (
			( projectedFirst.X < -1f && projectedSecond.X < -1f && projectedThird.X < -1f ) ||
			( projectedFirst.X > 1f && projectedSecond.X > 1f && projectedThird.X > 1f ) ||
			
			( projectedFirst.Y < -1f && projectedSecond.Y < -1f && projectedThird.Y < -1f ) ||
			( projectedFirst.Y > 1f && projectedSecond.Y > 1f && projectedThird.Y > 1f )
		)
		
			return true;
		
		// if (
		// 	( projectedFirst.X < -.5f && projectedSecond.X < -.5f && projectedThird.X < -.5f ) ||
		// 	( projectedFirst.X > .5f && projectedSecond.X > .5f && projectedThird.X > .5f ) ||
		// 	
		// 	( projectedFirst.Y < -.5f && projectedSecond.Y < -.5f && projectedThird.Y < -.5f ) ||
		// 	( projectedFirst.Y > .5f && projectedSecond.Y > .5f && projectedThird.Y > .5f )
		// )
		// 	return true;

		return false;
	}
}
