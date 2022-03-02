using System;
using UdonSharp;
using UnityEngine;

namespace MarbleRace.Scripts
{
    public class Spawn : UdonSharpBehaviour
    {
        [SerializeField] private Transform[] marbleSpawns;

        private void Start()
        {
            if (marbleSpawns.Length == 0)
            {
                Debug.LogError("Marble Race: No spawn points set on Spawn script!");
            }
        }

        public Vector2 _GetMarbleSpawn(int i)
        {
            if (i >= marbleSpawns.Length)
            {
                Debug.LogError($"Marble spawn {i} does not exist, defaulting to 0.");
                return _GetMarbleSpawn(0);
            }

            var spawn = marbleSpawns[i];
            if (spawn == null)
            {
                Debug.LogError($"Marble spawn {i} is null!");
                return Vector2.zero;
            }
            
            return new Vector2(marbleSpawns[i].position.x, marbleSpawns[i].position.y);
        }
    }
}
