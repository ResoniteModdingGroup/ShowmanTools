using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Elements.Assets;
using FrooxEngine;
using HarmonyLib;
using MonkeyLoader.Resonite;

namespace ShowmanTools
{
    [HarmonyPatchCategory(nameof(ShowMustGoOn))]
    [HarmonyPatch(typeof(AudioStreamInterface), nameof(AudioStreamInterface.SetAudioStream))]
    internal sealed class ShowMustGoOn : ConfiguredResoniteMonkey<ShowMustGoOn, ShowMustGoOnConfig>
    {
        private static readonly ConditionalWeakTable<IAudioStream, Component?> _audioStreams = new();

        private static void Postfix(IAudioStream source)
        {
            if (source is not null)
                _audioStreams.Add(source, null);
        }

        [HarmonyPatch]
        private static class UserAudioStreamPatch
        {
            private static bool MuteCheck(Component audioStream)
            {
                var world = audioStream.World;
                var stream = Traverse.Create(audioStream)
                    .Field(nameof(UserAudioStream<MonoSample>.Stream))
                    .GetValue<IAudioStream>();

                return world.Focus == World.WorldFocus.Focused
                    || (world.Focus == World.WorldFocus.Background
                        && (ConfigSection.EnableVoiceWhileUnfocused
                            || (ConfigSection.EnableStreamingWhileUnfocused && _audioStreams.TryGetValue(stream, out _))));
            }

            private static IEnumerable<MethodBase> TargetMethods()
            {
                var genericOptions = new[]
                {
                    typeof(MonoSample),
                    typeof(StereoSample),
                    typeof(QuadSample),
                    typeof(Surround51Sample)
                };

                return genericOptions
                    .Select(type => typeof(UserAudioStream<>)
                        .MakeGenericType(type)
                        .GetMethod(nameof(UserAudioStream<MonoSample>.OnNewAudioData), AccessTools.all));
            }

            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> codeInstructions)
            {
                var getWorldFocusMethod = typeof(World).GetProperty(nameof(World.Focus), AccessTools.all).GetMethod;
                var checkMethod = typeof(UserAudioStreamPatch).GetMethod(nameof(MuteCheck), AccessTools.all);

                var instructions = codeInstructions.ToList();
                var getWorldFocusIndex = instructions.FindIndex(instruction => instruction.Calls(getWorldFocusMethod));

                instructions[getWorldFocusIndex - 1] = new CodeInstruction(OpCodes.Ldarg_0);
                instructions[getWorldFocusIndex] = new CodeInstruction(OpCodes.Call, checkMethod);

                instructions.RemoveAt(getWorldFocusIndex + 1);
                instructions[getWorldFocusIndex + 1].opcode = OpCodes.Brfalse_S;

                return instructions;
            }
        }
    }
}