using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Flatova;

public class SoftEngine : IApplication
{
	public void Initialize()
	{
		var resolution = new Resolution( GetScreenWidth(), GetScreenHeight() );

		_device = new RenderDevice( resolution );

		_transformedMesh = new TransformedMesh( new CubeMesh(), new Transform() );

		// Understand how camera works here
		_camera = new Camera( 160 * DEG2RAD, resolution.AspectRatio, 0.01f, 200.0f );
	}

	public void Update()
	{
		_transformedMesh!.Transform.Rotation += new Vector3( GetFrameTime(), GetFrameTime(), 0f );

		DrawText( _camera.Position.ToString(), 0, 20, 15, Color.GOLD );
	}

	public void Draw()
	{
		ClearBackground( Color.BLACK );

		DrawFPS( 0, 0 );

		_device!.RenderTransformed( _transformedMesh!, _camera! );
	}


	Camera? _camera;

	RenderDevice? _device;

	TransformedMesh? _transformedMesh;

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
