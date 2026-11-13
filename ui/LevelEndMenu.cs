using Godot;

public partial class LevelEndMenu : CanvasLayer
{
	private const string MainMenuPath = "res://ui/main-menu.tscn";

	[ExportCategory("References")]
	[Export] private Label _title;
	[Export] private Button _restartButton;
	[Export] private Button _nextLevelButton;
	[Export] private Button _mainMenuButton;

	private string _nextLevel;

	public override void _Ready()
	{
		base._Ready();
		Hide();

		_restartButton.Pressed += () => GetTree().ReloadCurrentScene();
		_nextLevelButton.Pressed += () => GetTree().ChangeSceneToFile(_nextLevel);
		_mainMenuButton.Pressed += () => GetTree().ChangeSceneToFile(MainMenuPath);
	}

	public void Open(bool won, string nextLevel = null)
	{
		_nextLevel = nextLevel;
		_title.Text = won ? "Level Complete!" : "Level Failed";
		// Next Level only on win, and only if the level has a next one set.
		_nextLevelButton.Visible = won && !string.IsNullOrEmpty(nextLevel);
		Show();
		_restartButton.GrabFocus();
	}
}
