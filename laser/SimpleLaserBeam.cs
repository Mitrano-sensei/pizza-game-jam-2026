using Godot;

public partial class SimpleLaserBeam : CsgCylinder3D, ILaserBeam
{
    private StandardMaterial3D _material;

    // Scene instances share the sub-resource, so each beam needs its own copy.
    public override void _Ready() => Material = _material = (StandardMaterial3D)Material.Duplicate();

    public void SetLaser(Vector3 start, Vector3 end, LaserColor color)
    {
        GlobalTransform = new Transform3D(new Basis(new Quaternion(Vector3.Up, (end - start).Normalized())), (start + end) * .5f);
        Height = start.DistanceTo(end);

        var c = color switch
        {
            LaserColor.Red => new Color(1, 0, 0),
            LaserColor.Green => new Color(0, 1, 0),
            _ => new Color(0, 0, 1),
        };
        _material.AlbedoColor = new Color(c, 0.63f);
        _material.Emission = c * 1.75f;
    }
}

public interface ILaserBeam
{
    void SetLaser(Vector3 start, Vector3 end, LaserColor color);
}