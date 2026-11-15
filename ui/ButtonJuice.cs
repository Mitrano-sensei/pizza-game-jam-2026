using Godot;

/// <summary>Autoload: gives every Button in the game a hover scale tween, including ones created at runtime.</summary>
public partial class ButtonJuice : Node
{
	private const float HoverScale = 1.08f;
	private const float TweenTime = 0.1f;

	public override void _Ready()
	{
		GetTree().NodeAdded += node =>
		{
			if (node is Button button) Hook(button);
		};
	}

	private static void Hook(Button button)
	{
		// Scale from the center, not the top-left corner.
		button.Resized += () => button.PivotOffset = button.Size / 2;
		button.MouseEntered += () => ScaleTo(button, HoverScale);
		button.MouseExited += () => ScaleTo(button, 1f);
		button.FocusEntered += () => ScaleTo(button, HoverScale);
		button.FocusExited += () => ScaleTo(button, 1f);
	}

	private static void ScaleTo(Button button, float scale)
	{
		// Button's own tween follows its process mode, so it still runs in the pause menu.
		button.CreateTween()
			.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out)
			.TweenProperty(button, "scale", Vector2.One * scale, TweenTime);
	}
}
