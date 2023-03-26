namespace Flatova;

public readonly struct TriangleIndex
{
	public TriangleIndex( int first, int second, int third )
	{
		this.first = first;
		this.second = second;
		this.third = third;
	}
	
	public readonly int first;
	public readonly int second;
	public readonly int third;
}
