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
        public static TypedConfigEntry<KeyCode> increaseIntensityKey;
        public static TypedConfigEntry<KeyCode> decreaseIntensityKey;
        public static float currentSpotlightIntensity;

        public Plugin()
        {
            log = new LogSource(MODNAME);

            new Config(GUID)
                .Group("Spotlight Settings")
                    .Entry(out spotlightRange, "spotlightRange", 100.0f, false,
                        "Spotlight range.",
                        "Adjust the range of the spotlight.")
                    .Entry(out spotlightIntensity, "spotlightIntensity", 5.0f, false,
                        "Spotlight intensity.",
                        "Adjust the intensity of the spotlight.")
                .EndGroup()
                .Group("Keys",
                    "Key Codes: Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim,",
                    "DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen,",
                    "Asterisk, Plus, Comma, Minus, Period, Slash,",
                    "Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9,",
                    "Colon, Semicolon, Less, Equals, Greater, Question, At,",
                    "LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote,",
                    "A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,",
                    "LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete,",
                    "Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9,",
                    "KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals,",
                    "UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown,",
                    "F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15,",
                    "Numlock, CapsLock, ScrollLock,",
                    "RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightApple, RightApple,",
                    "LeftCommand, LeftCommand, LeftWindows, RightWindows, AltGr,",
                    "Help, Print, SysReq, Break, Menu,",
                    "Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6")
                    .Entry(out increaseIntensityKey, "increaseIntensityKey", KeyCode.KeypadPlus)
                    .Entry(out decreaseIntensityKey, "decreaseIntensityKey", KeyCode.KeypadMinus)
                .EndGroup()
                .Load()
                .Save();

            currentSpotlightIntensity = spotlightIntensity.Get();
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
            var harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            GameObject updaterObject = new GameObject("Updater");
            updaterObject.AddComponent<Updater>();
            UnityEngine.Object.DontDestroyOnLoad(updaterObject);
        }

        public class Updater : MonoBehaviour
        {
            void Update()
            {
                if (Input.GetKeyDown(Plugin.increaseIntensityKey.Get()))
                {
                    IncreaseIntensity();
                }
                if (Input.GetKeyDown(Plugin.decreaseIntensityKey.Get()))
                {
                    DecreaseIntensity();
                }
            }

            private void IncreaseIntensity()
            {
                Plugin.currentSpotlightIntensity += 0.5f; 
                UpdateHeadLampIntensity();
            }

            private void DecreaseIntensity()
            {
                Plugin.currentSpotlightIntensity -= 0.5f; 
                if (Plugin.currentSpotlightIntensity < 0) Plugin.currentSpotlightIntensity = 0; 
                UpdateHeadLampIntensity();
            }

            private void UpdateHeadLampIntensity()
            {
                var headLamp = GameObject.FindObjectOfType<HeadLamp>();
                if (headLamp != null)
                {
                    headLamp.spotLight.intensity = Plugin.currentSpotlightIntensity;
                    Plugin.log.Log($"Updated spotlight intensity to {Plugin.currentSpotlightIntensity}");
                }
            }
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
                float newIntensity = Plugin.currentSpotlightIntensity;

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
