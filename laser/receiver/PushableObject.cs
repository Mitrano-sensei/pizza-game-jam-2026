using Godot;

public partial class PushableObject : RigidBody3D, ILaserReceiver
{
	public LaserReceiverType OnLaserHit(Vector3 hitPoint, Vector3 incomingDirection, LaserColor color)
	{
		ApplyForce(incomingDirection * 25f, hitPoint - GlobalTransform.Origin);
		
		return LaserReceiverType.StopBouncing;
	}
}