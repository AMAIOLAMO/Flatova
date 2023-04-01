using System.Numerics;
using Flatova.Geometry;
using Flatova.Rendering;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Flatova;

public class RenderApplication : IApplication
{
	public RenderApplication()
	{
		var resolution = new Resolution( GetScreenWidth(), GetScreenHeight() );

		_device = new RenderDevice( resolution, new RayLibCanvasRenderer() );

		_centerMesh = new WorldObject( new CubeMesh() );

		_sideMesh = new WorldObject( new CubeMesh() )
		{
			Transform =
			{
				Scale = new Vector3( 2f, 1f, .5f )
			}
		};

		_camera = new Camera
		(
			Transform.FromPosition( Vector3.UnitZ * 10f ),
			60 * DEG2RAD, resolution.AspectRatio, 0.1f, 1.0f
		);
	}

	public void Initialize()
	{
	}

	public void Update()
	{
		_sideMesh!.Transform.Position = new Vector3( float.Sin( ( float )GetTime() ), float.Cos( ( float )GetTime() ), 0f ) * 3;
		_sideMesh!.Transform.Rotation += new Vector3( GetFrameTime(), 0f, GetFrameTime() );

		UpdateCameraMovement();
	}

	public void Draw()
	{
		ClearBackground( Color.BLUE );

		DrawFPS( 0, 0 );

		DrawText( $"Camera Position: {_camera!.Transform.Position.ToString( "0.00" )}", 0, 20, 17, Color.GOLD );

		_device!.ClearDepthBuffer();

		_device!.RenderObject( _sideMesh, _camera );
		_device!.RenderObject( _centerMesh, _camera );
	}

	const float CAMERA_MOVE_SPEED = 5f;

	readonly Camera _camera;

	readonly IRenderDevice<Color> _device;

	readonly WorldObject _sideMesh;
	readonly WorldObject _centerMesh;

	Vector3 _cameraVelocity = Vector3.Zero;

	float _rotationX;
	float _rotationY;


	void UpdateCameraMovement()
	{
		int horizontal = GetKeyAxisStrength( KeyboardKey.KEY_A, KeyboardKey.KEY_D );
		int depth = GetKeyAxisStrength( KeyboardKey.KEY_S, KeyboardKey.KEY_W );

		int vertical = GetKeyAxisStrength( KeyboardKey.KEY_Q, KeyboardKey.KEY_E );

		// Move using basis vectors
		Vector3 input = _camera!.Transform.BasisUnitX * horizontal +
						_camera.Transform.BasisUnitY * vertical +
						_camera.Transform.BasisUnitZ * depth;

		const int SLOW_DOWN_SPEED = 9;
		const int ACCELERATE_UP_SPEED = 7;

		if ( input.LengthSquared() < 1 )
			_cameraVelocity = _cameraVelocity.Lerp( Vector3.Zero, GetFrameTime() * SLOW_DOWN_SPEED );
		else
			_cameraVelocity = _cameraVelocity.Lerp( input * CAMERA_MOVE_SPEED, GetFrameTime() * ACCELERATE_UP_SPEED );


		_camera!.Transform.Position += _cameraVelocity * GetFrameTime();

		_rotationX -= GetMouseDelta().X * GetFrameTime() * 0.1f;
		_rotationY -= GetMouseDelta().Y * GetFrameTime() * 0.1f;

		_camera.Transform.Rotation = Vector3.UnitY * _rotationX;
		_camera.Transform.Rotation += Vector3.UnitX * _rotationY;
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
