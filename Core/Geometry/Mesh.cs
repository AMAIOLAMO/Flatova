using System.Numerics;

namespace Flatova.Geometry;

public class Mesh
{
	public Mesh( Vector3[] vertices, TriangleIndex[] triangles )
	{
		_vertices = vertices;

		_triangles = triangles;
	}

	public (Vector3 first, Vector3 second, Vector3 third) GetTriangleVertices( TriangleIndex triangleIndex )
	{
		Vector3 first = Vertices[ triangleIndex.First ];
		Vector3 second = Vertices[ triangleIndex.Second ];
		Vector3 third = Vertices[ triangleIndex.Third ];

		return ( first, second, third );
	}

	public ReadOnlySpan<Vector3> Vertices => new( _vertices );

	public ReadOnlySpan<TriangleIndex> Triangles => new( _triangles );

	readonly Vector3[] _vertices;

	readonly TriangleIndex[] _triangles;
}
