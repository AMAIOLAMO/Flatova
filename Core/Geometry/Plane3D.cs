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

	// explicit "in" to know that we are modifying the queue
	/// <summary>
	///     clips the given <paramref name="triangles" />
	///     and pass the clipped triangles back into <paramref name="triangles" />
	/// </summary>
	public void ClipTrianglesIntoQueue( in Queue<Triangle3D> triangles )
	{
		// because we know that queue is a FIFO structure
		// we can be sure that if we dequeue the amount of the contents before hand it's okay to use the same queue on the process
		int length = triangles.Count;

		for ( int index = 0; index < length; index++ )
		{
			// dump all last clipped triangles from the buffer
			Triangle3D clippingTriangle = triangles.Dequeue();

			// enqueue the clipped triangles in the front of the working buffer
			TryClipTriangle( clippingTriangle, triangles );
		}
	}

	// TODO: Simplify and optimize this further as this is a performance critical method
	/// <summary>
	///     Tries to clip the given <paramref name="triangleToClip" />,
	///     And Enqueues the clipped triangles into the given queue <paramref name="clippedTriangles" />,
	///     Returns false if the triangle is from the opposite direction of the current plane's <see cref="Normal" />
	/// </summary>
	public bool TryClipTriangle( Triangle3D triangleToClip, in Queue<Triangle3D> clippedTriangles )
	{
		var insidePoints = new Vector3[ 3 ];
		var outsidePoints = new Vector3[ 3 ];

		int insidePointCount = 0, outsidePointCount = 0;

		for ( int i = 0; i < 3; i++ )
		{
			Vector3 triangleVertex = triangleToClip[ i ];

			float signedDistance = GetSignShortDistance( triangleVertex );

			if ( signedDistance >= 0 )
			{
				insidePoints[ insidePointCount ] = triangleVertex;
				++insidePointCount;
			}
			else
			{
				outsidePoints[ outsidePointCount ] = triangleVertex;
				++outsidePointCount;
			}
		}

		// no points are inside the plane to clip, failed to clip the triangle
		if ( insidePointCount == 0 )
			return false;

		// the entire triangle is inside the clipping area, so no need to clip
		if ( insidePointCount == 3 )
		{
			clippedTriangles.Enqueue( triangleToClip );

			return true;
		}

		// clip into a smaller triangle
		if ( insidePointCount == 1 && outsidePointCount == 2 )
		{
			var newTriangle = new Triangle3D
			(
				insidePoints[ 0 ],
				IntersectLine( insidePoints[ 0 ], outsidePoints[ 0 ] ),
				IntersectLine( insidePoints[ 0 ], outsidePoints[ 1 ] )
			);

			clippedTriangles.Enqueue( newTriangle );

			return true;
		}


		if ( insidePointCount == 2 && outsidePointCount == 1 )
		{
			Vector3 newFirstPoint = IntersectLine( insidePoints[ 0 ], outsidePoints[ 0 ] );

			var newFirstTriangle = new Triangle3D
			(
				insidePoints[ 0 ],
				insidePoints[ 1 ],
				newFirstPoint
			);

			Vector3 newSecondPoint = IntersectLine( insidePoints[ 1 ], outsidePoints[ 0 ] );

			var newSecondTriangle = new Triangle3D
			(
				insidePoints[ 1 ],
				newFirstPoint,
				newSecondPoint
			);

			clippedTriangles.Enqueue( newFirstTriangle );
			clippedTriangles.Enqueue( newSecondTriangle );

			return true;
		}

		throw new Exception( "Impossible triangle clipping situation!" );
	}


	public Vector3 Position { get; }
	public Vector3 Normal   { get; }
}
