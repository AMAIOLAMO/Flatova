using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova.Geometry;

public class Mesh
{
	public Mesh( Vector3[] vertices, Face[] triangles )
	{
		_vertices = vertices;

		_triangles = triangles;
	}

	public void GetFaceVertices( in Face face, out Vector3 first, out Vector3 second, out Vector3 third )
	{
		first = Vertices[ face.First ];
		second = Vertices[ face.Second ];
		third = Vertices[ face.Third ];
	}

	public (Vector3 first, Vector3 second, Vector3 third) GetFaceVertices( Face face )
	{
		Vector3 first = Vertices[ face.First ];
		Vector3 second = Vertices[ face.Second ];
		Vector3 third = Vertices[ face.Third ];

		return ( first, second, third );
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Vector3 GetFaceNormal( Face face ) =>
		FaceUtils.GetNormal
		(
			Vertices[ face.First ],
			Vertices[ face.Second ],
			Vertices[ face.Third ]
		);

	public ReadOnlySpan<Vector3> Vertices => new( _vertices );

	public ReadOnlySpan<Face> Triangles => new( _triangles );

	readonly Vector3[] _vertices;

	readonly Face[] _triangles;
}
