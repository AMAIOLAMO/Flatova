using System.Numerics;
using Raylib_cs;

namespace Flatova.Rendering;

public interface IRenderDevice
{
	void RenderObject( WorldObject renderingObject, Camera camera );
	void RenderLine3D( Vector3 from, Vector3 to, Color color, Camera camera );
	void RenderPixel3D( Vector3 worldPosition, Color color, Camera camera );
	void RenderLineFrom3D( Vector3 from, Vector3 relativeOffset, Color color, Camera camera );
}
