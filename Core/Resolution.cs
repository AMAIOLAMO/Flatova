using System.Numerics;

public readonly struct Resolution
{
	public Resolution( int width, int height )
	{
		Width = width;
		Height = height;
	}

	public float AspectRatio  => ( float )Width / Height;

	public int Width  { get; }
	public int Height { get; }

	public Vector2 Half => new( HalfWidth, HalfHeight );

	public float HalfWidth  => Width * 0.5f;
	public float HalfHeight => Height * 0.5f;
}
