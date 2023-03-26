using System.Runtime.InteropServices;

namespace Flatova.Geometry;

[StructLayout( LayoutKind.Explicit )]
public readonly struct TriangleIndex
{
	public TriangleIndex( int first, int second, int third )
	{
		First = first;
		Second = second;
		Third = third;
	}

	[field: FieldOffset( 0 )]
	public int First { get; }

	[field: FieldOffset( sizeof( int ) * 1 )]
	public int Second { get; }

	[field: FieldOffset( sizeof( int ) * 2 )]
	public int Third { get; }
}
