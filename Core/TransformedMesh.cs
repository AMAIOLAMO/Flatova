using System.Numerics;

namespace Flatova;

public class TransformedMesh
{
	public TransformedMesh( Mesh mesh, Transform transform )
	{
		Mesh = mesh;
		Transform = transform;
	}

	public Transform Transform { get; }
	
	public Mesh Mesh { get; }

	public Matrix4x4 GetWorldMatrix() =>
		Transform.AsWorldMatrix();
}
