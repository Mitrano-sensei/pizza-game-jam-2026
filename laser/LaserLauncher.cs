using Godot;
using System.Collections.Generic;
using System.Linq;
using pizzagamejam.laser;

public partial class LaserLauncher : Node3D
{
    [ExportCategory("Laser Settings")]
    [Export] private Node3D _laserOrigin;
    [Export] private float _maxDistance = 100f;
    [Export] private int _maxBounces = 3;
    [Export(PropertyHint.Layers3DPhysics)] private uint _collisionMask = 1;

    [ExportCategory("Object Pool Settings")]
    [Export] private ObjectPool _laserPointPool;

    private List<Vector3> _points = new();
    private List<Vector3> _lastPoints = new();
    private List<ILaserBeam> _currentLasers = new();

    public override void _PhysicsProcess(double delta)
    {
        ComputePoints();
        if (!ComparePoints(_points, _lastPoints))
        {
            _lastPoints = _points;
            Draw();
        }
    }

    private void Draw()
    {
        if (_points.Count < 2) return;

        foreach (var laser in _currentLasers)
        {
            _laserPointPool.Release((Node3D)laser);
        }

        _currentLasers.Clear();

        for (int i = 0; i < _points.Count - 1; i++)
        {
            var start = _points[i];
            var end = _points[i + 1];

            var laser = (ILaserBeam)_laserPointPool.Get();
            laser.SetLaser(start, end);
            _currentLasers.Add(laser);
        }
    }

    private void ComputePoints()
    {
        var space = GetWorld3D().DirectSpaceState;

        Vector3 direction = -_laserOrigin.GlobalTransform.Basis.Z;
        Vector3 origin = _laserOrigin.GlobalPosition;
        var remaining = _maxDistance;
        _points = [origin];

        for (int i = 0; i < _maxBounces; i++)
        {
            var query = PhysicsRayQueryParameters3D.Create(origin, origin + direction * remaining, _collisionMask);
            var hit = space.IntersectRay(query);

            if (hit.Count == 0)
            {
                _points.Add(origin + direction * remaining);
                break;
            }

            var pos = (Vector3)hit["position"];
            var normal = (Vector3)hit["normal"];
            var collider = hit["collider"].As<Node3D>();
            // GD.Print("HIT " + collider.Name );

            _points.Add(pos);
            remaining -= origin.DistanceTo(pos);

            var colliderChildren = collider.FindChildren("*", "", true, false);
            var reflective = collider as ILaserReflective ?? colliderChildren.OfType<ILaserReflective>().FirstOrDefault();
            var receiver = collider as ILaserReceiver ?? colliderChildren.OfType<ILaserReceiver>().FirstOrDefault();
            var receiverType = receiver?.OnLaserHit(pos, direction) ?? LaserReceiverType.Continue;

            // Make the Beam stop
            if (receiverType == LaserReceiverType.StopBouncing) break; // Stop bouncing if the receiver indicates to do so
            if (receiver == null && reflective == null) break; // Stop bouncing if the object is neither a receiver that bounces nor reflective

            // Make the beam Bounce
            if (receiverType == LaserReceiverType.Bounce || reflective != null)
            {
                direction = direction.Bounce(normal).Normalized();
                origin = pos + normal * .001f; // Offset to avoid self-collision
                continue;
            }

            // Continue the beam
            origin = pos + direction * .001f; // Offset to avoid self-collision
            i--; // Because it didn't bounce
        }
    }

    private bool ComparePoints(List<Vector3> a, List<Vector3> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (!a[i].IsEqualApprox(b[i])) return false;
        }

        return true;
    }
}