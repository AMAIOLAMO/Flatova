using System.Numerics;

namespace Flatova.Geometry;

/// <summary>
///     Representing a plane in 3D space
/// </summary>
public readonly struct Plane3D
{
	public Plane3D( Vector3 position, Vector3 normal )
	{
		Position = position;
		Normal = normal.Normalize();
	}

	public Vector3 Position { get; }
	public Vector3 Normal   { get; }
	
	// x * N_x + y * N_x + z * N_x = P * N
}
