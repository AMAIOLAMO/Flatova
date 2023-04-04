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
		( second - origin ).Cross( first - origin ).Normalize();
}
