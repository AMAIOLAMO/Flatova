using System.Numerics;
using Flatova.Geometry;
using Flatova.Rendering;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Flatova;

public class RenderApplication : IApplication
{
	public void Initialize()
	{
		var resolution = new Resolution( GetScreenWidth(), GetScreenHeight() );

		_device = new RenderDevice( resolution );

		_centerMesh = new WorldObject( new CubeMesh(), new Transform() );
		_sideMesh = new WorldObject( new CubeMesh(), new Transform() );

		// Understand how camera works here
		_camera = new Camera( 60 * DEG2RAD, resolution.AspectRatio, 0.1f, 1.0f );
	}

	public void Update()
	{
		// _centerMesh!.Transform.Rotation += new Vector3( GetFrameTime(), GetFrameTime(), 0f );

		_sideMesh!.Transform.Position = new Vector3( float.Sin( ( float )GetTime() ), float.Cos( ( float )GetTime() ), 0f ) * 3;
		_sideMesh!.Transform.Rotation += new Vector3( GetFrameTime(), 0f, GetFrameTime() );

		UpdateCameraMovement();
	}

	public void Draw()
	{
		ClearBackground( Color.BLACK );

		DrawFPS( 0, 0 );

		DrawText( $"Camera Position: {_camera!.Position.ToString( "0.00" )}", 0, 20, 17, Color.GOLD );

		_device!.RenderObject( _centerMesh!, _camera! );
		_device!.RenderObject( _sideMesh!, _camera! );
	}

	const float CAMERA_MOVE_SPEED = 5f;

	Vector3 _cameraVelocity = Vector3.Zero;

	Camera? _camera;

	RenderDevice? _device;

	WorldObject? _centerMesh;
	WorldObject? _sideMesh;

	void UpdateCameraMovement()
	{
		int horizontal = GetKeyAxisStrength( KeyboardKey.KEY_A, KeyboardKey.KEY_D );
		int vertical = GetKeyAxisStrength( KeyboardKey.KEY_S, KeyboardKey.KEY_W );

		int depth = GetKeyAxisStrength( KeyboardKey.KEY_E, KeyboardKey.KEY_Q );

		var input = new Vector3( horizontal, vertical, depth );

		const int SLOW_DOWN_SPEED = 9;
		const int ACCELERATE_UP_SPEED = 7;

		if ( input.LengthSquared() < 1 )
			_cameraVelocity = _cameraVelocity.Lerp( Vector3.Zero, GetFrameTime() * SLOW_DOWN_SPEED );
		else
			_cameraVelocity = _cameraVelocity.Lerp( input * CAMERA_MOVE_SPEED, GetFrameTime() * ACCELERATE_UP_SPEED );


		_camera!.Position += _cameraVelocity * GetFrameTime();
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
