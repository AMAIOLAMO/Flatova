namespace Flatova.Geometry;

public readonly struct Face
{
	public Face( int first, int second, int third )
	{
		First = first;
		Second = second;
		Third = third;
	}

	public int First { get; }

	public int Second { get; }

	public int Third { get; }
}
