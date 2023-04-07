using System.Numerics;
using Flatova.Geometry;
using Flatova.World;

namespace Flatova.Rendering;

public interface IRenderDevice<in TColor>
{
	void RenderObject( WorldObject renderingObject, Camera camera );
	void RenderWorldLineSegment( Vector3 worldFromPosition, Vector3 worldToPosition, TColor color, Camera camera );
	void RenderWorldPixel( Vector3 worldPosition, TColor color, Camera camera );
	void RenderWorldRect( Vector3 worldCenterPosition, Vector2 size, TColor color, Camera camera );

	void RenderWorldTriangleLine( Triangle3D triangle, TColor color, Camera camera ) =>
		RenderWorldTriangleLine( triangle.First, triangle.Second, triangle.Third, color, camera );

	void RenderWorldTriangleLine( Vector3 a, Vector3 b, Vector3 c, TColor color, Camera camera );

	void Clear();
	void RenderScene( Scene scene );

	void RenderWorldLineSegment( Line3D line, TColor color, Camera camera ) =>
		RenderWorldLineSegment( line.Start, line.End, color, camera );
}
