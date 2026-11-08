using Godot;

namespace pizzagamejam.laser;

public partial class SimpleLaserBeam : CsgCylinder3D, ILaserBeam
{
    public void SetLaser(Vector3 start, Vector3 end)
    {
        GlobalTransform = new Transform3D(new Basis(new Quaternion(Vector3.Up, (end - start).Normalized())), (start + end) * .5f);
        Height = start.DistanceTo(end);
    }
}

public interface ILaserBeam
{
    void SetLaser(Vector3 start, Vector3 end);
}