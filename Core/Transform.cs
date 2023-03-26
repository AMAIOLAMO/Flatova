using System.Numerics;

public class Transform
{
	public Transform( Vector3 position, Vector3 rotation )
	{
		Position = position;
		Rotation = rotation;
	}

	// Order: Rotation -> Translate
	public Matrix4x4 AsWorldMatrix() =>
		Matrix4x4.CreateFromYawPitchRoll( Rotation.Y, Rotation.X, Rotation.Z ) * Matrix4x4.CreateTranslation( Position );

	public Vector3 Position { get; set; }
	public Vector3 Rotation { get; set; }

	public static Transform FromPosition( Vector3 position ) => new( position, Vector3.Zero );

	public static Transform FromRotation( Vector3 rotation ) => new( Vector3.Zero, rotation );
}
