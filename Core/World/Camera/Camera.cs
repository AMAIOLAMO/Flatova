using System.Numerics;

namespace Flatova.World;

// TODO: fix camera orientation matrices (view & projection)
public class Camera
{
	public Camera( Transform transform, CameraProfile profile )
	{
		Transform = transform;
		Profile = profile;
	}

	public Matrix4x4 GetProjectionMatrix() =>
		Matrix4x4.CreatePerspectiveFieldOfView
		(
			Profile.FovRadians, Profile.AspectRatio,
			Profile.NearPlaneDistance, Profile.FarPlaneDistance
		);

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

	public Vector3 WorldProjectDepthResolution( Vector3 worldPoint, Resolution resolution ) =>
		resolution.MapProjectedDepth( WorldProjectDepth( worldPoint ) );

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

		// return Matrix4x4.CreateLookAt( Transform.Position, GetCameraTarget(), Transform.BasisUnitY );
	}

	public CameraProfile Profile { get; }

	public Transform Transform { get; }

	Vector3 GetCameraTarget() =>
		Transform.Position + Transform.BasisUnitZ;

	Matrix4x4 GetViewProjectionMatrix() =>
		GetViewMatrix() * GetProjectionMatrix();
}
