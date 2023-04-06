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

			if ( !TryClipTriangle
			    (
				    new Plane3D( Vector3.UnitZ * .1f, Vector3.UnitZ ),
				    projectedTriangle, out Triangle3D[]? clippedProjectedTriangles
			    ) )
				continue;
			// else clipping is successful

			for ( int index = 0; index < clippedProjectedTriangles.Length; index++ )
			{
				Triangle3D clippedProjectedTriangle = clippedProjectedTriangles[ index ];

				projectedFirst = _resolution.MapProjectedDepth( clippedProjectedTriangle.First );
				projectedSecond = _resolution.MapProjectedDepth( clippedProjectedTriangle.Second );
				projectedThird = _resolution.MapProjectedDepth( clippedProjectedTriangle.Third );

				// Simple Phong Shading
				// TODO: After implementing materials, move this entire thing into materials
				Vector3 worldFaceCenter = worldTriangle.GetCenter();

				var lightPosition = new Vector3( 4, 10, 2 );

				Vector3 lightDirection = ( lightPosition - worldFaceCenter ).Normalize();

				const float AMBIENT_LIGHTING_STRENGTH = 0.2f;
				float phongShadingStrength = float.Min( float.Max( 0f, lightDirection.Dot( worldFaceNormal ) ) + AMBIENT_LIGHTING_STRENGTH, 1f );
				int shadingColor = ( int )( phongShadingStrength * 255f );

				Color faceColor = Color.WHITE;

				if ( index == 0 )
					faceColor = new Color( shadingColor, 0, 0, 255 );
				else if ( index == 2 )
					faceColor = new Color( 0, shadingColor, 0, 255 );
				else if ( index == 3 )
					faceColor = new Color( 0, 0, shadingColor, 255 );

				_canvasRenderer.DrawDepthTriangle( projectedFirst, projectedSecond, projectedThird, faceColor );
			}


			// TODO: Temporary Clipping
			// if ( IsAnyProjectedTriangleCulling( projectedFirst, projectedSecond, projectedThird ) )
			// {
			// 	Raylib.DrawText( "Clipping", 0, 100, 17, Color.WHITE );
			//
			// 	continue;
			// }

			// projectedFirst = _resolution.MapProjectedDepth( projectedFirst );
			// projectedSecond = _resolution.MapProjectedDepth( projectedSecond );
			// projectedThird = _resolution.MapProjectedDepth( projectedThird );
			//
			// Simple Phong Shading
			// TODO: After implementing materials, move this entire thing into materials
			// Vector3 worldFaceCenter = worldTriangle.GetCenter();
			//
			// var lightPosition = new Vector3( 4, 10, 2 );
			//
			// Vector3 lightDirection = ( lightPosition - worldFaceCenter ).Normalize();
			//
			// const float AMBIENT_LIGHTING_STRENGTH = 0.2f;
			// float phongShadingStrength = float.Min( float.Max( 0f, lightDirection.Dot( worldFaceNormal ) ) + AMBIENT_LIGHTING_STRENGTH, 1f );
			// byte shadingByteColor = ( byte )( phongShadingStrength * 255f );
			//
			// var faceColor = new Color( shadingByteColor, shadingByteColor, shadingByteColor, ( byte )255 );
			//
			// _canvasRenderer.DrawDepthTriangle( projectedFirst, projectedSecond, projectedThird, faceColor );
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

	public void RenderWorldLineSegment3D( Vector3 worldFromPosition, Vector3 worldToPosition, Color color, Camera camera )
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

	public void RenderWorldTriangleLine( Triangle3D triangle, Color color, Camera camera )
	{
		Vector3 projectedFirst = camera.WorldProjectDepth( triangle.First );
		Vector3 projectedSecond = camera.WorldProjectDepth( triangle.Second );
		Vector3 projectedThird = camera.WorldProjectDepth( triangle.Third );

		if ( IsAnyProjectedTriangleCulling( projectedFirst, projectedSecond, projectedThird ) )
			return;

		projectedFirst = _resolution.MapProjectedDepth( projectedFirst );
		projectedSecond = _resolution.MapProjectedDepth( projectedSecond );
		projectedThird = _resolution.MapProjectedDepth( projectedThird );

		_canvasRenderer.DrawDepthLine( projectedFirst, projectedSecond, color );
		_canvasRenderer.DrawDepthLine( projectedSecond, projectedThird, color );
		_canvasRenderer.DrawDepthLine( projectedThird, projectedFirst, color );
	}

	readonly Resolution _resolution;

	readonly ICanvasRenderer<Color> _canvasRenderer;

	static bool TryClipTriangle( Plane3D plane, Triangle3D triangleToClip, out Triangle3D[]? clippedTriangles )
	{
		var insidePoints = new Vector3[ 3 ];
		var outsidePoints = new Vector3[ 3 ];

		int insidePointCount = 0, outsidePointCount = 0;

		float dFirst = plane.GetSignShortDistance( triangleToClip.First );
		float dSecond = plane.GetSignShortDistance( triangleToClip.Second );
		float dThird = plane.GetSignShortDistance( triangleToClip.Third );

		// TODO: Simplify this by making Triangle 3D give an enumerable vertex / point list
		if ( dFirst >= 0 )
		{
			insidePoints[ insidePointCount ] = triangleToClip.First;
			++insidePointCount;
		}
		else
		{
			outsidePoints[ outsidePointCount ] = triangleToClip.First;
			++outsidePointCount;
		}

		if ( dSecond >= 0 )
		{
			insidePoints[ insidePointCount ] = triangleToClip.Second;
			++insidePointCount;
		}
		else
		{
			outsidePoints[ outsidePointCount ] = triangleToClip.Second;
			++outsidePointCount;
		}

		if ( dThird >= 0 )
		{
			insidePoints[ insidePointCount ] = triangleToClip.Third;
			++insidePointCount;
		}
		else
		{
			outsidePoints[ outsidePointCount ] = triangleToClip.Third;
			++outsidePointCount;
		}

		// no points are inside the plane to clip, failed to clip the triangle
		if ( insidePointCount == 0 )
		{
			clippedTriangles = null;

			return false;
		}

		// the entire triangle is inside the clipping area, so no need to clip
		if ( insidePointCount == 3 )
		{
			clippedTriangles = new[] { triangleToClip };

			return true;
		}

		// clip into a smaller triangle
		if ( insidePointCount == 1 && outsidePointCount == 2 )
		{
			var newTriangle = new Triangle3D
			(
				insidePoints[ 0 ],
				plane.IntersectLine( insidePoints[ 0 ], outsidePoints[ 0 ] ),
				plane.IntersectLine( insidePoints[ 0 ], outsidePoints[ 1 ] )
			);

			clippedTriangles = new[] { newTriangle };

			return true;
		}


		if ( insidePointCount == 2 && outsidePointCount == 1 )
		{
			Vector3 newFirstPoint = plane.IntersectLine( insidePoints[ 0 ], outsidePoints[ 0 ] );

			var newFirstTriangle = new Triangle3D
			(
				insidePoints[ 0 ],
				insidePoints[ 1 ],
				newFirstPoint
			);

			Vector3 newSecondPoint = plane.IntersectLine( insidePoints[ 1 ], outsidePoints[ 0 ] );

			var newSecondTriangle = new Triangle3D
			(
				insidePoints[ 1 ],
				newFirstPoint,
				newSecondPoint
			);

			clippedTriangles = new[] { newFirstTriangle, newSecondTriangle };

			return true;
		}

		throw new Exception( "Impossible triangle clipping situation!" );
	}

	static bool IsProjectedPointCulled( Vector3 projectedPoint ) =>
		projectedPoint.X is < -1f or > 1f ||
		projectedPoint.Y is < -1f or > 1f ||
		projectedPoint.Z is < 0f or > 1f;


	static bool IsAnyProjectedTriangleCulling( Vector3 projectedFirst, Vector3 projectedSecond, Vector3 projectedThird )
	{
		// if ( projectedFirst.Z < 0f || projectedSecond.Z < 0f || projectedThird.Z < 0f )
		// 	return true;

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
