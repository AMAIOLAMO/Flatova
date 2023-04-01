using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

public static class FaceUtils
{
	/// <summary>
	///     Returns the normal of the given triangle face, in the order of clock wise
	/// </summary>
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 GetNormal( Vector3 origin, Vector3 first, Vector3 second ) =>
		( first - origin ).Cross( second - origin ).Normalize();

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 GetCenter( Vector3 a, Vector3 b, Vector3 c ) =>
		( a + b + c ) * 0.3333333333333333333f;
}
