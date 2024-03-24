using System;
using System.Collections;
using Fusion;
using FusionHelpers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace FusionExamples.Tanknarok
{
	/// <summary>
	/// The LevelManager controls the map - keeps track of spawn points for players.
	/// </summary>
	/// TODO: This is partially left over from previous SDK versions which had a less capable SceneManager, so could probably be simplified quite a bit
	public class LevelManager : NetworkSceneManagerDefault
	{
		[SerializeField] private ScoreManager _scoreManager;
		[FormerlySerializedAs("_readyupManager")] [SerializeField] private ReadyUpManager _readyUpManager;
		[SerializeField] private CountdownManager _countdownManager;
		[SerializeField] private CameraStrategy _cameraStrategy;
		[SerializeField] private CameraScreenFXBehaviour _transitionEffect;
		[SerializeField] private AudioEmitter _audioEmitter;

		[SerializeField] private int _lobby;
		[SerializeField] private int[] _levels;
		
		private LevelBehaviour _currentLevel;
		private SceneRef _loadedScene = SceneRef.None;

		public Action<NetworkRunner,FusionLauncher.ConnectionStatus, string> onStatusUpdate { get; set; }
		public CameraStrategy cameraStrategy => _cameraStrategy;
		public ReadyUpManager readyUpManager => _readyUpManager;

		private void Awake()
		{
			_countdownManager.Reset();
//			_scoreManager.ResetAllGameScores();
		}

		public override void Shutdown()
		{
			Debug.Log("LevelManager.Shutdown();");
			_currentLevel = null;
			if (_loadedScene.IsValid)
			{
				Debug.Log($"LevelManager.UnloadLevel(); - _currentLevel={_currentLevel} _loadedScene={_loadedScene}");
				SceneManager.UnloadSceneAsync(_loadedScene.AsIndex);
				_loadedScene = SceneRef.None;
			}
			_scoreManager.ResetAllGameScores();
			base.Shutdown();
		}

		// Get a random level
		public int GetRandomLevelIndex()
		{
			int idx = Random.Range(0, _levels.Length);
			// Make sure it's not the same level again. This is partially because it's more fun to try different levels and partially because scene handling breaks if trying to load the same scene again.
			if (_levels[idx] == _loadedScene.AsIndex)
				idx = (idx + 1) % _levels.Length;
			return idx;
		}

		public SpawnPoint GetPlayerSpawnPoint(int playerIndex)
		{
			if (_currentLevel!=null)
				return _currentLevel.GetPlayerSpawnPoint(playerIndex);
			return null;
		}

		public void LoadLevel(int nextLevelIndex)
		{
			_currentLevel = null;
			if (_loadedScene.IsValid)
			{
				Debug.Log($"LevelManager.UnloadLevel(); - _currentLevel={_currentLevel} _loadedScene={_loadedScene}");
				UnloadScene(_loadedScene);
				_loadedScene = SceneRef.None;
			}
			Debug.Log($"LevelManager.LoadLevel({nextLevelIndex});");
			if (nextLevelIndex < 0)
			{
				Runner.LoadScene(SceneRef.FromIndex(_lobby), new LoadSceneParameters(LoadSceneMode.Additive));
			}
			else
			{
				Runner.LoadScene(SceneRef.FromIndex(_levels[nextLevelIndex]), new LoadSceneParameters(LoadSceneMode.Additive));
			}
		}

		protected override IEnumerator UnloadSceneCoroutine(SceneRef prevScene)
		{
			Debug.Log($"LevelManager.UnloadSceneCoroutine({prevScene});");

			GameManager gameManager;
			while (!Runner.TryGetSingleton(out gameManager))
			{
				Debug.LogWarning("Waiting for GameManager");
				yield return null;
			}

			if (Runner.IsServer || Runner.IsSharedModeMasterClient)
				gameManager.currentPlayState = GameManager.PlayState.TRANSITION;

			if (prevScene.AsIndex > 0)
			{
				yield return new WaitForSeconds(1.0f);

				InputController.fetchInput = false;

				// Despawn players with a small delay between each one
				Debug.Log("De-spawning all tanks");
				foreach (FusionPlayer fusionPlayer in gameManager.AllPlayers)
				{
					Player player = (Player) fusionPlayer;
					Debug.Log($"De-spawning tank {fusionPlayer.PlayerIndex}:{fusionPlayer}");
					player.TeleportOut();
					yield return new WaitForSeconds(0.1f);
				}

				yield return new WaitForSeconds(1.5f - gameManager.PlayerCount * 0.1f);

				_scoreManager.ResetAllGameScores();
				if (gameManager.lastPlayerStanding != null)
				{
					_scoreManager.ShowIntermediateLevelScore( gameManager );
					yield return new WaitForSeconds(1.5f);
					_scoreManager.ResetAllGameScores();
				}
			}

			yield return base.UnloadSceneCoroutine(prevScene);
		}

		protected override IEnumerator OnSceneLoaded(SceneRef newScene, Scene loadedScene, NetworkLoadSceneParameters sceneFlags)
		{
			Debug.Log($"LevelManager.OnSceneLoaded({newScene},{loadedScene},{sceneFlags});");

			yield return base.OnSceneLoaded(newScene, loadedScene, sceneFlags);

			if (newScene.AsIndex == 0)
				yield break;
			
			_transitionEffect.ToggleGlitch(true);
			_audioEmitter.Play();
			
			onStatusUpdate?.Invoke( Runner, FusionLauncher.ConnectionStatus.Loading, "");

			yield return null;

			_loadedScene = newScene;
			Debug.Log($"Loading scene {newScene}");

			// Delay one frame
			yield return null;

			onStatusUpdate?.Invoke( Runner, FusionLauncher.ConnectionStatus.Loaded, "");
			
			// Activate the next level
			_currentLevel = FindObjectOfType<LevelBehaviour>();
			if(_currentLevel!=null)
				_currentLevel.Activate();
			MusicPlayer.instance.SetLowPassTranstionDirection( newScene.AsIndex>_lobby ? 1f : -1f);

			yield return new WaitForSeconds(0.3f);

			Debug.Log($"Stop glitching");
			_transitionEffect.ToggleGlitch(false);
			_audioEmitter.Stop();

			GameManager gameManager;
			while (!Runner.TryGetSingleton(out gameManager))
			{
				Debug.Log($"Waiting for GameManager to Spawn!");
				yield return null;
			}

			if (gameManager.matchWinner!=null && newScene.AsIndex == _lobby)
			{
				// Show lobby scores and reset the score ui.
				_scoreManager.ShowFinalGameScore(gameManager);
			}

			gameManager.lastPlayerStanding = null;
			
			// Respawn with slight delay between each player
			Debug.Log($"Respawning All {gameManager.PlayerCount} Players");
			foreach (FusionPlayer fusionPlayer in gameManager.AllPlayers)
			{
				Player player = (Player) fusionPlayer;
				Debug.Log($"Initiating Respawn of Player #{fusionPlayer.PlayerIndex} ID:{fusionPlayer.PlayerId}:{player}");
				player.Reset();
				player.Respawn();
				yield return new WaitForSeconds(0.3f);
			}

			// Set state to playing level
			if (_loadedScene.AsIndex == _lobby)
			{
				if(Runner.IsServer || Runner.IsSharedModeMasterClient)
					gameManager.currentPlayState = GameManager.PlayState.LOBBY;
				InputController.fetchInput = true;
//		    Debug.Log($"Switched Scene from {prevScene} to {newScene}");
			}
			else
			{
				StartCoroutine(_countdownManager.Countdown(() =>
				{
					// Set state to playing level
					if (Runner != null && (Runner.IsServer || Runner.IsSharedModeMasterClient))
					{
						gameManager.currentPlayState = GameManager.PlayState.LEVEL;
					}
					// Enable inputs after countdow finishes
					InputController.fetchInput = true;
					//		    Debug.Log($"Switched Scene from {prevScene} to {newScene}");
				}));
			}
		}
	}
}