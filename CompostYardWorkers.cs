using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using Il2Cpp;
using Il2CppInterop.Runtime;

namespace CompostYardMod
{
    public class CompostYardWorkers : MelonMod
    {
        private List<int> compostIds = new List<int>();

        private bool mapLoaded;

        private int firstCheckTimes = 0;

        private float lastCheck;

        private MelonPreferences_Entry<int> maxWorkers;

        public override void OnInitializeMelon()
        {
            base.LoggerInstance.Msg("Loading configuration ...");
            MelonPreferences_Category modCategory = MelonPreferences.CreateCategory("Compost_Yard_Workers");
            maxWorkers = modCategory.CreateEntry("YardMaxWorkers", 2);
            if (maxWorkers.Value > 8)
            {
                maxWorkers.Value = 8;
            }
            else if (maxWorkers.Value < 1)
            {
                maxWorkers.Value = 1;
            }
            base.LoggerInstance.Msg("Max workers for compost yards: " + maxWorkers.Value);
        }

        public override void OnDeinitializeMelon()
        {
            UnloadMod();
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            UnloadMod();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName.ToLower().Equals("map"))
            {
                mapLoaded = true;
                CheckYards();
            }
        }

        public override void OnUpdate()
        {
            if (mapLoaded)
            {
                lastCheck += Time.deltaTime;
                if(firstCheckTimes < 2 && lastCheck > 5f)
                {
                    lastCheck = 0f;
                    CheckYards();
                    firstCheckTimes++;
                }
                if (!(lastCheck < 30f))
                {
                    lastCheck = 0f;
                    CheckYards();
                }
            }
        }

        private void CheckYards()
        {
            //base.LoggerInstance.Msg("Checking for yards...");
            try
            {
                if (!mapLoaded)
                {
                    return;
                }

                Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppArrayBase<CompostYard> list = GameObject.FindObjectsOfType<CompostYard>();
                if (list.Count <= compostIds.Count)
                {
                    return;
                }

                base.LoggerInstance.Msg("New compost yard(s) discovered. Updating ...");
                foreach (CompostYard yard in list)
                {
                    if (!compostIds.Contains(yard.GetInstanceID()))
                    {
                        yard.maxWorkers = maxWorkers.Value;
                        yard.userDefinedMaxWorkers = maxWorkers.Value;
                        compostIds.Add(yard.GetInstanceID());
                        base.LoggerInstance.Msg("Compost yard updated.");
                    }
                }
                base.LoggerInstance.Msg("Total compost yards: " + compostIds.Count);
            }
            catch (Exception ex)
            {
                base.LoggerInstance.Error("Error while checking for new yards: " + ex.Message);
                base.LoggerInstance.Warning("Terminating mod due to error.");
                UnloadMod();
            }
        }

        private void UnloadMod()
        {
            if (mapLoaded)
            {
                mapLoaded = false;
                compostIds.Clear();
                base.LoggerInstance.Msg("Mod unloaded.");
            }
        }
    }
}
