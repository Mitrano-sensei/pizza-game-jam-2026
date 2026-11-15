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
        ReflectionCamera.Projection = Camera3D.ProjectionType.Frustum;

        var material = (ShaderMaterial)Surface.GetActiveMaterial(0);
        material.SetShaderParameter("reflection_texture", ReflectionViewport.GetTexture());
    }

    public override void _Process(double delta)
    {
        Camera3D mainCamera = GetViewport().GetCamera3D();
        if (mainCamera == null)
            return;

        Vector2 mirrorSize = GetMirrorSize();
        SyncViewportWith(mirrorSize);
        PlaceReflectionCamera(mainCamera, mirrorSize);
    }

    private Vector2 GetMirrorSize()
    {
        Vector2 quadSize = ((QuadMesh)Surface.Mesh).Size;
        Vector3 scale = Surface.GlobalBasis.Scale;
        return new Vector2(quadSize.X * scale.X, quadSize.Y * scale.Y);
    }

    // Viewport aspect must match the mirror quad, since the texture is mapped onto it by mesh UV.
    private void SyncViewportWith(Vector2 mirrorSize)
    {
        int height = (int)GetViewport().GetVisibleRect().Size.Y;
        int width = Mathf.Max(1, Mathf.RoundToInt(height * mirrorSize.X / mirrorSize.Y));
        ReflectionViewport.Size = new Vector2I(width, height);
    }

    // Off-axis camera looking straight through the mirror: its near plane lies exactly on the
    // mirror plane, so nothing behind the mirror leaks into the reflection at oblique angles.
    private void PlaceReflectionCamera(Camera3D mainCamera, Vector2 mirrorSize)
    {
        Basis surfaceBasis = Surface.GlobalBasis.Orthonormalized();
        Vector3 normal = surfaceBasis.Z;
        Vector3 up = surfaceBasis.Y;
        Vector3 mirrorPoint = Surface.GlobalPosition;

        Vector3 camPos = mainCamera.GlobalPosition;
        float distance = normal.Dot(camPos - mirrorPoint);
        if (distance <= 0.01f)
            return; // camera behind the mirror; surface is back-face culled anyway

        Vector3 reflectedPos = camPos - 2f * distance * normal;
        ReflectionCamera.LookAtFromPosition(reflectedPos, reflectedPos + normal, up);

        Vector3 toCenter = mirrorPoint - reflectedPos;
        Basis camBasis = ReflectionCamera.GlobalBasis;
        var offset = new Vector2(toCenter.Dot(camBasis.X), toCenter.Dot(camBasis.Y));

        ReflectionCamera.SetFrustum(mirrorSize.Y, offset, distance, mainCamera.Far);
    }
}
