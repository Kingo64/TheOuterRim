using ThunderRoad.Skill.Spell;
using ThunderRoad.Skill;
using ThunderRoad;

namespace TOR {
    public class SkillForceLightningAI : AISkillData {
        public bool applyBurning;
        public float damageIntensity = 2f;
        public float range = 2f;

        public override void OnSpellLoad(SpellData spell, SpellCaster caster = null) {
            base.OnSpellLoad(spell, caster);
            if (spell is SpellCastLightning lightning) {
                lightning.AddModifier(this, Modifier.Intensity, damageIntensity);
                lightning.AddModifier(this, Modifier.Range, range);

                if (applyBurning) {
                    var status = Catalog.GetData<StatusData>("Burning");
                    lightning.AddStatus(this, status, 3);
                }
            }
        }

        public override void OnSpellUnload(SpellData spell, SpellCaster caster = null) {
            base.OnSpellUnload(spell, caster);
            if (spell is SpellCastLightning lightning) {
                lightning.RemoveModifiers(this);
                lightning.RemoveStatus(this);
            }
        }
    }
}
