using System.Numerics;

namespace Flatova.Geometry;

public class Mesh
{
	public Mesh( Vector3[] vertices, Face[] triangles )
	{
		_vertices = vertices;

		_triangles = triangles;
	}

	public (Vector3 first, Vector3 second, Vector3 third) GetFaceVertices( Face face )
	{
		Vector3 first = Vertices[ face.First ];
		Vector3 second = Vertices[ face.Second ];
		Vector3 third = Vertices[ face.Third ];

		return ( first, second, third );
	}

	public ReadOnlySpan<Vector3> Vertices => new( _vertices );

	public ReadOnlySpan<Face> Triangles => new( _triangles );

	readonly Vector3[] _vertices;

	readonly Face[] _triangles;
}
