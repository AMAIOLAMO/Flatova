using System.Numerics;

using Flatova.Geometry;

namespace Flatova;

public class WorldObject
{
	public WorldObject( Mesh mesh, Transform transform )
	{
		Mesh = mesh;
		Transform = transform;
	}

	public Matrix4x4 GetWorldMatrix() =>
		Transform.AsWorldMatrix();

	public Transform Transform { get; }

	public Mesh Mesh { get; }
}
