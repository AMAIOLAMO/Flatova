using System.Numerics;
using Flatova.Geometry;
using Flatova.Geometry.Primitives;
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

		var cube = new WorldObject
		(
			MeshPrimitives.Cube,
			Transform.FromPosition( Vector3.UnitZ * -10f )
		);

		_spherePlanet = new WorldObject
		(
			Mesh.LoadObj( "sphere.obj" ),
			Transform.FromPosition( Vector3.UnitZ * -30f )
		);

		_scene.AddRange( fox, cube, _spherePlanet );

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

		RenderWorldAxis();

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

	void RenderWorldAxis()
	{
		_device.RenderWorldLineSegment3D( Vector3.Zero, Vector3.UnitX, Color.GREEN, _camera );
		_device.RenderWorldLineSegment3D( Vector3.Zero, Vector3.UnitY, Color.YELLOW, _camera );
		_device.RenderWorldLineSegment3D( Vector3.Zero, Vector3.UnitZ, Color.BLUE, _camera );
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
