using ThunderRoad;

namespace TOR {
    public class ThunderScriptLoadingTips : ThunderScript {

        public string[] loadingTips = new string[]{
            "You can switch out the kyber crystal in a lightsaber by using the Lightsaber Tool on a hilt.",
            "Bacta stims can quickly heal you by jabbing it into your arms, legs, or neck.",
            "Comlinks can be used to quickly summon in Friendly (Blue), Typical (Yellow), Free-for-all (Red) troops from a variety of factions.",
            "Electrobinoculars can zoom in and out by tapping the Alt Use button when held.",
            "Thermal detonators must be switched on by holding Alt Use before they are able to be armed by tapping the same button again once the slider is open.",
            "Thermal detonators will not explode if they are still held by the person who armed it.",
            "Universal pouches may be used to store vast amounts of equipment between levels.",
            "Legend has it that strange artifacts can be discovered on the Outer Rim planets...",
            "Blaster power cells can be used to reload or switch out a blaster's gas. You must press it against the blaster's fill port.",
            "The Z-6 Jetpack contains two additional holsters for carrying gear.",
            // "Some helmets have additional functionality if you press Alt Use whilst holding them, such as flashlights.",
            "The Z-6 Rotary Blaster Cannon must be spun up by holding Alt Use before it is able to fire.",
            "Various lightsabers have dual-phase capabilities that allows the length to be adjusted in the heat of combat.\n\nExample: Darth Vader's lightsaber.",
            "There are a few lightsabers capable of attaching and detaching from one another.\n\nExample: Cal's Lightsaber (Split) and Darth Maul's Lightsaber (Broken).",
            "Double-bladed lightsabers often support switching into a single-blade mode by holding the Alt Use button.",
            "Double-bladed spinning lightsabers can be used as a deadly buzzsaw or a tool for flight depending on which way the lightsaber is held.\n\nExample: Grand Inquisitor's Lightsaber.",
            "Double-bladed collapsible lightsabers can be opened and closed by holding the Alt Use button.\n\nExample: Pong Krell's Lightsaber.",
            "Most blasters have the ability to switch into a Stun mode by holding the Alt Use button whilst holding by the grip.",
            "Automatic blasters can often be switched to burst or single fire by tapping the Alt Use button whilst holding the grip.",
            "Zoom levels on blaster scopes can be adjusted by tapping the Alt Use button whilst holding the scope.",
            "The electro-bayonet on the Amban Rifle may be activated by tapping the Alt Use button whilst holding the foregrip."
        };

        public override void ScriptLoaded(ModManager.ModData modData) {
            base.ScriptLoaded(modData);
            SetupLoadingTips(); 
        }

        public void SetupLoadingTips() {
            if (loadingTips != null) {
                var tips = Catalog.GetTextData()?.textGroups.Find(x => x.id == "LoadingTips");
                var count = tips.texts.Count;
                foreach (var tip in loadingTips) {
                    tips.texts.Add(new TextData.TextID {
                        id = (++count).ToString(),
                        text = tip
                    });
                }
            }
        }
    }
}