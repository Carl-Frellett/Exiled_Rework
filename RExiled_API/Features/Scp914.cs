using System.Collections.Generic;

using global::Scp914;

using Mirror;

using UnityEngine;

using Utils.ConfigHandler;

namespace Exiled.API.Features
{
    public static class Scp914
    {
        public static Scp914Knob KnobStatus
        {
            get => Scp914Machine.singleton.NetworkknobState;
            set => Scp914Machine.singleton.NetworkknobState = value;
        }

        public static Dictionary<ItemType, Dictionary<Scp914Knob, ItemType[]>> Recipes
        {
            get => Scp914Machine.singleton.recipesDict;
            set => Scp914Machine.singleton.recipesDict = value;
        }

        public static ConfigEntry<Scp914Mode> ConfigMode
        {
            get => Scp914Machine.singleton.configMode;
            set => Scp914Machine.singleton.configMode = value;
        }

        public static bool IsWorking => Scp914Machine.singleton.working;

        public static Transform IntakeBooth => Scp914Machine.singleton.intake;

        public static Transform OutputBooth => Scp914Machine.singleton.output;

        public static void Start() => Scp914Machine.singleton.RpcActivate(NetworkTime.time);
    }
}
