using MonkeyLoader.Configuration;
using System;

namespace ShowmanTools
{
    internal sealed class ShowMustGoOnConfig : ConfigSection
    {
        private static readonly DefiningConfigKey<bool> _enableStreamingWhileUnfocused = new("EnableStreamingWhileUnfocused", "Keep streaming custom audio to unfocused worlds.", () => true);

        private static readonly DefiningConfigKey<bool> _enableVoiceWhileUnfocused = new("EnableVoiceWhileUnfocused", "Keep streaming your voice to unfocused worlds.", () => false);

        public override string Description => "Contains options for the ShowMustGoOn monkey.";
        public bool EnableStreamingWhileUnfocused => _enableStreamingWhileUnfocused;
        public bool EnableVoiceWhileUnfocused => _enableVoiceWhileUnfocused;
        public override string Id => "ShowMustGoOn";
        public override Version Version { get; } = new Version(1, 0, 0);
    }
}