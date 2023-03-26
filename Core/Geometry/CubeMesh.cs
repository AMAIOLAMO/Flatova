using System.Numerics;

public class CubeMesh : Mesh
{
	public CubeMesh() : base
	(
		new[]
		{
			new Vector3(1, 1, 1),
			new Vector3(1, 1, -1),
			new Vector3(1, -1, 1),
			new Vector3(1, -1, -1),
			
			new Vector3(-1, 1, 1),
			new Vector3(-1, 1, -1),
			new Vector3(-1, -1, 1),
			new Vector3(-1, -1, -1),
		}
	)
	{
	}
}
