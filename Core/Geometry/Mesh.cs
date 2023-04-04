using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova.Geometry;

public class Mesh
{
	public Mesh( Vector3[] vertices, TriangleIndex[] triangleIndices )
	{
		Vertices = vertices;
		TriangleIndices = triangleIndices;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Vector3 GetFaceNormal( TriangleIndex triangleIndex ) =>
		FaceUtils.GetNormal
		(
			Vertices[ triangleIndex.First ],
			Vertices[ triangleIndex.Second ],
			Vertices[ triangleIndex.Third ]
		);

	public Triangle3D GetTriangleFromIndex( TriangleIndex triangleIndex ) =>
		new
		(
			Vertices[ triangleIndex.First ],
			Vertices[ triangleIndex.Second ],
			Vertices[ triangleIndex.Third ]
		);

	public Vector3[] Vertices { get; }

	public TriangleIndex[] TriangleIndices { get; }

	// TODO: Parse Polygons where faces shows: "f a/b/c a/b/c a/b/c a/b/c ..."
	// TODO: Parse into a class called OBJ instead of Mesh, as obj stores textures n stuff data that is NOT related to the mesh
	public static Mesh LoadObj( string filePath )
	{
		using FileStream fileStream = File.OpenRead( filePath );

		var reader = new StreamReader( fileStream );

		List<Vector3> vertices = new();
		List<TriangleIndex> triangleIndices = new();

		while ( !reader.EndOfStream )
		{
			string? line = reader.ReadLine();

			if ( line == null )
				continue;

			if ( line.StartsWith( "v " ) )
			{
				string[] splitLines = line.Split( ' ' );

				var vertex = new Vector3
				(
					float.Parse( splitLines[ 1 ] ),
					float.Parse( splitLines[ 2 ] ),
					float.Parse( splitLines[ 3 ] )
				);

				vertices.Add( vertex );
			}
			else if ( line.StartsWith( "f " ) )
			{
				string[] splitLines = line.Split( ' ' );

				// order is reversed, as obj stores triangle indices in counter-clockwise order
				int first = int.Parse( splitLines[ 1 ] ) - 1;
				int second = int.Parse( splitLines[ 3 ] ) - 1;
				int third = int.Parse( splitLines[ 2 ] ) - 1;

				var triangleIndex = new TriangleIndex
				(
					first, second, third
				);

				triangleIndices.Add( triangleIndex );
			}
		}

		return new Mesh( vertices.ToArray(), triangleIndices.ToArray() );
	}
}
