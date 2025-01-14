using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace TOR {
    public class SpellCastForceGravityNPC : SpellCastGravity {
        bool originalsSet;
        float originalUpwardsForceLerpOnPlayer;
        float originalPushMaxForce;
        float originalPushForceOnPlayer;

        public override void Throw(Vector3 velocity) {
            if (!originalsSet) {
                originalUpwardsForceLerpOnPlayer = upwardsForceLerpOnPlayer;
                originalPushMaxForce = pushMaxForce;
                originalPushForceOnPlayer = pushForceOnPlayer;
                originalsSet = true;
            }

            upwardsForceLerpOnPlayer = originalUpwardsForceLerpOnPlayer * GlobalSettings.NPCForceGravityMultiplier;
            pushMaxForce = originalPushMaxForce * GlobalSettings.NPCForceGravityMultiplier;
            pushForceOnPlayer = originalPushForceOnPlayer * GlobalSettings.NPCForceGravityMultiplier;
            base.Throw(velocity);
        }
    }
}