using Godot;

/**
 * A laser receiver that only reflects lasers that hit a specific face of the box shape.
 */
public partial class OneSideReflector : CollisionShape3D, ILaserReceiver
{
	[Export] private Vector3 _reflectiveFace = Vector3.Back;

	public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
	{
		var faceDistance = ((BoxShape3D)Shape).Size.Dot(_reflectiveFace.Abs()) * .5f;
		var onFace = ToLocal(hitPoint).Dot(_reflectiveFace) >= faceDistance - .001f;

		return onFace ? LaserReceiverType.Bounce : LaserReceiverType.StopBouncing;
	}
}
