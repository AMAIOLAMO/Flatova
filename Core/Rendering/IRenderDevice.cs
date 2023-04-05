using System.Numerics;
using Flatova.World;
using Raylib_cs;

namespace Flatova.Rendering;

public interface IRenderDevice<in TColor>
{
	void RenderObject( WorldObject renderingObject, Camera camera );
	void RenderWorldLine3D( Vector3 worldFromPosition, Vector3 worldToPosition, TColor color, Camera camera );
	void RenderWorldPixel( Vector3 worldPosition, TColor color, Camera camera );
	void RenderWorldCircle( Vector3 worldCenterPosition, float radius, Color color, Camera camera );

	void Clear();
	void RenderScene( Scene scene );
}
