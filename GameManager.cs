using Godot;
using System;
using System.Linq;

public partial class GameManager : Node
{
	[ExportCategory("References")]
	[Export] private CameraScript _camera;
	[Export] private Godot.Collections.Array<Lamp> _lamps = new();
	[Export] private Godot.Collections.Array<DestructibleObject> _looseCondition = new();
	
	[ExportCategory("Settings")]
	[Export] private float _lampFocusTime = 2f;
	[Export(PropertyHint.File, "*.tscn")] private string _nextLevel;

	private GameState _currentState = GameState.Playing;
	private LevelEndMenu _endMenu;
	private PauseMenu _pauseMenu;

	public override void _Ready()
	{
		base._Ready();
		_endMenu = GD.Load<PackedScene>("res://ui/level-end-menu.tscn").Instantiate<LevelEndMenu>();
		AddChild(_endMenu);
		_pauseMenu = GD.Load<PackedScene>("res://ui/pause-menu.tscn").Instantiate<PauseMenu>();
		AddChild(_pauseMenu);
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (_currentState != GameState.Playing) return;
		
		// Loose Condition
		if (_looseCondition.Any(obj => obj.IsDestroyed)) ChangeState(GameState.GameOver);
		
		// Win condition
		if (_lamps.All(lamp => lamp.IsActive)) ChangeState(GameState.Win);
	}

	public async void ShowLamps()
	{
		try
		{
			foreach (var lamp in _lamps)
			{
				_camera.SetFocus(lamp);
				await ToSignal(GetTree().CreateTimer(_lampFocusTime), "timeout");
			}
		
			_endMenu.Open(true, _nextLevel);
		}
		catch (Exception e)
		{
			GD.PrintErr(e.Message);
		}
	}
	
	public async void ShowDestroyedObject()
	{
		try
		{
			var destroyedObject = _looseCondition.FirstOrDefault(obj => obj.IsDestroyed);
			if (destroyedObject != null)
			{
				if (destroyedObject.GetCamera() == null)
				{
					GD.PrintErr("Destroyed object has no camera assigned: " + destroyedObject.Name);
				}
				else
				{
					_camera.SetFocus(destroyedObject);
					await ToSignal(GetTree().CreateTimer(_lampFocusTime), "timeout");
				}
			}

			_endMenu.Open(false);
		}
		catch (Exception e)
		{
			GD.PrintErr(e.Message);
		}
	}
	
	
	private void ChangeState(GameState newState)
	{
		_currentState = newState;
		// No pausing once the level has ended; the end menu takes over.
		_pauseMenu.ProcessMode = newState == GameState.Playing ? ProcessModeEnum.Always : ProcessModeEnum.Disabled;
		switch (newState)
		{
			case GameState.Intro:
				break;
			case GameState.Playing:
				break;
			case GameState.Win:
				ShowLamps();
				break;
			case GameState.GameOver:
				ShowDestroyedObject();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}
	}


	private enum GameState
	{
		Intro,
		Playing,
		Win,
		GameOver
	}
}
