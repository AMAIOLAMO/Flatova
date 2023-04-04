using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

public static class Matrix4X4Utils
{
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Matrix4x4 FromEulerRadians( Vector3 eulerAnglesRadians ) =>
		Matrix4x4.CreateFromYawPitchRoll( eulerAnglesRadians.Y, eulerAnglesRadians.X, eulerAnglesRadians.Z );

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Matrix4x4 FromEulerDegrees( Vector3 eulerAnglesDegrees ) =>
		FromEulerRadians( eulerAnglesDegrees * MathUtils.DEG_2_RAD );

	public static Matrix4x4 Invert( this Matrix4x4 matrix )
	{
		Matrix4x4.Invert( matrix, out Matrix4x4 result );

		return result;
	}
}
