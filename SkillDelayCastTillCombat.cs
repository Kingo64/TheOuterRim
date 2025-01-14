using ThunderRoad.Skill;
using ThunderRoad;
using System.Reflection;
using UnityEngine;

namespace TOR {
    public class SkillDelayCastTillCombat : AISkillData {
        BrainModuleCast moduleCast;
        readonly System.Type brainModuleCast = typeof(BrainModuleCast);

        public override void OnSkillLoaded(SkillData skillData, Creature creature) {
            base.OnSkillLoaded(skillData, creature);
            moduleCast = creature.brain.instance.GetModule<BrainModuleCast>(true);
            moduleCast.castMinMaxDelay.x *= GlobalSettings.CastForcePowerDelayMultiplier;
            moduleCast.castMinMaxDelay.y *= GlobalSettings.CastForcePowerDelayMultiplier;
            brainModuleCast.GetField("lastCastTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(moduleCast, Time.time);

            if (creature.brain.state == Brain.State.Combat) SetNextCastDelay(moduleCast, Random.Range(moduleCast.castMinMaxDelay.x, moduleCast.castMinMaxDelay.y));
            else SetNextCastDelay(moduleCast, float.MaxValue);

            creature.brain.OnStateChangeEvent += OnStateChangeEvent;
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature) {
            base.OnSkillUnloaded(skillData, creature);
            creature.brain.OnStateChangeEvent -= OnStateChangeEvent;
        }

        private void OnStateChangeEvent(Brain.State state) {
            if (state == Brain.State.Combat) {
                SetNextCastDelay(moduleCast, Random.Range(moduleCast.castMinMaxDelay.x, moduleCast.castMinMaxDelay.y));
            }
        }

        private void SetNextCastDelay(BrainModuleCast instance, float delay) {
            if (!GlobalSettings.AllowNPCForcePowers) delay = float.MaxValue;
            brainModuleCast.GetField("nextCastDelay", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, delay);
        }
    }
}
