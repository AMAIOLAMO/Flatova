using System.Numerics;

namespace Flatova;

public class Transform
{
	public Transform( Vector3 position, Vector3 rotation ) : this( position, rotation, Vector3.One )
	{
	}

	public Transform( Vector3 position, Vector3 rotation, Vector3 scale )
	{
		Position = position;
		Rotation = rotation;
		Scale = scale;
	}

	/// <summary>
	///     Returns this transform as a world matrix using <see cref="Position" /> and <see cref="Rotation" />
	/// </summary>
	public Matrix4x4 GetWorldMatrix() =>
		GetScaleMatrix() * GetRotationMatrix() * GetTranslationMatrix();

	public Matrix4x4 GetRotationMatrix() =>
		Matrix4X4Utils.FromEulerRadians( Rotation );

	public Matrix4x4 GetScaleMatrix() =>
		Matrix4x4.CreateScale( Scale );

	public Matrix4x4 GetTranslationMatrix() =>
		Matrix4x4.CreateTranslation( Position );


	public static Transform Identity => new( Vector3.Zero, Vector3.Zero, Vector3.One );

	public Vector3 Scale    { get; set; }
	public Vector3 Position { get; set; }
	public Vector3 Rotation { get; set; }

	public Vector3 BasisUnitY => Vector3.UnitY.Transform( GetRotationMatrix() ).Normalize();

	public Vector3 BasisUnitX => Vector3.UnitX.Transform( GetRotationMatrix() ).Normalize();

	public Vector3 BasisUnitZ => Vector3.UnitZ.Transform( GetRotationMatrix() ).Normalize();


	public static Transform FromPosition( Vector3 position ) => new( position, Vector3.Zero );

	public static Transform FromRotation( Vector3 rotation ) => new( Vector3.Zero, rotation );
	public static Transform FromRotation( float x, float y, float z ) => FromRotation( new Vector3( x, y, z ) );
	public static Transform FromScale( Vector3 scale ) => new( Vector3.Zero, Vector3.Zero, scale );

	// TODO: Quaternion Implementation (not done yet)
	// public Transform() : this( Vector3.Zero, Quaternion.Identity )
	// {
	// }
	//
	// public Transform( Vector3 position, Quaternion rotation )
	// {
	// 	Position = position;
	// 	Rotation = rotation;
	// }
	//
	// /// <summary>
	// ///     Returns this transform as a world matrix using <see cref="Position" /> and <see cref="Rotation" />
	// /// </summary>
	// public Matrix4x4 AsWorldMatrix() =>
	// 	Matrix4x4.CreateFromQuaternion( Rotation ) * Matrix4x4.CreateTranslation( Position );
	//
	// public Vector3    Position { get; set; }
	// public Quaternion Rotation { get; set; }
	//
	//
	// public static Transform FromPosition( Vector3 position ) => new( position, Quaternion.Identity );
	//
	// public static Transform FromRotation( Quaternion rotation ) => new( Vector3.Zero, rotation );
	//
	// public static Transform FromEuler( Vector3 eulerAngles ) => new( Vector3.Zero, QuaternionUtils.FromEuler( eulerAngles ) );
}
