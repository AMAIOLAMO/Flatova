using System.Numerics;
using Flatova.Geometry;
using Flatova.World;

namespace Flatova.Rendering;

public interface IRenderDevice<in TColor>
{
	void RenderObject( WorldObject renderingObject, Camera camera );
	void RenderWorldLineSegment3D( Vector3 worldFromPosition, Vector3 worldToPosition, TColor color, Camera camera );
	void RenderWorldPixel( Vector3 worldPosition, TColor color, Camera camera );
	void RenderWorldRect( Vector3 worldCenterPosition, Vector2 size, TColor color, Camera camera );

	void Clear();
	void RenderScene( Scene scene );

	void RenderWorldLineSegment3D( Line3D line, TColor color, Camera camera ) =>
		RenderWorldLineSegment3D( line.Start, line.End, color, camera );
}
