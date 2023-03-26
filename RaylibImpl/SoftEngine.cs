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

		_renderObjectList = new List<TransformedMesh>();

		_transformedMesh = new TransformedMesh( new CubeMesh(), new Transform( Vector3.Zero, Vector3.Zero ) );
		_secondMesh = new TransformedMesh( new CubeMesh(), new Transform( Vector3.One, Vector3.Zero ) );

		_renderObjectList.Add( _transformedMesh );
		_renderObjectList.Add( _secondMesh );

		// Understand how camera works here
		_camera = new Camera( -Vector3.UnitZ * 3, Vector3.UnitZ, 20f, 20f, 1f, 10 );
	}

	public void Update()
	{
		_secondMesh.Transform.Position = new Vector3( ( float )Math.Cos( GetTime() ) * 3f, ( float )Math.Sin( GetTime() ) * 3f, 0 );

		int horizontalInput = GetKeyAxisStrength( KeyboardKey.KEY_A, KeyboardKey.KEY_D );
		int verticalInput = GetKeyAxisStrength( KeyboardKey.KEY_S, KeyboardKey.KEY_W );

		_camera.Position += Vector3.UnitX * horizontalInput * GetFrameTime();
		_camera.Position += Vector3.UnitY * verticalInput * GetFrameTime();

		if ( IsMouseButtonDown( MouseButton.MOUSE_BUTTON_LEFT ) )
			_cameraRotation += GetFrameTime();

		Vector3 rotatedDirection = Vector3.Transform( Vector3.UnitX, Matrix4x4.CreateRotationY( _cameraRotation ) );

		_camera.LookDirection = rotatedDirection;
	}

	public void Draw()
	{
		ClearBackground( Color.BLACK );

		DrawFPS( 0, 0 );

		_device.RenderList( _renderObjectList, _camera );
	}


	Camera _camera;

	RenderDevice _device;

	TransformedMesh _transformedMesh;

	TransformedMesh _secondMesh;

	List<TransformedMesh> _renderObjectList;

	float _cameraRotation;

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
