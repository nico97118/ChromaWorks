// ChromaWorks.cs
// ChromaWorks Plugin for Kerbal Space Program
// This plugin extends ModuleScienceConverter to utilize AI scientists for science conversion.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChromaWorks
{


    public class CW_ModuleScienceConverter : ModuleScienceConverter
    {
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
            return Mathf.Max(totalLevel,0.0f);
        }
    }

    
    public class CW_AIScientists : PartModule
    {
        [KSPField(isPersistant = true)]
        public float level = 1f;

        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "AI Active")]
        public bool isActive = true;

        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "EC/s")]
        public float ecConsumption = 0.5f;

        [KSPEvent(guiActive = true, guiName = "Enable AI", active = true)]
        public void ToggleAI()
        {
            isActive = !isActive;
            UpdateEvents();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            UpdateEvents();
        }

        // Updates the button label and state in the UI
        private void UpdateEvents()
        {
            if (Events == null || !Events.Contains("ToggleAI"))
                return;

            Events["ToggleAI"].guiName = isActive ? "Disable AI" : "Enable AI";
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            if (isActive && HighLogic.LoadedSceneIsFlight)
            {
                double ecNeeded = ecConsumption * TimeWarp.fixedDeltaTime;
                double ecDrawn = part.RequestResource("ElectricCharge", ecNeeded);
                if (ecDrawn < ecNeeded)
                {
                    isActive = false;
                    UpdateEvents();
                    ScreenMessages.PostScreenMessage("AI disabled: not enough ElectricCharge", 3f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
        }

        public override string GetInfo()
        {
            string status = isActive ? "Active" : "Inactive";
            return $"AI Scientist\n" +
                   $"- Level: {level}\n" +
                   $"- Status: {status}\n" +
                   $"- EC Consumption: {ecConsumption} EC/s";
        }
    }
}