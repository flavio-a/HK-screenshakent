using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using HutongGames.PlayMaker;

namespace Screenshakent
{

    [Serializable]
    public class Multiplier
    {
        public float multiplier = 1f;
    }

    public class Screenshakent : Mod
    {
        internal static Screenshakent Instance;

        public static Multiplier Multiplier = new Multiplier();
        public static string MultiplierPath => Path.Combine(Application.persistentDataPath, "screenShakeModifier.json");

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");

            Instance = this;
            LoadMultiplier();
            EditScreenShake();

            Log("Initialized");
        }

        public static void EditScreenShake()
        {
            LoadMultiplier();
            var fsm = GameCameras.instance.cameraShakeFSM;

            if (Constants.GAME_VERSION == "1.4.3.2" || Constants.GAME_VERSION == "1.3.1.5")
            {
                foreach (var state in fsm.FsmStates)
                {
                    foreach (var action in state.Actions)
                    {
                        var type = action.GetType();
                        if (type.FullName == "HutongGames.PlayMaker.Actions.ShakePosition")
                        {
                            var extentsFieldInfo = type.GetField("extents", BindingFlags.Instance | BindingFlags.Public);
                            var extents = (FsmVector3) extentsFieldInfo.GetValue(action);
                            extentsFieldInfo.SetValue(action, (FsmVector3) (extents.Value * Multiplier.multiplier));
                        }
                    }
                }
            }
        }

        public static void LoadMultiplier()
        {
            try
            {
                if (!File.Exists(MultiplierPath))
                {
                    File.WriteAllText(MultiplierPath, JsonUtility.ToJson(Multiplier, true));
                }

                Multiplier = JsonUtility.FromJson<Multiplier>(File.ReadAllText(MultiplierPath));
                Modding.Logger.Log("Multiplier: " + Multiplier.multiplier.ToString());
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
        }

        public override string GetVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}