using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShowmanTools
{
    [HarmonyPatch]
    [HarmonyPatchCategory(nameof(RestrictedCameraAnchors))]
    internal sealed class RestrictedCameraAnchors : ResoniteMonkey<RestrictedCameraAnchors>
    {
        [HarmonyPostfix]
        private static async Task PostfixAsync(Task __result, InteractiveCameraControl __instance)
        {
            await __result;

            if (!Enabled)
                return;

            var lastAnchor = __instance._lastCamera.LocalUserSpace.Children.LastOrDefault(child => child.Name == "Camera Anchor");

            if (lastAnchor is null)
            {
                Logger.Warn(() => $"{nameof(InteractiveCameraControl.OnCreateCameraAnchor)} was triggered, but no slot 'Camera Anchor' was found in the local user space!");
                return;
            }

            lastAnchor.GetComponentInChildren<Grabbable>().OnlyUsers.Add().Target = lastAnchor.LocalUser;
        }

        private static MethodBase TargetMethod()
        {
            var taskMethod = typeof(InteractiveCameraControl).FirstMethod(
                method => method.ReturnType == typeof(Task) && method.Name.Contains(nameof(InteractiveCameraControl.OnCreateCameraAnchor))
                    && method.GetCustomAttribute<CompilerGeneratedAttribute>() is not null);

            return taskMethod;
        }
    }
}