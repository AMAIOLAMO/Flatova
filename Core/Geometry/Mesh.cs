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

	public static Mesh Load( string filePath )
	{
		using FileStream fileStream = File.OpenRead( filePath );

		var reader = new StreamReader( fileStream );

		List<Vector3> vertices = new();
		List<Face> faces = new();

		while ( !reader.EndOfStream )
		{
			string? line = reader.ReadLine();

			if ( line == null )
				continue;

			if ( line.StartsWith( 'v' ) )
			{
				string[] splitLines = line.Split( ' ' );

				var vertex = new Vector3
				(
					float.Parse( splitLines[ 1 ] ),
					float.Parse( splitLines[ 2 ] ),
					float.Parse( splitLines[ 3 ] )
				);

				vertices.Add( vertex );
				// Console.WriteLine( $"vertex: {vertex}" );
			}
			else if ( line.StartsWith( 'f' ) )
			{
				string[] splitLines = line.Split( ' ' );

				int first = int.Parse( splitLines[ 1 ] ) - 1;
				int second = int.Parse( splitLines[ 2 ] ) - 1;
				int third = int.Parse( splitLines[ 3 ] ) - 1;

				var face = new Face
				(
					first, second, third
				);

				faces.Add( face );
			}
		}

		return new Mesh( vertices.ToArray(), faces.ToArray() );
	}
}
