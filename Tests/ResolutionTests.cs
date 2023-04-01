namespace Tests;

public class ResolutionTests
{
	[Test]
	public void ConstructWidth_20()
	{
		var resolution = new Resolution( 20, 10 );
		Assert.That( resolution.Width, Is.EqualTo( 20 ) );
	}

	[Test]
	public void ConstructHeight_10()
	{
		var resolution = new Resolution( 20, 10 );
		Assert.That( resolution.Height, Is.EqualTo( 10 ) );
	}

	[Test]
	public void Width_16_HalfWidth_8()
	{
		var resolution = new Resolution( 16, 10 );
		Assert.That( resolution.HalfWidth, Is.EqualTo( 8 ) );
	}

	[Test]
	public void Height_16_HalfHeight_8()
	{
		var resolution = new Resolution( 2, 16 );
		Assert.That( resolution.HalfHeight, Is.EqualTo( 8 ) );
	}

	[Test]
	public void Width_10_Height_15_Contains()
	{
		var resolution = new Resolution( 10, 15 );
		Assert.That( resolution.Contains( 5, 10 ) );
	}

	[Test]
	public void Width_10_Height_15_Negative_Not_Contains()
	{
		var resolution = new Resolution( 10, 15 );
		Assert.That( resolution.Contains( -5, 10 ) is false );
	}

	[Test]
	public void Width_10_Height_15_Positive_Not_Contains()
	{
		var resolution = new Resolution( 10, 15 );
		Assert.That( resolution.Contains( 0, 200 ) is false );
	}

	[Test]
	public void Width_192_Height_96_AspectRatio_2()
	{
		var resolution = new Resolution( 192, 96 );
		Assert.That( resolution.AspectRatio, Is.EqualTo( 2f ) );
	}
}
