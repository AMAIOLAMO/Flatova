using System.Numerics;
using Flatova.Geometry;
using Flatova.Rendering;
using Flatova.World;
using Raylib_cs;
using static Raylib_cs.Raylib;
using Mesh = Flatova.Geometry.Mesh;

namespace Flatova;

public class RenderApplication : IApplication
{
	public RenderApplication()
	{
		var resolution = new Resolution( GetScreenWidth(), GetScreenHeight() );

		var canvasRenderer = new RayLibCanvasRenderer( resolution, new RayLibCanvas() );
		_device = new RenderDevice( resolution, canvasRenderer );

		var cameraProfile = new CameraProfile
		(
			60 * DEG2RAD, resolution.AspectRatio,
			.1f, 100.0f
		);

		_camera = new Camera
		(
			Transform.Identity,
			cameraProfile
		);

		_scene = new Scene( _camera );

		var fox = new WorldObject
		(
			Mesh.LoadObj( "fox.obj" ),
			Transform.FromPosition( Vector3.UnitZ * 10f )
		);

		_spherePlanet = new WorldObject
		(
			Mesh.LoadObj( "sphere.obj" ),
			Transform.FromPosition( Vector3.UnitZ * -30f )
		);

		_scene.AddRange( fox, _spherePlanet );

		_stars = new List<Star>();

		var random = new Random();

		for ( int index = 0; index < 6000; index++ )
		{
			var starPosition = new Vector3
			(
				( random.NextSingle() - .5f ) * 2f * 200f,
				( random.NextSingle() - .5f ) * 2f * 200f,
				( random.NextSingle() - .5f ) * 2f * 200f
			);

			byte brightness = ( byte )random.Next( 100, 256 );

			var color = new Color( brightness, brightness, brightness, ( byte )255 );

			_stars.Add( new Star( starPosition, color ) );
		}
	}

	public void Initialize()
	{
	}

	public void Update()
	{
		_spherePlanet.Transform.Rotation += new Vector3( 1f, .8f, -.2f ) * GetFrameTime() * .2f;


		UpdateCameraMovement();

		UpdateZoom();
	}

	public void Draw()
	{
		ClearBackground( Color.BLACK );

		_device.Clear();

		_device.RenderScene( _scene );

		foreach ( Star star in _stars )
			_device.RenderWorldPixel( star.Position, star.Color, _camera );

		var testTriangle = new Triangle3D( Vector3.UnitY, Vector3.UnitX, Vector3.UnitZ );

		_alreadyClippedTriangles.Clear();
		var plane = new Plane3D( Vector3.UnitX * .1f, Vector3.UnitX );

		_device.RenderWorldRect( plane.Position, Vector2.One * 10, Color.YELLOW, _camera );

		// clip first plane
		if ( TryClipTriangle( plane, testTriangle, _alreadyClippedTriangles ) )
		{
			// if successfully clipped first, then clip second
			var secondPlane = new Plane3D( Vector3.UnitY * ( .7f + float.Sin( ( float )GetTime() ) * .1f ), -Vector3.UnitY );

			// clear working buffer
			_currentlyClippingTriangles.Clear();

			// dump all clipped triangles into currentlyClippingTriangles
			while ( _alreadyClippedTriangles.TryDequeue( out Triangle3D clippingTriangle ) )
				TryClipTriangle( secondPlane, clippingTriangle, _currentlyClippingTriangles );

			foreach ( Triangle3D currentlyClippingTriangle in _currentlyClippingTriangles )
				_alreadyClippedTriangles.Enqueue( currentlyClippingTriangle );
		}


		foreach ( Triangle3D clippedTriangle in _alreadyClippedTriangles )
			_device.RenderWorldTriangleLine( clippedTriangle, Color.RED, _camera );

		// original triangle
		_device.RenderWorldTriangleLine( testTriangle, Color.PINK.WithAlpha( 100 ), _camera );

		RenderWorldAxis( 80 );

		DrawFPS( 0, 0 );

		DrawText( $"Camera Position: {_camera.Transform.Position.ToString( "0.00" )}", 0, 20, 17, Color.GOLD );
		DrawText( $"Camera Forward: {_camera.Transform.BasisUnitZ.ToString( "0.00" )}", 0, 40, 17, Color.GOLD );
		DrawText( $"Camera Rotation: {_camera.Transform.Rotation.ToString( "0.00" )}", 0, 60, 17, Color.GOLD );
	}

	const float CAMERA_MOVE_SPEED = 5f;

	readonly Camera _camera;

	readonly Scene _scene;

	readonly IRenderDevice<Color> _device;

	readonly WorldObject _spherePlanet;

	readonly List<Star> _stars;

	readonly Queue<Triangle3D> _alreadyClippedTriangles = new();

	readonly Queue<Triangle3D> _currentlyClippingTriangles = new();

	Vector3 _cameraVelocity = Vector3.Zero;

	float _rotationX;
	float _rotationY;

	float _targetFovRadians;

	void UpdateZoom()
	{
		const float ZOOM_SPEED = 3.5f;

		if ( IsKeyDown( KeyboardKey.KEY_C ) )
			_targetFovRadians = 40 * MathUtils.DEG_2_RAD;

		if ( IsKeyUp( KeyboardKey.KEY_C ) )
			_targetFovRadians = 60 * MathUtils.DEG_2_RAD;

		_camera.Profile.FovRadians = MathUtils.Lerp( _camera.Profile.FovRadians, _targetFovRadians, ZOOM_SPEED * GetFrameTime() );
	}

	void RenderWorldAxis( int alpha = 255 )
	{
		_device.RenderWorldLineSegment( Vector3.Zero, Vector3.UnitX, Color.GREEN.WithAlpha( alpha ), _camera );
		_device.RenderWorldLineSegment( Vector3.Zero, Vector3.UnitY, Color.YELLOW.WithAlpha( alpha ), _camera );
		_device.RenderWorldLineSegment( Vector3.Zero, Vector3.UnitZ, Color.BLUE.WithAlpha( alpha ), _camera );
	}


	void UpdateCameraMovement()
	{
		int horizontal = GetKeyAxisStrength( KeyboardKey.KEY_A, KeyboardKey.KEY_D );
		int depth = GetKeyAxisStrength( KeyboardKey.KEY_S, KeyboardKey.KEY_W );

		int vertical = GetKeyAxisStrength( KeyboardKey.KEY_Q, KeyboardKey.KEY_E );

		// Move using basis vectors
		Vector3 input = _camera.Transform.BasisUnitX * horizontal +
						_camera.Transform.BasisUnitY * vertical +
						_camera.Transform.BasisUnitZ * -depth;

		const int SLOW_DOWN_SPEED = 9;
		const int ACCELERATE_UP_SPEED = 7;

		if ( input.LengthSquared() < 1 )
			_cameraVelocity = _cameraVelocity.Lerp( Vector3.Zero, GetFrameTime() * SLOW_DOWN_SPEED );
		else
			_cameraVelocity = _cameraVelocity.Lerp( input * CAMERA_MOVE_SPEED, GetFrameTime() * ACCELERATE_UP_SPEED );

		_camera.Transform.Position += _cameraVelocity * GetFrameTime();

		int horizontalRotation = GetKeyAxisStrength( KeyboardKey.KEY_LEFT, KeyboardKey.KEY_RIGHT );
		int verticalRotation = GetKeyAxisStrength( KeyboardKey.KEY_DOWN, KeyboardKey.KEY_UP );

		_rotationX -= horizontalRotation * GetFrameTime();
		_rotationY += verticalRotation * GetFrameTime();

		_camera.Transform.Rotation = Vector3.UnitY * _rotationX; // left right
		_camera.Transform.Rotation += Vector3.UnitX * _rotationY; // up down
	}

	static int GetKeyAxisStrength( KeyboardKey negativeKey, KeyboardKey positiveKey )
	{
		int resultStrength = 0;

		if ( IsKeyDown( negativeKey ) )
			resultStrength -= 1;

		if ( IsKeyDown( positiveKey ) )
			resultStrength += 1;

		return resultStrength;
	}

	static bool TryClipTriangle( Plane3D plane, Triangle3D triangleToClip, Queue<Triangle3D> clippedTriangles )
	{
		var insidePoints = new Vector3[ 3 ];
		var outsidePoints = new Vector3[ 3 ];

		int insidePointCount = 0, outsidePointCount = 0;

		for ( int i = 0; i < 3; i++ )
		{
			Vector3 triangleVertex = triangleToClip[ i ];

			float signedDistance = plane.GetSignShortDistance( triangleVertex );

			if ( signedDistance >= 0 )
			{
				insidePoints[ insidePointCount ] = triangleVertex;
				++insidePointCount;
			}
			else
			{
				outsidePoints[ outsidePointCount ] = triangleVertex;
				++outsidePointCount;
			}
		}

		// no points are inside the plane to clip, failed to clip the triangle
		if ( insidePointCount == 0 )
			return false;

		// the entire triangle is inside the clipping area, so no need to clip
		if ( insidePointCount == 3 )
		{
			clippedTriangles.Enqueue( triangleToClip );

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

			clippedTriangles.Enqueue( newTriangle );

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

			clippedTriangles.Enqueue( newFirstTriangle );
			clippedTriangles.Enqueue( newSecondTriangle );

			return true;
		}

		throw new Exception( "Impossible triangle clipping situation!" );
	}
}
internal static class ColorExtensions
{
	public static Color WithAlpha( this Color color, int alpha ) => new( color.r, color.g, color.b, alpha );
}
public readonly struct Star
{
	public Star( Vector3 position, Color color )
	{
		Position = position;
		Color = color;
	}

	public Vector3 Position { get; }
	public Color   Color    { get; }
}
public class RayLibCanvas : ICanvas<Color>
{
	public void DrawPixel( int x, int y, Color color ) =>
		Raylib.DrawPixel( x, y, color );

	public void Flush()
	{
	}
}
