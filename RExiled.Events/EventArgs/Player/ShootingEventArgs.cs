using System;
using RExiled.API.Features;
using UnityEngine;

namespace RExiled.Events.EventArgs.Player
{
    public class ShootingEventArgs : System.EventArgs
    {
        public ShootingEventArgs(
            RExiled.API.Features.Player shooter,
            GameObject target,
            ref Vector3 targetPosition)
        {
            Shooter = shooter;
            Target = target;
            TargetPosition = targetPosition;
            IsAllowed = true;
        }

        public RExiled.API.Features.Player Shooter { get; }
        public GameObject Target { get; set; }
        public Vector3 TargetPosition { get; set; }
        public bool IsAllowed { get; set; }
    }
}