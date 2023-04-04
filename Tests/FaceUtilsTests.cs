using System.Numerics;
using Flatova;

namespace Tests;

public class FaceUtilsTests
{
	[Test]
	public void GetNormal_UnitY()
	{
		Vector3 normal = FaceUtils.GetNormal( Vector3.Zero, Vector3.UnitZ, Vector3.UnitX );

		Assert.That( normal, Is.EqualTo( Vector3.UnitY ) );
	}

	[Test]
	public void GetNormal_UnitZ()
	{
		Vector3 normal = FaceUtils.GetNormal( Vector3.Zero, Vector3.UnitX, Vector3.UnitY );

		Assert.That( normal, Is.EqualTo( Vector3.UnitZ ) );
	}
}
