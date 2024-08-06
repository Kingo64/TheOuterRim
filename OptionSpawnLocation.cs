using ThunderRoad;

namespace TOR {
    public class OptionSpawnLocation : OptionString {
        public OptionSpawnLocation() {
            this.displayName = "Start Location";
            this.description = "Select which area you would like to start in";
            this.defaultIntValue = 0;
            this.currentIntValue = this.defaultIntValue;
        }
    }
}
