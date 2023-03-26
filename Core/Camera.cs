using System.Numerics;

namespace Flatova;

public class Camera
{
	// TODO: TOO MANY PARAMS!
	public Camera( float fovRadians, float aspectRatio, float nearPlaneDistance, float farPlaneDistance )
	{
		_fovRadians = fovRadians;
		_aspectRatio = aspectRatio;

		Position = Vector3.UnitZ * 3;
		LookDirection = -Vector3.UnitZ;

		_nearPlaneDistance = nearPlaneDistance;
		_farPlaneDistance = farPlaneDistance;
	}

	public Matrix4x4 GetProjectionMatrix() =>
		Matrix4x4.CreatePerspectiveFieldOfView( _fovRadians, _aspectRatio, _nearPlaneDistance, _farPlaneDistance );

	public Vector2 VertexToScreen( Vector3 vertex, Matrix4x4 worldMatrix, Resolution resolution ) =>
		WorldToScreen( vertex.Transform( worldMatrix ), resolution );

	public Vector2 WorldToScreen( Vector3 worldPoint, Resolution resolution )
	{
		Vector3 projectedScreenPercent = worldPoint.Transform( GetViewProjectionMatrix() );

		return new Vector2
		(
			projectedScreenPercent.X * resolution.Width,
			-projectedScreenPercent.Y * resolution.Height
		) + resolution.Half;
	}

	// TODO: allow camera to rotate on yaw
	public Matrix4x4 GetViewMatrix() =>
		Matrix4x4.CreateLookAt( Position, GetLookTarget(), Vector3.UnitY );

	public Vector3 Position      { get; set; }
	public Vector3 LookDirection { get; set; }


	readonly float _nearPlaneDistance;
	readonly float _farPlaneDistance;

	readonly float _fovRadians;

	readonly float _aspectRatio;

	Matrix4x4 GetViewProjectionMatrix() =>
		GetViewMatrix() * GetProjectionMatrix();

	Vector3 GetLookTarget() =>
		Position + LookDirection;
}
