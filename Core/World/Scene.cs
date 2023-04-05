namespace Flatova.World;

/// <summary>
///     Represents a collection of <see cref="WorldObject" /> and a <see cref="Camera" />
/// </summary>
public class Scene
{
	public Scene( Camera camera ) =>
		Camera = camera;

	public void Add( WorldObject worldObject ) =>
		WorldObjects.Add( worldObject );


	public void AddRange( params WorldObject[] worldObjects ) =>
		WorldObjects.AddRange( worldObjects );

	public void AddRange( IEnumerable<WorldObject> worldObjects ) =>
		WorldObjects.AddRange( worldObjects );

	public Camera Camera { get; }

	public List<WorldObject> WorldObjects { get; } = new();
}
