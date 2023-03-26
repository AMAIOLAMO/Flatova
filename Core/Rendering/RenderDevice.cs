using System.Numerics;
using Raylib_cs;

namespace Flatova;

public class RenderDevice
{
	public RenderDevice( Resolution resolution ) =>
		_resolution = resolution;

	public void RenderList( IEnumerable<TransformedMesh> meshes, Camera camera )
	{
		foreach ( TransformedMesh mesh in meshes )
			RenderTransformed( mesh, camera );
	}

	public void RenderTransformed( TransformedMesh renderingObject, Camera camera )
	{
		Matrix4x4 worldMatrix = renderingObject.GetWorldMatrix();

		Matrix4x4 viewMatrix = camera.GetViewMatrix();

		Matrix4x4 resultMatrix = worldMatrix * viewMatrix;

		foreach ( Vector3 vertex in renderingObject.Mesh.Vertices )
		{
			Vector3 viewPoint = Vector3.Transform( vertex, resultMatrix );

			Vector2 projectedPixel = camera.ProjectToScreen( viewPoint, _resolution );

			Raylib.DrawCircle( ( int )projectedPixel.X, ( int )projectedPixel.Y, 5, Color.BLUE );
		}
	}

	readonly Resolution _resolution;
}
