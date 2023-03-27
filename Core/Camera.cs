using System.Numerics;

namespace Flatova;

public class Camera
{
	// TODO: TOO MANY PARAMS!
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

	public Vector2 VertexToScreen( Vector3 vertex, Matrix4x4 worldMatrix, Resolution resolution ) =>
		WorldToScreen( vertex.Transform( worldMatrix ), resolution );

	public Vector2 WorldToScreen( Vector3 worldPoint, Resolution resolution )
	{
		Vector4 projectedScreenPercent = new Vector4( worldPoint, 1f ).Transform( GetViewProjectionMatrix() );

		// requires specific perspective division system.Numerics' transform method does not do it
		return new Vector2
		(
			projectedScreenPercent.X / projectedScreenPercent.W * resolution.Width,
			projectedScreenPercent.Y / projectedScreenPercent.W * resolution.Height
		) + resolution.Half;
	}

	// TODO: allow camera to rotate on yaw
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
