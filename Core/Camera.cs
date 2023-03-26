using System.Numerics;

namespace Flatova;

public class Camera
{
	// TODO: TOO MANY PARAMS!
	public Camera( Vector3 position, Vector3 lookDirection, float fov, float aspectRatio, float nearPlaneDistance, float farPlaneDistance )
	{
		Position = position;
		LookDirection = lookDirection;

		_fov = fov;
		_aspectRatio = aspectRatio;

		_nearPlaneDistance = nearPlaneDistance;
		_farPlaneDistance = farPlaneDistance;
	}

	public Matrix4x4 GetProjectionMatrix() =>
		Matrix4x4.CreatePerspectiveFieldOfView( _fov, _aspectRatio, _nearPlaneDistance, _farPlaneDistance );

	public Vector2 ProjectToScreen( Vector3 worldPoint, Resolution resolution )
	{
		Vector3 projectedWorldPoint = Vector3.Transform( worldPoint, GetProjectionMatrix() );

		return new Vector2
		(
			projectedWorldPoint.X * resolution.Width,
			-projectedWorldPoint.Y * resolution.Width
		) + resolution.Half;
	}

	// TODO: allow camera to rotate on jaw
	public Matrix4x4 GetViewMatrix() =>
		Matrix4x4.CreateLookAt( Position, GetAbsoluteLookDirection(), Vector3.UnitY );

	public Vector3 Position { get; set; }
	public Vector3 LookDirection { get; set; }

	readonly float _nearPlaneDistance;
	readonly float _farPlaneDistance;

	readonly float _fov;

	readonly float _aspectRatio;

	Vector3 GetAbsoluteLookDirection() =>
		Position + LookDirection;
}
