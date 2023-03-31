using System.Numerics;
using Flatova.Geometry;

namespace Flatova;

public class WorldObject
{
	public WorldObject( Mesh mesh ) : this( Transform.Identity, mesh ) { }

	public WorldObject( Transform transform, Mesh mesh )
	{
		Transform = transform;
		Mesh = mesh;
	}

	public Matrix4x4 GetWorldMatrix() =>
		Transform.GetWorldMatrix();

	public Transform Transform { get; }

	public Mesh Mesh { get; }
}
