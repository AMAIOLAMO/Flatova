using System.Numerics;

public class Mesh
{
	public Mesh( Vector3[] vertices ) =>
		_vertices = vertices;

	public ReadOnlySpan<Vector3> Vertices => _vertices.AsSpan();

	readonly Vector3[] _vertices;
}
