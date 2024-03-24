using UnityEngine;
using Fusion;
using FusionHelpers;

namespace FusionExamples.Tanknarok
{
	public class GameManager : FusionSession
	{
		public enum PlayState { LOBBY, LEVEL, TRANSITION }

		[SerializeField] private ForceField _forceField;

		[Networked] public PlayState currentPlayState { get; set; }
		[Networked, Capacity(4)] private NetworkArray<int> score => default;

		public Player lastPlayerStanding { get; set; }
		public Player matchWinner
		{
			get
			{
				for (int i = 0; i < score.Length; i++)
				{
					if (score[i] >= MAX_SCORE)
						return GetPlayerByIndex<Player>(i);
				}
				return null;
			}	
		}
		
		public const byte MAX_SCORE = 3;

		private bool _restart;

		public override void Spawned()
		{
			base.Spawned();
			Runner.RegisterSingleton(this);
			
			if (Object.HasStateAuthority)
			{
				LoadLevel(-1);
			}
			else if(currentPlayState!=PlayState.LOBBY)
			{
				Debug.Log("Rejecting Player, game is already running!");
				_restart = true;
			}
		}

		protected override void OnPlayerAvatarAdded(FusionPlayer fusionPlayer)
		{
			Runner.GetLevelManager()?.cameraStrategy.AddTarget(((Player)fusionPlayer).cameraTarget);
		}

		protected override void OnPlayerAvatarRemoved(FusionPlayer fusionPlayer)
		{
			Runner.GetLevelManager()?.cameraStrategy.RemoveTarget(((Player)fusionPlayer).cameraTarget);
		}

		public void OnTankDeath()
		{
			if (currentPlayState != PlayState.LOBBY)
			{
				int playersLeft = 0;
				lastPlayerStanding = null;

				foreach (FusionPlayer fusionPlayer in AllPlayers)
				{
					Player player = (Player) fusionPlayer;
					if (player.isActivated || player.lives > 0)
					{
						lastPlayerStanding = player;
						playersLeft++;
					}
				}

				if (playersLeft > 1)
					lastPlayerStanding = null;
				
				Debug.Log($"Someone died - {playersLeft} left");
        if (lastPlayerStanding != null)
        {
	        int nextLevelIndex = Runner.GetLevelManager().GetRandomLevelIndex();
          int newScore = score[lastPlayerStanding.PlayerIndex] + 1;
          if(HasStateAuthority)
	          score.Set(lastPlayerStanding.PlayerIndex, newScore);
          if (newScore >= MAX_SCORE)
            nextLevelIndex = -1;
          LoadLevel( nextLevelIndex );
        }
			}
		}

		public void Restart(ShutdownReason shutdownReason)
		{
			if (!Runner.IsShutdown)
			{
				// Calling with destroyGameObject false because we do this in the OnShutdown callback on FusionLauncher
				Runner.Shutdown(false,shutdownReason);
				_restart = false;
			}
		}

		public const ShutdownReason ShutdownReason_GameAlreadyRunning = (ShutdownReason)100;

		private void Update()
		{
			for (int i = 0; i < 4; i++)
			{
				_forceField.SetPlayer(i, GetPlayerByIndex<Player>(i));
			}

			LevelManager lm = Runner.GetLevelManager();
			lm.readyUpManager.UpdateUI(currentPlayState, AllPlayers, OnAllPlayersReady);
			
			if (_restart || Input.GetKeyDown(KeyCode.Escape))
			{
				Restart( _restart ? ShutdownReason_GameAlreadyRunning : ShutdownReason.Ok);
				_restart = false;
			}
		}

		private void ResetStats()
		{
			if (!HasStateAuthority)
				return;
			for (int i = 0; i < score.Length; i++)
				score.Set(i,0);
		}

		// Transition from lobby to level
		public void OnAllPlayersReady()
		{
			Debug.Log("All players are ready");

			// close and hide the session from matchmaking / lists. this demo does not allow late join.
      Runner.SessionInfo.IsOpen = false;
      Runner.SessionInfo.IsVisible = false;

      // Reset stats and transition to level.
      ResetStats();
	    LoadLevel(Runner.GetLevelManager().GetRandomLevelIndex());
		}
		
		private void LoadLevel(int nextLevelIndex)
		{
			if (Object.HasStateAuthority)
				Runner.GetLevelManager().LoadLevel(nextLevelIndex);
		}

		public int GetScore(Player player)
		{
			return score[player.PlayerIndex];
		}
	}
}