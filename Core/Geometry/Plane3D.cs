using System.Numerics;

namespace Flatova.Geometry;

/// <summary>
///     Representing a plane in 3D space
/// </summary>
public readonly struct Plane3D
{
	public Plane3D( Vector3 position, Vector3 normal )
	{
		Position = position;
		Normal = normal.Normalize();
	}

	/// <summary>
	///     Returns the signed shortest distance from <paramref name="point" /> to this plane
	/// </summary>
	public float GetSignShortDistance( Vector3 point )
	{
		Vector3 normalizedPoint = point.Normalize();

		return normalizedPoint.X * Normal.X +
			   normalizedPoint.Y * Normal.Y +
			   normalizedPoint.Z * Normal.Z -
			   Normal.Dot( Position );
	}

	public bool TryIntersectLine( Vector3 start, Vector3 end, out Vector3 intersectPoint )
	{
		Vector3 lineDirection = ( end - start ).Normalize();

		float planeNormalSimilarityToLineDirection = Normal.Dot( lineDirection );

		if ( MathUtils.AlmostEquals( planeNormalSimilarityToLineDirection, 0f ) )
		{
			intersectPoint = Vector3.Zero;

			return false;
		}

		float scalar = ( Normal.Dot( Position ) - Normal.Dot( start ) ) / planeNormalSimilarityToLineDirection;

		intersectPoint = start + lineDirection * scalar;

		return true;
	}

	/// <summary>
	///     Tries to intersect this line with the given <paramref name="plane" />
	/// </summary>
	public bool TryIntersectLine( Line3D line, out Vector3 intersectPoint ) =>
		TryIntersectLine( line.Start, line.End, out intersectPoint );

	public Vector3 IntersectLine( Vector3 start, Vector3 end )
	{
		Vector3 lineDirection = ( end - start ).Normalize();

		float planeNormalSimilarityToLineDirection = Normal.Dot( lineDirection );

		float scalar = ( Normal.Dot( Position ) - Normal.Dot( start ) ) / planeNormalSimilarityToLineDirection;

		return start + lineDirection * scalar;
	}


	public Vector3 Position { get; }
	public Vector3 Normal   { get; }

	// x * N_x + y * N_x + z * N_x = P * N
}
