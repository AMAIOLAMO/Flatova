using System.Numerics;
using Flatova;

namespace Tests;

public class MathUtilsTests
{
	[Test]
	public void Deg180ToRadPi() =>
		Assert.That( 180f * MathUtils.DEG_2_RAD, Is.EqualTo( float.Pi ).Within( 0.0001f ) );

	[Test]
	public void RadPiToDeg180() =>
		Assert.That( float.Pi * MathUtils.RAD_2_DEG, Is.EqualTo( 180f ).Within( 0.0001f ) );

	[Test]
	public void LineSide2D_Line_RightSideOfPoint() =>
		Assert.That( MathUtils.LineSide2D( Vector2.UnitX * -5f, Vector2.One, Vector2.UnitX ), Is.Positive );

	[Test]
	public void LineSide2D_Line_LeftSideOfPoint() =>
		Assert.That( MathUtils.LineSide2D( Vector2.UnitX * 10, Vector2.One, Vector2.UnitX ), Is.Negative );

	[Test]
	public void LineSide2D_Point_On_Line() =>
		Assert.That( MathUtils.LineSide2D( Vector2.One, Vector2.One + Vector2.UnitY, Vector2.One - Vector2.UnitY ), Is.EqualTo( 0 ) );

	[Test]
	public void LineSide2D_3DVector_Line_RightSideOfPoint() =>
		Assert.That( MathUtils.LineSide2D( Vector3.UnitX * -5f, Vector3.One, Vector3.UnitX ), Is.Positive );

	[Test]
	public void LineSide2D_3DVector_Line_LeftSideOfPoint() =>
		Assert.That( MathUtils.LineSide2D( Vector3.UnitX * 10, Vector3.One, Vector3.UnitX ), Is.Negative );

	[Test]
	public void LineSide2D_3DVector_Point_On_Line() =>
		Assert.That( MathUtils.LineSide2D( Vector3.One, Vector3.One + Vector3.UnitY, Vector3.One - Vector3.UnitY ), Is.EqualTo( 0 ) );


	[Test]
	public void LerpClamped_0_eq_5()
	{
		float value = MathUtils.LerpClamped( 5, 200, 0f );
		Assert.That( value, Is.EqualTo( 5f ) );
	}

	[Test]
	public void LerpClamped_1_eq_200()
	{
		float value = MathUtils.LerpClamped( 5, 200, 1f );
		Assert.That( value, Is.EqualTo( 200f ) );
	}

	[Test]
	public void LerpClamped_0_point_5_eq_102_point5()
	{
		float value = MathUtils.LerpClamped( 5, 200, .5f );
		Assert.That( value, Is.EqualTo( 102.5f ) );
	}

	[Test]
	public void LerpClamped_neg_1_eq_5()
	{
		float value = MathUtils.LerpClamped( 5, 200, -1f );
		Assert.That( value, Is.EqualTo( 5f ) );
	}

	[Test]
	public void LerpClamped_20_eq_200()
	{
		float value = MathUtils.LerpClamped( 5, 200, 20f );
		Assert.That( value, Is.EqualTo( 200f ) );
	}

	[Test]
	public void FlattenIndex2D_x_2_y_4_width_6_eq_26()
	{
		int resultFlattenIndex = MathUtils.FlattenIndex2D( 2, 4, 6 );
		Assert.That( resultFlattenIndex, Is.EqualTo( 26 ) );
	}
}
