using System.Numerics;
using Flatova.Rendering;
using Raylib_cs;
using static Raylib_cs.Raylib;
using Mesh = Flatova.Geometry.Mesh;

namespace Flatova;

public class RenderApplication : IApplication
{
	public RenderApplication()
	{
		var resolution = new Resolution( GetScreenWidth(), GetScreenHeight() );

		_device = new RenderDevice( resolution, new RayLibCanvasRenderer() );

		_fox = new WorldObject( Mesh.Load( "fox.obj" ) );
		// _axis = new WorldObject( Mesh.Load( "Axis.obj" ) );

		_camera = new Camera
		(
			Transform.FromPosition( Vector3.UnitZ * 10f ),
			60 * DEG2RAD, resolution.AspectRatio, 0.1f, 50.0f
		);
	}

	public void Initialize()
	{
	}

	public void Update()
	{
		// _sideMesh!.Transform.Position = new Vector3( float.Sin( ( float )GetTime() ), float.Cos( ( float )GetTime() ), 0f ) * 3;
		// _sideMesh!.Transform.Rotation += new Vector3( GetFrameTime(), 0f, GetFrameTime() );

		UpdateCameraMovement();
	}

	public void Draw()
	{
		ClearBackground( Color.BLUE );

		_device.ClearDepthBuffer();
		// _device.RenderObject( _axis, _camera );
		_device.RenderObject( _fox, _camera );

		_device.RenderLine3D( Vector3.Zero, Vector3.UnitX, Color.GREEN, _camera );
		_device.RenderLine3D( Vector3.Zero, Vector3.UnitY, Color.YELLOW, _camera );
		_device.RenderLine3D( Vector3.Zero, Vector3.UnitZ, Color.BLUE, _camera );

		_device.RenderLine3D( Vector3.Zero, _camera.Transform.BasisUnitX, Color.RED, _camera );
		_device.RenderLine3D( Vector3.Zero, _camera.Transform.BasisUnitY, Color.RED, _camera );
		_device.RenderLine3D( Vector3.Zero, _camera.Transform.BasisUnitZ, Color.RED, _camera );
		DrawFPS( 0, 0 );

		DrawText( $"Camera Position: {_camera.Transform.Position.ToString( "0.00" )}", 0, 20, 17, Color.GOLD );
		DrawText( $"Camera Forward: {_camera.Transform.BasisUnitZ.ToString( "0.00" )}", 0, 40, 17, Color.GOLD );
		DrawText( $"Camera Rotation: {_camera.Transform.Rotation.ToString( "0.00" )}", 0, 60, 17, Color.GOLD );
	}

	const float CAMERA_MOVE_SPEED = 5f;

	readonly Camera _camera;

	readonly IRenderDevice<Color> _device;

	readonly WorldObject _fox;

	Vector3 _cameraVelocity = Vector3.Zero;

	float _rotationX;
	float _rotationY = float.Pi;


	void UpdateCameraMovement()
	{
		int horizontal = GetKeyAxisStrength( KeyboardKey.KEY_A, KeyboardKey.KEY_D );
		int depth = GetKeyAxisStrength( KeyboardKey.KEY_S, KeyboardKey.KEY_W );

		int vertical = GetKeyAxisStrength( KeyboardKey.KEY_Q, KeyboardKey.KEY_E );

		// Move using basis vectors
		Vector3 input = _camera.Transform.BasisUnitX * horizontal +
						_camera.Transform.BasisUnitY * vertical +
						_camera.Transform.BasisUnitZ * depth;

		const int SLOW_DOWN_SPEED = 9;
		const int ACCELERATE_UP_SPEED = 7;

		if ( input.LengthSquared() < 1 )
			_cameraVelocity = _cameraVelocity.Lerp( Vector3.Zero, GetFrameTime() * SLOW_DOWN_SPEED );
		else
			_cameraVelocity = _cameraVelocity.Lerp( input * CAMERA_MOVE_SPEED, GetFrameTime() * ACCELERATE_UP_SPEED );

		_camera.Transform.Position += _cameraVelocity * GetFrameTime();

		int horizontalRotation = GetKeyAxisStrength( KeyboardKey.KEY_LEFT, KeyboardKey.KEY_RIGHT );
		int verticalRotation = GetKeyAxisStrength( KeyboardKey.KEY_DOWN, KeyboardKey.KEY_UP );

		_rotationX += horizontalRotation * GetFrameTime();
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
public class RayLibCanvasRenderer : ICanvasRenderer<Color>
{
	public void DrawPixel( int x, int y, Color color ) =>
		Raylib.DrawPixel( x, y, color );
}
