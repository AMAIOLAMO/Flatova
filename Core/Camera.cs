using System.Numerics;

namespace Flatova;

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

	public Matrix4x4 GetProjectionMatrix() =>
		Matrix4x4.CreatePerspectiveFieldOfView( _fovRadians, _aspectRatio, _nearPlaneDistance, _farPlaneDistance );

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

	public Vector3 MapProjectedDepthToResolution( Vector3 projectedDepthPoint, Resolution resolution ) =>
		new
		(
			projectedDepthPoint.X * resolution.Width + resolution.HalfWidth,
			projectedDepthPoint.Y * resolution.Height + resolution.HalfHeight,
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
			projectedPoint.Y / projectedPoint.W * resolution.Height + resolution.HalfHeight
		);
	}

	public Matrix4x4 GetViewMatrix() =>
		Matrix4x4.CreateLookAt( Transform.Position, GetCameraTarget(), Transform.BasisUnitY );

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
