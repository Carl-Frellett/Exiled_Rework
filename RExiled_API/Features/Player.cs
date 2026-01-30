using Grenades;
using System;
using UnityEngine;

namespace RExiled.API.Features
{
    public class Player
    {
        private ReferenceHub referenceHub;

        public Player(ReferenceHub referenceHub) => ReferenceHub = referenceHub;
        public Player(GameObject gameObject) => ReferenceHub = ReferenceHub.GetHub(gameObject);

        public ReferenceHub ReferenceHub
        {
            get => referenceHub;
            private set
            {
                if (value == null)
                    throw new NullReferenceException("Player's ReferenceHub cannot be null!");

                referenceHub = value;

                GameObject = value.gameObject;
                Ammo = value.ammoBox;
                Inventory = value.inventory;
                GrenadeManager = value.GetComponent<GrenadeManager>();
            }
        }
        public GameObject GameObject { get; private set; }

        public AmmoBox Ammo { get; private set; }

        public Inventory Inventory { get; private set; }

        public GrenadeManager GrenadeManager { get; private set; }
    }
}
