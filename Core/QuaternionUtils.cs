using System.Numerics;

namespace Flatova;

public static class QuaternionUtils
{
	public static Quaternion FromEuler( Vector3 eulerAngles ) =>
		Quaternion.CreateFromYawPitchRoll( eulerAngles.Y, eulerAngles.X, eulerAngles.Z );
}
