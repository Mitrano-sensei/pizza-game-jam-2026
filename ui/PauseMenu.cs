using Godot;

public partial class PauseMenu : CanvasLayer
{
	private const string MainMenuPath = "res://ui/main-menu.tscn";

	[ExportCategory("References")]
	[Export] private Button _resumeButton;
	[Export] private Button _resetButton;
	[Export] private Button _levelSelectButton;
	[Export] private Button _mainMenuButton;
	[Export] private Button _quitButton;
	[Export] private Control _levelPanel;
	[Export] private Container _levelList;
	[Export] private HSlider _volumeSlider;

	[ExportCategory("Settings")]
	[Export] private Godot.Collections.Array<PackedScene> _levels = new();
	[Export] private float _slideTime = 0.3f;

	private bool _panelOpen;
	private Tween _tween;

	public override void _Ready()
	{
		base._Ready();
		// Must keep running while the tree is paused to receive input and button presses.
		ProcessMode = ProcessModeEnum.Always;
		Hide();

		_resumeButton.Pressed += () => SetPaused(false);
		_resetButton.Pressed += () => Leave(() => GetTree().ReloadCurrentScene());
		_mainMenuButton.Pressed += () => Leave(() => GetTree().ChangeSceneToFile(MainMenuPath));
		_quitButton.Pressed += () => GetTree().Quit();
		_levelSelectButton.Pressed += ToggleLevelPanel;

		for (var i = 0; i < _levels.Count; i++)
		{
			var level = _levels[i];
			var button = new Button { Text = $"Level {i}" };
			button.Pressed += () => Leave(() => GetTree().ChangeSceneToPacked(level));
			_levelList.AddChild(button);
		}

		// Master bus volume is global, so the slider reflects changes made in earlier levels.
		_volumeSlider.Value = Mathf.DbToLinear(AudioServer.GetBusVolumeDb(0));
		_volumeSlider.ValueChanged += value => AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb((float)value));
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!@event.IsActionPressed("pause")) return;
		SetPaused(!GetTree().Paused);
		GetViewport().SetInputAsHandled();
	}

	private void SetPaused(bool paused)
	{
		GetTree().Paused = paused;
		Visible = paused;
		if (paused) _resumeButton.GrabFocus();
		else if (_panelOpen) ToggleLevelPanel();
	}

	// Same slide-in as MainMenu; the tween runs while paused because this node processes Always.
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

	private void Leave(System.Action changeScene)
	{
		GetTree().Paused = false;
		changeScene();
	}
}
