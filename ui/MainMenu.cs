using Godot;

public partial class MainMenu : Control
{
	[ExportCategory("References")]
	[Export] private Button _playButton;
	[Export] private Button _quitButton;
	[Export] private Control _levelPanel;
	[Export] private Container _levelList;

	[ExportCategory("Settings")]
	[Export] private Godot.Collections.Array<PackedScene> _levels = new();
	[Export] private float _slideTime = 0.3f;

	private bool _panelOpen;
	private Tween _tween;

	public override void _Ready()
	{
		base._Ready();

		_playButton.Pressed += ToggleLevelPanel;
		_quitButton.Pressed += () => GetTree().Quit();

		for (var i = 0; i < _levels.Count; i++)
		{
			var level = _levels[i];
			var button = new Button { Text = $"Level {i}" };
			button.Pressed += () => GetTree().ChangeSceneToPacked(level);
			_levelList.AddChild(button);
		}
	}

	private void ToggleLevelPanel()
	{
		_panelOpen = !_panelOpen;
		var width = _levelPanel.Size.X;
		// Panel is anchored to the right edge; offsets slide it in from off-screen.
		var left = _panelOpen ? -width : 0f;

		_tween?.Kill();
		_tween = CreateTween().SetParallel().SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
		_tween.TweenProperty(_levelPanel, "offset_left", left, _slideTime);
		_tween.TweenProperty(_levelPanel, "offset_right", left + width, _slideTime);
	}
}
