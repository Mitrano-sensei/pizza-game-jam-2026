using Godot;

public partial class Mirror : Node3D
{
    [Export] public MeshInstance3D Surface { get; set; }
    [Export] public SubViewport ReflectionViewport { get; set; }
    [Export] public Camera3D ReflectionCamera { get; set; }

    [Export(PropertyHint.Layers3DRender)] public uint ReflectionCullMask { get; set; } = 0xFFFFF;

    public override void _Ready()
    {
        ReflectionCamera.CullMask = ReflectionCullMask;

        var material = (ShaderMaterial)Surface.GetActiveMaterial(0);
        material.SetShaderParameter("reflection_texture", ReflectionViewport.GetTexture());
    }

    public override void _Process(double delta)
    {
        Camera3D mainCamera = GetViewport().GetCamera3D();
        if (mainCamera == null)
            return;

        SyncViewportWith(mainCamera);
        PlaceReflectionCamera(mainCamera);
    }

    private void SyncViewportWith(Camera3D mainCamera)
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        ReflectionViewport.Size = new Vector2I((int)screenSize.X, (int)screenSize.Y);
        ReflectionCamera.Fov = mainCamera.Fov;
    }

    private void PlaceReflectionCamera(Camera3D mainCamera)
    {
        Vector3 normal = Surface.GlobalBasis.Z.Normalized();
        Vector3 mirrorPoint = Surface.GlobalPosition;

        Vector3 camPos = mainCamera.GlobalPosition;
        float distance = normal.Dot(camPos - mirrorPoint);

        Vector3 reflectedPos = camPos - 2f * distance * normal;
        Vector3 reflectedForward = ReflectAcross(-mainCamera.GlobalBasis.Z, normal);
        Vector3 reflectedUp = ReflectAcross(mainCamera.GlobalBasis.Y, normal);

        ReflectionCamera.LookAtFromPosition(reflectedPos, reflectedPos + reflectedForward, reflectedUp);

        ReflectionCamera.Near = Mathf.Max(0.05f, Mathf.Abs(distance) - 0.05f);
    }

    private static Vector3 ReflectAcross(Vector3 v, Vector3 n) => v - 2f * v.Dot(n) * n;
}