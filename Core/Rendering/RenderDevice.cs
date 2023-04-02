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

		_depthMap = new DepthMap( _resolution );
	}

	public void ClearDepthBuffer() =>
		_depthMap.Clear();

	public void RenderObject( WorldObject renderingObject, Camera camera )
	{
		Matrix4x4 worldMatrix = renderingObject.GetWorldMatrix();

		Face[] faces = renderingObject.Mesh.Faces;

		for ( int index = 0; index < faces.Length; index++ )
		{
			Face face = faces[ index ];
			// Vertices 

			renderingObject.Mesh.GetFaceVertices
			(
				face,
				out Vector3 first, out Vector3 second, out Vector3 third
			);

			Vector3 worldFirst = first.Transform( worldMatrix );
			Vector3 worldSecond = second.Transform( worldMatrix );
			Vector3 worldThird = third.Transform( worldMatrix );

			Vector3 worldFaceNormal = FaceUtils.GetNormal( worldFirst, worldSecond, worldThird );
			Vector3 cameraDirectionTowardsFace = worldFirst - camera.Transform.Position;

			float faceFacingTowardsCamera = worldFaceNormal.Dot( cameraDirectionTowardsFace );

			// backface culling
			if ( faceFacingTowardsCamera > 0 )
				continue;

			Vector3 projectedFirst = camera.WorldProjectDepth( worldFirst );
			Vector3 projectedSecond = camera.WorldProjectDepth( worldSecond );
			Vector3 projectedThird = camera.WorldProjectDepth( worldThird );

			// TODO: Temporary Clipping
			if ( IsAnyProjectedClippingOut( projectedFirst, projectedSecond, projectedThird ) )
				continue;

			projectedFirst = camera.MapProjectedDepthToResolution( projectedFirst, _resolution );
			projectedSecond = camera.MapProjectedDepthToResolution( projectedSecond, _resolution );
			projectedThird = camera.MapProjectedDepthToResolution( projectedThird, _resolution );

			// Simple Phong Shading
			// TODO: After implementing materials, move this entire thing into materials
			Vector3 worldFaceCenter = FaceUtils.GetCenter( worldFirst, worldSecond, worldThird );

			var lightPosition = new Vector3( 4, 10, 2 );

			Vector3 lightDirection = ( lightPosition - worldFaceCenter ).Normalize();

			const float AMBIENT_LIGHTING_STRENGTH = 0.2f;
			float phongShadingStrength = float.Min( float.Max( 0f, lightDirection.Dot( worldFaceNormal ) ) + AMBIENT_LIGHTING_STRENGTH, 1f );
			byte shadingByteColor = ( byte )( phongShadingStrength * 255f );

			var faceColor = new Color( shadingByteColor, shadingByteColor, shadingByteColor, ( byte )255 );

			RenderScreenTriangle3D( projectedFirst, projectedSecond, projectedThird, faceColor );

			// Debug: Renders face normals (though, should be rendering normals regardless of able to see it)
			// RenderLine3D( worldFaceCenter, worldFaceCenter + worldFaceNormal, Color.GREEN, camera );
		}
	}


	public void RenderLine3D( Vector3 a, Vector3 b, Color color, Camera camera )
	{
		Vector3 projectedA = camera.WorldProjectDepthResolution( a, _resolution );
		Vector3 projectedB = camera.WorldProjectDepthResolution( b, _resolution );

		// TODO: Temporary clipping
		if ( projectedA.Z < -1 || projectedB.Z < -1 )
			return;

		RenderScreenLine( projectedA.To2D(), projectedB.To2D(), color );
	}

	readonly Resolution _resolution;

	readonly ICanvasRenderer<Color> _canvasRenderer;

	readonly DepthMap _depthMap;

	void RenderScreenTriangle3D( Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		// Sort points using Y axis with order from top to bottom:
		// p1 -> p2 -> p3
		SortScreenPointsByY( ref p1, ref p2, ref p3 );

		// Where P2 is to the right of the P1 P3 line
		if ( MathUtils.LineSide2D( p2, p1, p3 ) > 0 )
			RenderTriangleWhereP2RightSide( p1, p2, p3, color );

		// Where P2 is to the left of the P1 P3 line
		else
			RenderTriangleWhereP2LeftSide( p1, p2, p3, color );
	}

	void RenderTriangleWhereP2LeftSide( Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		for ( int y = ( int )p1.Y; y <= ( int )p3.Y; y++ )
			if ( y < p2.Y )
				RenderScreenScanLine( y, p1, p2, p1, p3, color );
			else
				RenderScreenScanLine( y, p2, p3, p1, p3, color );
	}

	void RenderTriangleWhereP2RightSide( Vector3 p1, Vector3 p2, Vector3 p3, Color color )
	{
		for ( int y = ( int )p1.Y; y <= ( int )p3.Y; y++ )
			if ( y < p2.Y )
				RenderScreenScanLine( y, p1, p3, p1, p2, color );
			else
				RenderScreenScanLine( y, p1, p3, p2, p3, color );
	}

	void SortScreenPointsByY( ref Vector3 p1, ref Vector3 p2, ref Vector3 p3 )
	{
		if ( p1.Y > p2.Y )
			( p1, p2 ) = ( p2, p1 );

		if ( p2.Y > p3.Y )
			( p2, p3 ) = ( p3, p2 );

		if ( p1.Y > p2.Y )
			( p1, p2 ) = ( p2, p1 );
	}

	// TODO: moved RenderScreenLine method into another class
	// TODO: Implement `Line3D` to hide some of the implementation details
	void RenderScreenScanLine( int y, Vector3 lineAStart, Vector3 lineAEnd, Vector3 lineBStart, Vector3 lineBEnd, Color color )
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

	void DrawDepthPixel( int x, int y, float depth, Color color )
	{
		if ( !_depthMap.IsCloser( x, y, depth ) )
			return;
		// else

		_canvasRenderer.DrawPixel( x, y, color );

		_depthMap.SetDepth( x, y, depth );
	}


	// using Bresenham's Line drawing Algorithm
	void RenderScreenLine( Vector2 pointA, Vector2 pointB, Color color )
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
			_canvasRenderer.DrawPixel( x0, y0, color );

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

	static bool IsAnyProjectedClippingOut( Vector3 projectedFirst, Vector3 projectedSecond, Vector3 projectedThird )
	{
		if ( projectedFirst.Z < -.5f || projectedSecond.Z < -.5f || projectedThird.Z < -.5f )
			return true;

		if ( projectedFirst.Z > 1f || projectedSecond.Z > 1f || projectedThird.Z > 1f )
			return true;


		if ( projectedFirst.X < -.5f && projectedSecond.X < -.5f && projectedThird.X < -.5f )
			return true;

		if ( projectedFirst.X > .5f && projectedSecond.X > .5f && projectedThird.X > .5f )
			return true;


		if ( projectedFirst.Y < -.5f && projectedSecond.Y < -.5f && projectedThird.Y < -.5f )
			return true;

		if ( projectedFirst.Y > .5f && projectedSecond.Y > .5f && projectedThird.Y > .5f )
			return true;

		return false;
	}
}
