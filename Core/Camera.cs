using System.Numerics;

namespace Flatova;

// TODO: fix camera orientation matrices (view & projection)
public class Camera
{
	// TODO: Consider putting these settings into another data structure
	public Camera( float fovRadians, float aspectRatio, float nearPlaneDistance, float farPlaneDistance ) :
		this( Transform.Identity, fovRadians, aspectRatio, nearPlaneDistance, farPlaneDistance )
	{
	}

	public Camera( Transform transform, float fovRadians, float aspectRatio, float nearPlaneDistance, float farPlaneDistance )
	{
		_fovRadians = fovRadians;
		_aspectRatio = aspectRatio;

		Transform = transform;

		_nearPlaneDistance = nearPlaneDistance;
		_farPlaneDistance = farPlaneDistance;
	}

	public Matrix4x4 GetProjectionMatrix()
	{
		float yScale = 1.0f / MathF.Tan( _fovRadians * 0.5f );
		float q = _farPlaneDistance / ( _nearPlaneDistance - _farPlaneDistance );

		return new Matrix4x4
		{
			M11 = yScale / _aspectRatio,
			M22 = yScale,
			M33 = q,
			M34 = -1.0f,
			M43 = q * _nearPlaneDistance
		};

		// This is left hand perspective projection, not what we wanted :/
		// because after applying this projection matrix, it turns into right hand coordinate system
		// but we wanted a left hand coord system instead
		return Matrix4x4.CreatePerspectiveFieldOfView( _fovRadians, _aspectRatio, _nearPlaneDistance, _farPlaneDistance );
	}

	// Depth Projection
	public Vector3 VertexProjectDepthResolution( Vector3 vertex, Matrix4x4 worldMatrix, Resolution resolution ) =>
		WorldProjectDepthResolution( vertex.Transform( worldMatrix ), resolution );

	public Vector3 WorldProjectDepth( Vector3 worldPoint )
	{
		// 1f on w means we are trying to transform a position, instead of a direction
		Vector4 projectedPoint = new Vector4( worldPoint, 1f ).Transform( GetViewProjectionMatrix() );

		return new Vector3
		(
			// perspective division
			projectedPoint.X / projectedPoint.W,
			projectedPoint.Y / projectedPoint.W,
			projectedPoint.Z / projectedPoint.W
		);
	}

	// TODO: move this into resolution with a good name
	public Vector3 MapProjectedDepthToResolution( Vector3 projectedDepthPoint, Resolution resolution ) =>
		new
		(
			// map from -1f to 1f -> 0f to 1f -> 0f to resolution.Size
			( projectedDepthPoint.X + 1f ) * .5f * resolution.Width,
			( -projectedDepthPoint.Y + 1f ) * .5f * resolution.Height,
			projectedDepthPoint.Z
		);

	public Vector3 WorldProjectDepthResolution( Vector3 worldPoint, Resolution resolution ) =>
		MapProjectedDepthToResolution( WorldProjectDepth( worldPoint ), resolution );

	// Projection without depth
	public Vector2 WorldProjectResolution( Vector3 worldPoint, Resolution resolution )
	{
		Vector4 projectedPoint = new Vector4( worldPoint, 1f ).Transform( GetViewProjectionMatrix() );

		// requires specific perspective division system.Numerics' transform method does not do it
		return new Vector2
		(
			projectedPoint.X / projectedPoint.W * resolution.Width + resolution.HalfWidth,
			-projectedPoint.Y / projectedPoint.W * resolution.Height + resolution.HalfHeight
		);
	}

	public Matrix4x4 GetViewMatrix()
	{
		Matrix4x4 worldMatrix = Matrix4x4.CreateRotationX( Transform.Rotation.X ) * Matrix4x4.CreateRotationY( Transform.Rotation.Y ) * Matrix4x4.CreateTranslation( Transform.Position );

		Matrix4x4 viewMatrix = worldMatrix.Invert();

		return viewMatrix;
		
		return Matrix4x4.CreateLookAt( Transform.Position, GetCameraTarget(), Transform.BasisUnitY );
	}
	
	public Transform Transform { get; }

	readonly float _nearPlaneDistance;
	readonly float _farPlaneDistance;

	readonly float _fovRadians;

	readonly float _aspectRatio;

	Vector3 GetCameraTarget() =>
		Transform.Position + Transform.BasisUnitZ;

	Matrix4x4 GetViewProjectionMatrix() =>
		GetViewMatrix() * GetProjectionMatrix();
}
