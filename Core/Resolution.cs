using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

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

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Vector3 MapProjectedDepth( Vector3 projectedDepthPoint ) =>
		new
		(
			// map from -1f to 1f -> 0f to 1f -> 0f to resolution.Size
			( projectedDepthPoint.X + 1f ) * .5f * Width,
			( -projectedDepthPoint.Y + 1f ) * .5f * Height,
			projectedDepthPoint.Z
		);

	public float AspectRatio => ( float )Width / Height;

	public int Width  { get; }
	public int Height { get; }

	public Vector2 Half => new( HalfWidth, HalfHeight );

	public float HalfWidth  => Width * 0.5f;
	public float HalfHeight => Height * 0.5f;
}
