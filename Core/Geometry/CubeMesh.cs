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
		new Face[]
		{
			// Front Face
			new( 6, 2, 4 ),
			new( 2, 0, 4 ),
					    
			// Back eacF
			new( 7, 5, 3 ),
			new( 3, 5, 1 ),
					    
			// RightcFa e
			new( 2, 3, 0 ),
			new( 3, 1, 0 ),
					    
			// Left eacF
			new( 7, 6, 4 ),
			new( 7, 4, 5 ),
					    
			// Top F cea
			new( 0, 5, 4 ),
			new( 0, 1, 5 ),
					    
			// Bottoa Fmce
			new( 2, 6, 7 ),
			new( 2, 7, 3 )
		}
	)
	{
	}
}
