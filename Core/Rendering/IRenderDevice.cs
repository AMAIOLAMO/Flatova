using System.Numerics;

namespace Flatova.Rendering;

public interface IRenderDevice<in TColor>
{
	void RenderObject( WorldObject renderingObject, Camera camera );
	void RenderWorldLine3D( Vector3 from, Vector3 to, TColor color, Camera camera );

	void Clear();
}
