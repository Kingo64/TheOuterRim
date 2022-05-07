using ThunderRoad;
using System.Collections;

namespace TOR {
    public class LevelModuleFixCreaturePhysics : LevelModule {

        public override IEnumerator OnLoadCoroutine() {
            EventManager.onCreatureSpawn += OnCreatureSpawn;
            yield break;
        }

        public override void OnUnload() {
            base.OnUnload();
            EventManager.onCreatureSpawn -= OnCreatureSpawn;
        }

        void OnCreatureSpawn(Creature creature) {
            SetCreaturePhysics(creature);
        }

        public static void SetCreaturePhysics(Creature creature) {
            if (creature.isPlayer) return;
            creature.ragdoll.physicToggle = (!GlobalSettings.DisableCreaturePhysicsCullingDungeon && Level.current.dungeon) || (!GlobalSettings.DisableCreaturePhysicsCulling && !Level.current.dungeon);
            if (creature.ragdoll.state == Ragdoll.State.NoPhysic) {
                creature.ragdoll.SetState(Ragdoll.State.Standing, false);
            }
        }

        public static void SetAllCreaturePhysics() {
            foreach (var creature in Creature.allActive) {
                SetCreaturePhysics(creature);
            } 
        }
    }
}
