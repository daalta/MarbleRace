using System;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace MarbleRace.Scripts
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RaceManager : UdonSharpBehaviour
    {
        [Header("Race settings")]
        [SerializeField, Tooltip("How much time players have to bet after betting as been started.")]
        private uint bettingTime = 10;

        [Header("References")]
        [SerializeField] private Marble[] marbles;
        [SerializeField] private Spawn spawn;
        [SerializeField] private Finish finish;
        [SerializeField] private BetScreen[] betScreens;
        [SerializeField] private Animator startGameButton;
        

        /// <summary>
        /// Placement of each marble at the end of the race.
        /// Length equals amount of marbles.
        /// Each index corresponds to a marble in the marbles. 
        /// -1 indicates that a marble has not yet finished the race.
        /// </summary>
        [UdonSynced, FieldChangeCallback(nameof(RacePlacement))] private sbyte[] racePlacement;

        [UdonSynced, FieldChangeCallback(nameof(IsGameRunning))] private bool isGameRunning;

        [UdonSynced, FieldChangeCallback(nameof(IsTimeForPayout))]
        private bool isTimeForPayout;

        public bool IsTimeForPayout
        {
            get => isTimeForPayout;
            set
            {
                if (value == isTimeForPayout) return;
                isTimeForPayout = value;
                if (IsTimeForPayout) GivePlayerPayout();
            }
        }
        
        
        /// <summary>
        /// How much money the player has earned. Or lost!
        /// </summary>
        private int money;

        private int Money
        {
            get => money;
            set
            {
                if (value == money) return;
                money = value;
                Debug.Log("Marble Race: Player now has " + money + "$");
                finish._SetMoney(Money);
            }
        }

        private bool IsGameRunning
        {
            get => isGameRunning;
            set
            {
                if (IsGameRunning == value) return;
                isGameRunning = value;
                startGameButton.SetBool("IsGameRunning", IsGameRunning);
            }
        }

        public sbyte[] RacePlacement
        {
            get => racePlacement;
            set
            {
                racePlacement = value;
                OnRacePlacementChanged();
            }
        }

        private void OnRacePlacementChanged()
        {
            var highestPlacement = -1;
            for (sbyte marbleIndex = 0; marbleIndex < RacePlacement.Length; marbleIndex++)
            {
                var placement = RacePlacement[marbleIndex];
                foreach (var betScreen in betScreens)
                {
                    betScreen._SetPlacement(marbleIndex, placement);
                }
                if (placement > highestPlacement) highestPlacement = placement;
            }
        }

        private void Start()
        {
            CheckReferences();
            if (Networking.IsMaster) InitPlacement();
            SetupUI();
            finish.Setup(this);
        }

        /// <summary>
        /// Gets the marble names and colors, and applies them to the in-game racing UI.
        /// </summary>
        private void SetupUI()
        {
            foreach (var betScreen in betScreens)
            {
                betScreen._Setup(this, bettingTime);
                for (var marbleIndex = 0; marbleIndex < marbles.Length; marbleIndex++)
                {
                    var marble = marbles[marbleIndex];
                    betScreen._SetupButton(marbleIndex, marble.name, marble._GetColor());
                }
            }
        }

        private void CheckReferences()
        {
            if (marbles == null || marbles.Length == 0)
                Debug.LogError("Marble Race: Marble array in RaceManager is null");
            if (spawn == null)
                Debug.LogError("Marble Race: Spawn reference in RaceManager is null");
            if (finish == null)
                Debug.LogError("Marble Race: Finish reference in RaceManager is null");
            if (transform.rotation.y != 0)
                Debug.LogWarning("Marble Race: Y rotation of the MarbleRace prefab / RaceManager script should be 0");
        }

        private void InitPlacement()
        {
            RacePlacement = new sbyte[marbles.Length];
            for (var i = 0; i < RacePlacement.Length; i++)
            {
                RacePlacement[i] = (sbyte) -1;
            }
            RequestSerialization();
            OnRacePlacementChanged();
        }

        
        /// <summary>
        /// Start the initial betting before the race begins.
        /// After betting is concluded, the race begins.
        /// </summary>
        [PublicAPI]
        public void StartPreRace()
        {
            if (IsGameRunning)
            {
                Debug.LogWarning("Marble Race: Already running!");
                return;
            }

            if (!Networking.IsMaster)
            {
                SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(StartPreRace));
                return;
            }
            FreezeMarbles(true);
            RespawnMarbles();
            ResetBetScreens();
            betScreens[0]._StartBettingWithoutTimer();
            IsGameRunning = true;
            IsTimeForPayout = false;
            RequestSerialization();
        }

        private void ResetBetScreens()
        {
            foreach (var betScreen in betScreens)
            {
                betScreen.State = 0;
            }
        }

        public void _StartRace()
        {
            // if (isRaceRunning) return; TODO For debugging this is disabled.
            if (!Networking.IsMaster) return;
            FreezeMarbles(false);
            InitPlacement();
        }
        
        private void RespawnMarbles()
        {
            Debug.Log("Respawning marbles");
            for (var i = 0; i < marbles.Length; i++)
            {
                var marble = marbles[i];
                marble._Respawn(spawn._GetMarbleSpawn(i));
                marble._SerializeRigidbodyData();
            }
        }

        private void FreezeMarbles(bool b)
        {
            for (var i = 0; i < marbles.Length; i++)
            {
                var marble = marbles[i];
                marble._SetSimulatePhysics(!b);
            }
        }
        
        private sbyte GetMarbleIndex(Marble marble)
        {
            for (var i = 0; i < marbles.Length; i++)
            {
                if (marbles[i] == marble) return (sbyte) i;
            }
            return 0;
        }

        public void _Finish(Marble marble)
        {
            var marbleIndex = GetMarbleIndex(marble);
            if (RacePlacement[marbleIndex] != -1) return; // Already finished
            
            var placement = GetNextPlacement();
            Debug.Log(marbleIndex + " finished at " + placement);
            RacePlacement[marbleIndex] = placement;

            if (placement >= 2 && IsGameRunning) EndRace();
            
            RequestSerialization();
            OnRacePlacementChanged();
        }

        /// <summary>
        /// Returns the next placement a marble reaching the finish would receive.
        /// </summary>
        /// <returns>Placement of the next marble to reach the finish.</returns>
        private sbyte GetNextPlacement()
        {
            var highestPlacement = -1;
            foreach (var placement in RacePlacement)
            {
                if (highestPlacement < placement) highestPlacement = placement;
            }

            return (sbyte) (highestPlacement + 1);
        }

        private void EndRace()
        {
            Debug.Log("Marble Race: Top 3 have finished, race is over!");
            IsGameRunning = false;
            IsTimeForPayout = true;
            RequestSerialization();
            // TODO End betting if it hasn't already concluded
        }

        public void OnBetPlaced()
        {
            if (betScreens[0].State > 1) return; // First bet screen has already been bet on.
            SendCustomEventDelayedSeconds(nameof(_StartRace), 10);
            if (!Networking.IsMaster) SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(OnBetPlaced));
            else
            {
                betScreens[0]._StartBetting();
            }
        }

        private void GivePlayerPayout()
        {
            var newMoneyTotal = Money;
            foreach (var betScreen in betScreens)
            {
                var payout = betScreen._GetPayout(racePlacement);
                newMoneyTotal += payout;
            }

            Money = newMoneyTotal;
        }
    }
}
