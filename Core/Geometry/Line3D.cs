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

	/// <summary>
	///     Tries to intersect with the given <paramref name="plane" />
	/// </summary>
	public bool TryIntersect( Plane3D plane, out Vector3 intersectPoint, float epsilon = float.Epsilon )
	{
		Vector3 lineDirection = End - Start;
		float dotProductOfNormalAndDirection = plane.Normal.Dot( lineDirection );

		// Check if the line is parallel
		if ( float.Abs( dotProductOfNormalAndDirection ) > epsilon )
		{
			Vector3 w = Start - plane.Position;
			float factor = -plane.Position.Dot( w );

			Vector3 newU = lineDirection * factor;

			intersectPoint = Start + newU;

			return true;
		}

		intersectPoint = Vector3.Zero;

		return false;
	}

	public Vector3 Start { get; }
	public Vector3 End   { get; }
}
