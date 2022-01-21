﻿using System.Collections.Generic;
using Carcassonne.Controller;
using Carcassonne.Meeple;
using Carcassonne.State;
using UnityEngine;

namespace Carcassonne.Player
{

    /// <summary>
    /// The Gameplay AI team will likely have to extract a base class from this so that there are Players who do not have
    /// PhotonUsers.
    /// </summary>
    public class PlayerScript : MonoBehaviour
    {
        public int nMeeples = 7;
        
        private int _id;
        private Material mat;
        public List<MeepleScript> meeples => meepleState.MeeplesForPlayer(this);
        public GameObject photonUser => gameObject;
        private Color32 playerColor;
        private string playerName;
        public int score;
        public GameObject ai;
        public bool controlledByAI = false;

        public MeepleState meepleState;

        /// <summary>
        /// Set up the player.
        ///
        /// Within this function, the player creates its own Meeples. I *think* this means each client will instantiate
        /// its own meeples and therefore own its own meeples (in PUN), but I'm not 100% sure of that, so this should be
        /// tested.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="playerMat"></param>
        public void Setup(int id, string name, Material playerMat)
        {
            _id = id;
            playerName = name;
            mat = playerMat;
            mat.name = playerName;

            for (var i = 0; i < nMeeples; i++)
            {
                // Should be a meeple factory method
                var meepleControllerScript = GameObject.Find("GameController").GetComponent<MeepleControllerScript>();
                var meeple = meepleControllerScript.GetNewInstance();
                meeple.player = this;
                meepleState.All.Add(meeple);
            }

            if (controlledByAI)
            {
                GameObject aiObj = Instantiate(ai, transform);
                aiObj.GetComponent<CarcassonneAgent>().wrapper.player = this;
                aiObj.SetActive(true);
            }
        }

        private void Awake()
        {
            score = 0;
            
        }

        public int getID()
        {
            return _id;
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public int AmountOfFreeMeeples()
        {
            return meeples.Count;
        }
    }
}