using System.Numerics;
using System.Runtime.CompilerServices;

namespace Flatova;

public static class Matrix4X4Utils
{
	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public static Matrix4x4 FromEuler( Vector3 eulerAngles ) =>
		Matrix4x4.CreateFromYawPitchRoll( eulerAngles.Y, eulerAngles.X, eulerAngles.Z );
}
