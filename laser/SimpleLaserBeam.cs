using Godot;

public partial class SimpleLaserBeam : CsgCylinder3D, ILaserBeam
{
    private ShaderMaterial _material;

    // Scene instances share the sub-resource, so each beam needs its own copy.
    public override void _Ready() => Material = _material = (ShaderMaterial)Material.Duplicate();

    public void SetLaser(Vector3 start, Vector3 end, LaserColor color)
    {
        GlobalTransform = new Transform3D(new Basis(new Quaternion(Vector3.Up, (end - start).Normalized())), (start + end) * .5f);
        Height = start.DistanceTo(end);

        _material.SetShaderParameter("color", color.ToColor());
    }
}

public interface ILaserBeam
{
    void SetLaser(Vector3 start, Vector3 end, LaserColor color);
}