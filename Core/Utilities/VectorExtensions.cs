using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

public static class VectorExtensions
{
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Transform( this Vector3 vector, Matrix4x4 matrix ) =>
		Vector3.Transform( vector, matrix );


	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Normalize( this Vector3 vector ) =>
		Vector3.Normalize( vector );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Lerp( this Vector3 from, Vector3 target, float delta ) =>
		Vector3.Lerp( from, target, delta );
}
