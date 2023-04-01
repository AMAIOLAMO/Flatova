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

			Vector3 projectedFirst = camera.VertexProjectDepthResolution( first, worldMatrix, _resolution );
			Vector3 projectedSecond = camera.VertexProjectDepthResolution( second, worldMatrix, _resolution );
			Vector3 projectedThird = camera.VertexProjectDepthResolution( third, worldMatrix, _resolution );

			Vector3 faceNormal = FaceUtils.GetNormal( projectedFirst, projectedSecond, projectedThird );

			float facingTowardsCamera = faceNormal.Dot( camera.Transform.BasisUnitZ );

			// faces which are not facing the camera, will be ignored
			if ( facingTowardsCamera <= 0 )
				continue;


			Color faceColor = index % 2 == 0 ?
				Color.WHITE :
				new Color( 170, 170, 170, 255 );

			RenderScreenTriangle3D( projectedFirst, projectedSecond, projectedThird, faceColor );
		}
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
		if ( MathUtils.PointSide2D( p2, p1, p3 ) > 0 )
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
	void RenderScreenScanLine( int y, Vector3 lineAStart, Vector3 lineAEnd, Vector3 lineBStart, Vector3 lineBEnd, Color color )
	{
		float lineAxStepPerY = !MathUtils.AlmostEquals( lineAStart.Y, lineAEnd.Y ) ? ( y - lineAStart.Y ) / ( lineAEnd.Y - lineAStart.Y ) : 1;
		float lineBxStepPerY = !MathUtils.AlmostEquals( lineBStart.Y, lineBEnd.Y ) ? ( y - lineBStart.Y ) / ( lineBEnd.Y - lineBStart.Y ) : 1;

		int startX = ( int )MathUtils.Lerp( lineAStart.X, lineAEnd.X, lineAxStepPerY );
		int endX = ( int )MathUtils.Lerp( lineBStart.X, lineBEnd.X, lineBxStepPerY );

		// starting Z & ending Z
		float startDepth = MathUtils.Lerp( lineAStart.Z, lineAEnd.Z, lineAxStepPerY );
		float endDepth = MathUtils.Lerp( lineBStart.Z, lineBEnd.Z, lineBxStepPerY );

		for ( int x = startX; x < endX; x++ )
		{
			if ( !_resolution.ContainsPixel( x, y ) )
				continue;

			float percentage = ( x - startX ) / ( float )( endX - startX );

			float currentDepth = MathF.Abs( MathUtils.Lerp( startDepth, endDepth, percentage ) );

			if ( !_depthMap.IsCloser( x, y, currentDepth ) )
				continue;
			// else

			_canvasRenderer.DrawPixel( x, y, color );

			_depthMap.SetDepth( x, y, currentDepth );
		}
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
}
