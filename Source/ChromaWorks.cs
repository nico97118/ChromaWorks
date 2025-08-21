// ChromaWorks.cs
// ChromaWorks Plugin for Kerbal Space Program
// This plugin extends ModuleScienceConverter to utilize AI scientists for science conversion.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;

namespace ChromaWorks
{


    public class CW_ModuleScienceConverter : ModuleScienceConverter
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "AI Bonus")]
        public string scientistBonusDisplay = "0";

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            UpdateScientistBonus();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            UpdateScientistBonus();
        }


        /// Updates the scientist bonus display in the GUI.
        /// This method calculates the total bonus from all active AI scientists connected to the same parent part
        /// and updates the scientistBonusDisplay field.
        private void UpdateScientistBonus()
        {
            try
            {
                if (part == null || part.vessel == null) return;

                float bonus = GetScientists();
                scientistBonusDisplay = $"{bonus:F2}";
            }
            catch (Exception e)
            {
                Debug.LogError($"[ChromaWorks] Error updating scientistBonusDisplay: {e}");
                scientistBonusDisplay = "Error";
            }
        }

        // Returns the sum of all ACTIVE AI scientist levels connected to the same parent part as this converter
        protected override float GetScientists()
        {
            float totalLevel = 0f;

            if (this.part != null && this.part.vessel != null)
            {
                Part CW_core = this.part.parent;
                var chips = this.part.vessel.Parts.Where(p => p.parent == CW_core);

                foreach (Part p in chips)
                {
                    foreach (PartModule m in p.Modules)
                    {
                        if (m is CW_AIScientists AIScientist && AIScientist.isActive)
                        {
                            totalLevel += 1.0f + AIScientist.level * scientistBonus;
                        }
                    }
                }
            }
            return Mathf.Max(totalLevel, 0.0f);
        }
    }


    public class CW_AIScientists : PartModule
    {
        [KSPField(isPersistant = true)]
        public float level = 1f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "AI Active")]
        public bool isActive = true;

        // Configured base EC/s, visible in editor only, user-configurable
        [KSPField(isPersistant = true, guiActiveEditor = false, guiActive = false, guiName = "EC/s Base Rate")]
        public float ecBase = 0.5f;

        // Dynamic EC/s, visible in flight and editor for DBS, not user-configurable
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "EC/s Usage")]
        public float ecConsumption = 0f;

        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Enable AI", active = true)]
        public void ToggleAI()
        {
            isActive = !isActive;
            UpdateEvents();
            UpdateEcConsumption();

            if (HighLogic.LoadedSceneIsEditor)
                ScreenMessages.PostScreenMessage(isActive ? "AI Activated" : "AI Deactivated", 2f, ScreenMessageStyle.UPPER_CENTER);
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            UpdateEvents();
            UpdateEcConsumption();
        }

        // Updates the button label and state in the UI
        private void UpdateEvents()
        {
            if (Events == null || !Events.Contains("ToggleAI"))
                return;

            Events["ToggleAI"].guiName = isActive ? "Disable AI" : "Enable AI";
            Events["ToggleAI"].active = true;
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (isActive && HighLogic.LoadedSceneIsFlight)
            {
                double ecNeeded = ecBase * TimeWarp.fixedDeltaTime;
                double ecDrawn = part.RequestResource("ElectricCharge", ecNeeded);
                if (ecDrawn < ecNeeded)
                {
                    isActive = false;
                    UpdateEvents();
                    ScreenMessages.PostScreenMessage("AI disabled: not enough ElectricCharge", 3f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            UpdateEcConsumption();
        }

        public override string GetInfo()
        {
            string status = isActive ? "Active" : "Inactive";
            return $"AI Scientist\n" +
                   $"- Level: {level}\n" +
                   $"- Status: {status}\n" +
                   $"-  EC/s Usage: {ecBase}";
        }
        private void UpdateEcConsumption()
        {
            ecConsumption = isActive ? ecBase : 0f;
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class ChromaWorksCategory : MonoBehaviour
    {
        void Start()
        {
            var filter = PartCategorizer.Instance.filters.Find(f => f.button.categoryName == "Filter by Function");
            if (filter != null)
            {
                PartCategorizer.AddCustomSubcategoryFilter(
                    filter,
                    "ChromaWorks",
                    "ChromaWorks",
                    PartCategorizer.Instance.iconLoader.GetIcon("ChromaWorksIcon"),
                    p => p.partPrefab != null &&
                         !string.IsNullOrEmpty(p.partPrefab.partInfo?.tags) &&
                         p.partPrefab.partInfo.tags.Contains("chroma"));
            }
        }
    }
}