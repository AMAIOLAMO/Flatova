using System.Numerics;
using System.Runtime.CompilerServices;

public readonly struct Resolution
{
	public Resolution( int width, int height )
	{
		Width = width;
		Height = height;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public bool Contains( int x, int y ) =>
		x >= 0 && x < Width && y >= 0 && y < Height;
	
	public float AspectRatio => ( float )Width / Height;

	public int Width  { get; }
	public int Height { get; }

	public Vector2 Half => new( HalfWidth, HalfHeight );

	public float HalfWidth  => Width * 0.5f;
	public float HalfHeight => Height * 0.5f;
}
