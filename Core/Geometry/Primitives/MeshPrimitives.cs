using System.Numerics;

namespace Flatova.Geometry.Primitives;

public static class MeshPrimitives
{
	public static Mesh Cube => new
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
			new( 6, 4, 2 ),
			new( 2, 4, 0 ),

			new( 7, 3, 5 ),
			new( 3, 1, 5 ),

			new( 2, 0, 3 ),
			new( 3, 0, 1 ),

			new( 7, 4, 6 ),
			new( 7, 5, 4 ),

			new( 0, 4, 5 ),
			new( 0, 5, 1 ),

			new( 2, 7, 6 ),
			new( 2, 3, 7 )
		}
	);
}
