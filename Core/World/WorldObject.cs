using System.Numerics;
using System.Runtime.CompilerServices;
using Flatova.Geometry;

namespace Flatova.World;

/// <summary>
///     Represents a <see cref="Mesh" /> with a <see cref="Transform" />
/// </summary>
public class WorldObject
{
	public WorldObject( Mesh mesh ) : this( mesh, Transform.Identity ) { }

	public WorldObject( Mesh mesh, Transform transform )
	{
		Mesh = mesh;
		Transform = transform;
	}

	[MethodImpl( MethodImplOptions.AggressiveInlining )]
	public Matrix4x4 GetWorldMatrix() =>
		Transform.GetWorldMatrix();

	public Transform Transform { get; }

	public Mesh Mesh { get; }
}
