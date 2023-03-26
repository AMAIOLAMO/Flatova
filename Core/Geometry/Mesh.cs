using System.Numerics;

namespace Flatova;

public class Mesh
{
	public Mesh( Vector3[] vertices, TriangleIndex[] triangles )
	{
		_vertices = vertices;

		_triangles = triangles;
	}

	public ReadOnlySpan<Vector3> Vertices => _vertices.AsSpan();

	readonly Vector3[] _vertices;

	readonly TriangleIndex[] _triangles;
}
