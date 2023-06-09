using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

public static class VectorExtensions
{
	public static Vector2 To2D( this Vector3 vector ) => new( vector.X, vector.Y );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static bool AlmostEquals( this Vector3 vector, Vector3 other, float epsilon = float.Epsilon ) =>
		MathUtils.AlmostEquals( vector.X, other.X, epsilon ) &&
		MathUtils.AlmostEquals( vector.Y, other.Y, epsilon ) &&
		MathUtils.AlmostEquals( vector.Z, other.Z, epsilon );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Transform( this Vector3 vector, Matrix4x4 matrix ) =>
		Vector3.Transform( vector, matrix );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector4 Transform( this Vector4 vector, Matrix4x4 matrix ) =>
		Vector4.Transform( vector, matrix );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Normalize( this Vector3 vector ) =>
		Vector3.Normalize( vector );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static float Dot( this Vector3 vector, Vector3 other ) =>
		Vector3.Dot( vector, other );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Cross( this Vector3 vector, Vector3 other ) =>
		Vector3.Cross( vector, other );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Vector3 Lerp( this Vector3 from, Vector3 target, float delta ) =>
		Vector3.Lerp( from, target, delta );
}
