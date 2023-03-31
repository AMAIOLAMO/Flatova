namespace Flatova.Rendering;

public interface IRenderDevice<in TColor>
{
	void RenderObject( WorldObject renderingObject, Camera camera );
	// void RenderLine3D( Vector3 from, Vector3 to, TColor color, Camera camera );
	// void RenderPixel3D( Vector3 worldPosition, TColor color, Camera camera );
	// void RenderLineFrom3D( Vector3 from, Vector3 relativeOffset, TColor color, Camera camera );

	void ClearDepthBuffer();
}
