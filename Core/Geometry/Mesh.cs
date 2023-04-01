using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova.Geometry;

public class Mesh
{
	public Mesh( Vector3[] vertices, Face[] faces )
	{
		Vertices = vertices;
		Faces = faces;
	}

	public void GetFaceVertices( in Face face, out Vector3 first, out Vector3 second, out Vector3 third )
	{
		first = Vertices[ face.First ];
		second = Vertices[ face.Second ];
		third = Vertices[ face.Third ];
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Vector3 GetFaceNormal( Face face ) =>
		FaceUtils.GetNormal
		(
			Vertices[ face.First ],
			Vertices[ face.Second ],
			Vertices[ face.Third ]
		);

	public Vector3[] Vertices { get; }

	public Face[] Faces { get; }
}
