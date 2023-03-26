using Raylib_cs;
using System.Numerics;
using Flatova.Geometry;
using Flatova.Rendering;
using static Raylib_cs.Raylib;

namespace Flatova;

public class RenderApplication : IApplication
{
	public void Initialize()
	{
		var resolution = new Resolution( GetScreenWidth(), GetScreenHeight() );

		_device = new RenderDevice( resolution );

		_centerMesh = new WorldObject( new CubeMesh(), new Transform() );

		// Understand how camera works here
		_camera = new Camera( 160 * DEG2RAD, resolution.AspectRatio, 0.01f, 200.0f );
	}

	public void Update()
	{
		_centerMesh!.Transform.Rotation += new Vector3( GetFrameTime(), GetFrameTime(), 0f );

		int horizontal = GetKeyAxisStrength( KeyboardKey.KEY_A, KeyboardKey.KEY_D );
		int vertical = GetKeyAxisStrength( KeyboardKey.KEY_S, KeyboardKey.KEY_W );

		_camera.Position += new Vector3( horizontal, vertical, 0f ) * GetFrameTime() * CAMERA_MOVE_SPEED;
	}

	public void Draw()
	{
		ClearBackground( Color.BLACK );

		DrawFPS( 0, 0 );

		DrawText( $"Camera Position: {_camera!.Position.ToString()}", 0, 20, 17, Color.GOLD );

		_device!.RenderTransformed( _centerMesh!, _camera! );
	}

	const float CAMERA_MOVE_SPEED = 5f;

	Camera? _camera;

	RenderDevice? _device;

	WorldObject? _centerMesh;

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
