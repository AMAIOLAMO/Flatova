using System.Numerics;

namespace Flatova.Geometry;

/// <summary>
///     Represents three points in a 3D space
/// </summary>
public readonly struct Triangle3D
{
	public Triangle3D( Vector3 first, Vector3 second, Vector3 third )
	{
		First = first;
		Second = second;
		Third = third;
	}

	public Triangle3D Transform( Matrix4x4 worldMatrix ) =>
		new
		(
			First.Transform( worldMatrix ),
			Second.Transform( worldMatrix ),
			Third.Transform( worldMatrix )
		);

	public Vector3 GetNormal() =>
		FaceUtils.GetNormal( First, Second, Third );

	public Vector3 GetCenter() =>
		( First + Second + Third ) * 0.3333333333333333333f;

	public Vector3 First  { get; }
	public Vector3 Second { get; }
	public Vector3 Third  { get; }
}
