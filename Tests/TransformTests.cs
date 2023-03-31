using System.Numerics;
using Flatova;

namespace Tests;

public class TransformTests
{
	[Test]
	public void ConstructPositionEqualZero()
	{
		Transform transform = Transform.Identity;

		Assert.That( transform.Position, Is.EqualTo( Vector3.Zero ) );
	}

	[Test]
	public void ConstructRotationEqualsZero()
	{
		Transform transform = Transform.Identity;

		Assert.That( transform.Rotation, Is.EqualTo( Vector3.Zero ) );
	}

	[Test]
	public void ConstructFromPosition()
	{
		Transform transform = Transform.FromPosition( new Vector3( 2034.2f, 1293.1f, 1247.112389f ) );

		Assert.That( transform.Position, Is.EqualTo( new Vector3( 2034.2f, 1293.1f, 1247.112389f ) ) );
	}

	[Test]
	public void ConstructFromRotation()
	{
		Transform transform = Transform.FromRotation( new Vector3( 2034.2f, 1293.1f, 1247.112389f ) );

		Assert.That( transform.Rotation, Is.EqualTo( new Vector3( 2034.2f, 1293.1f, 1247.112389f ) ) );
	}

	[Test]
	public void ConstructBasisUnitX_1_0_0()
	{
		Transform transform = Transform.Identity;

		Assert.That( transform.BasisUnitX, Is.EqualTo( Vector3.UnitX ) );
	}

	[Test]
	public void ConstructBasisUnitY_0_1_0()
	{
		Transform transform = Transform.Identity;

		Assert.That( transform.BasisUnitY, Is.EqualTo( Vector3.UnitY ) );
	}

	[Test]
	public void ConstructBasisUnitZ_0_0_1()
	{
		Transform transform = Transform.Identity;

		Assert.That( transform.BasisUnitZ, Is.EqualTo( Vector3.UnitZ ) );
	}

	[Test]
	public void WorldMatrix()
	{
		Transform transform = Transform.FromPosition( new Vector3( 2031389.123f, 123247.123f, 57823.123785f ) );

		Assert.That( transform.GetWorldMatrix().Translation, Is.EqualTo( new Vector3( 2031389.123f, 123247.123f, 57823.123785f ) ) );
	}

	[Test]
	public void RotationMatrix()
	{
		Transform transform = Transform.FromRotation( new Vector3( 20, 123, 53 ) );

		Assert.That( transform.GetRotationMatrix(), Is.EqualTo( Matrix4x4.CreateFromYawPitchRoll( 123f, 20f, 53f ) ) );
	}
}
