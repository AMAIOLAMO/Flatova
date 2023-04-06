using System.Numerics;

namespace Flatova.Geometry;

/// <summary>
///     Representing a line in 3D space
/// </summary>
public readonly struct Line3D
{
	public Line3D( Vector3 start, Vector3 end )
	{
		Start = start;
		End = end;
	}


	public Vector3 Start { get; }
	public Vector3 End   { get; }

	public float Slope
	{
		get
		{
			Vector3 difference = End - Start;

			return difference.Y / difference.X;
		}
	}
	
	public float InverseSlope
	{
		get
		{
			Vector3 difference = End - Start;

			return difference.X / difference.Y;
		}
	}
}
