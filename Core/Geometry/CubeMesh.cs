using System.Numerics;

namespace Flatova.Geometry;

public class CubeMesh : Mesh
{
	public CubeMesh() : base
	(
		new Vector3[]
		{
			new( 1, 1, 1 ),
			new( 1, 1, -1 ),
			new( 1, -1, 1 ),
			new( 1, -1, -1 ),

			new( -1, 1, 1 ),
			new( -1, 1, -1 ),
			new( -1, -1, 1 ),
			new( -1, -1, -1 )
		},
		new TriangleIndex[]
		{
			// Front Face
			new( 6, 4, 2 ),
			new( 2, 4, 0 ),

			// Back Face
			new( 7, 3, 5 ),
			new( 3, 1, 5 ),

			// Right Face
			new( 2, 0, 3 ),
			new( 3, 0, 1 ),

			// Left Face
			new( 7, 4, 6 ),
			new( 7, 5, 4 ),

			// Top Face
			new( 0, 4, 5 ),
			new( 0, 5, 1 ),

			// Bottom Face
			new( 2, 7, 6 ),
			new( 2, 3, 7 )
		}
	)
	{
	}
}
