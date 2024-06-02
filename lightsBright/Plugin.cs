using System;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Unfoundry;
using C3;
using C3.ModKit;

namespace lightsBright
{
    [UnfoundryMod(Plugin.GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "lightsBright",
            AUTHOR = "casper",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.0.1";

        public static LogSource log;

        public static TypedConfigEntry<float> spotlightRange;
        public static TypedConfigEntry<float> spotlightIntensity;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            new Config(GUID)
                .Group("Spotlight Settings")
                    .Entry(out spotlightRange, "spotlightRange", 100.0f, true,
                        "Spotlight range.",
                        "Adjust the range of the spotlight.")
                    .Entry(out spotlightIntensity, "spotlightIntensity", 5.0f, true,
                        "Spotlight intensity.",
                        "Adjust the intensity of the spotlight.")
                .EndGroup()
                .Load()
                .Save();
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
            var harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(HeadLamp))]
    [HarmonyPatch("setHeadLamp")]
    public static class HeadLampPatch
    {
        static void Postfix(HeadLamp __instance, bool to)
        {
            if (to)
            {
                float newRange = Plugin.spotlightRange.Get();
                float newIntensity = Plugin.spotlightIntensity.Get();

                if (newRange > 0.0f)
                {
                    Plugin.log.Log($"Setting spotlight range to {newRange}");
                    __instance.spotLight.range = newRange;
                }

                if (newIntensity > 0.0f)
                {
                    Plugin.log.Log($"Setting spotlight intensity to {newIntensity}");
                    __instance.spotLight.intensity = newIntensity;
                }
            }
        }
    }
}
