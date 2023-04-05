namespace Flatova.World;

/// <summary>
///     Represents a collection of various settings for the <see cref="Camera" />
/// </summary>
public class CameraProfile
{
	public CameraProfile( float fovRadians, float aspectRatio, float nearPlaneDistance, float farPlaneDistance )
	{
		FovRadians = fovRadians;
		AspectRatio = aspectRatio;

		NearPlaneDistance = nearPlaneDistance;
		FarPlaneDistance = farPlaneDistance;
	}

	public float FovRadians  { get; set; }
	public float AspectRatio { get; set; }

	public float NearPlaneDistance { get; }
	public float FarPlaneDistance  { get; }
}
