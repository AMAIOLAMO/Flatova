using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Flatova.Rendering;

// TODO: Currently Depth Map will never check if the point is contained

/// <summary>
///     Represents a float map for depth (similar to a float 2 dimensional array),
///     Currently un-resizable
/// </summary>
public class DepthMap
{
	public DepthMap( Resolution resolution )
	{
		_resolution = resolution;
		_depthBuffer = new float[ resolution.Width * resolution.Height ];
	}

	public void Clear()
	{
		for ( int index = 0; index < _depthBuffer.Length; index++ )
			_depthBuffer[ index ] = float.MaxValue;
	}

	public bool IsCloser( int x, int y, float depth )
	{
		Debug.Assert( depth >= 0, "depth should not be below 0!" );

		float bufferDepth = GetDepth( x, y );

		return depth < bufferDepth;
	}

	public void SetDepth( int x, int y, float depth )
	{
		Debug.Assert( depth >= 0, "depth should not be below 0!" );

		int index = MathUtils.FlattenIndex2D( x, y, _resolution.Width );

		_depthBuffer[ index ] = depth;
	}

	readonly Resolution _resolution;

	readonly float[] _depthBuffer;

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	float GetDepth( int x, int y )
	{
		int index = MathUtils.FlattenIndex2D( x, y, _resolution.Width );

		return _depthBuffer[ index ];
	}
}
