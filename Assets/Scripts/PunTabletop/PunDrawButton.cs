﻿using Carcassonne.Controller;
using Photon.Pun;

namespace PunTabletop
{
    public class PunDrawButton : MonoBehaviourPun
    {
        public GameControllerScript gameScript;
        void Start()
        {

            gameScript.IsPunEnabled = true;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void OnDrawTileHandler()
        {
            photonView.RPC("Pun_RPC_DrawTile", RpcTarget.MasterClient);
        }

        [PunRPC]
        private void Pun_RPC_DrawTile()
        {
            gameScript.PickupTile();

        }

    }
}