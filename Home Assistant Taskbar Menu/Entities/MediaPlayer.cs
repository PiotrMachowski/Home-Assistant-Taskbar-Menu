using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Home_Assistant_Taskbar_Menu.Entities
{
    public class MediaPlayer : MyStateObject
    {
        public const string DomainName = "media_player";
        private static readonly List<string> OffStatesList = new List<string> {States.Off, States.Unavailable};

        public override string Domain()
        {
            return DomainName;
        }

        protected override List<string> OffStates()
        {
            return OffStatesList;
        }

        protected override List<int> AllSupportedFeatures()
        {
            return SupportedFeatures.All;
        }

        protected override Dictionary<int, (string service, string header)> FeatureToServiceMap()
        {
            return SupportedFeatures.ServiceMap;
        }

        public override Control ToMenuItem(Dispatcher dispatcher, string name)
        {
            var root = new MenuItem
            {
                Header = GetName(name),
                IsChecked = IsOn(),
                ToolTip = EntityId
            };
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.TurnOn);
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.TurnOff);
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.Play);
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.Pause);
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.Stop);
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.NextTrack);
            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.PreviousTrack);
            if (IsSupported(SupportedFeatures.VolumeMute))
            {
                root.Items.Add(CreateMenuItem(dispatcher, "volume_mute", "Mute", GetBoolAttribute("is_volume_muted"),
                    data: Tuple.Create<string, object>("is_volume_muted", !GetBoolAttribute("is_volume_muted"))));
            }

            AddSliderIfSupported(dispatcher, root, SupportedFeatures.VolumeSet, 0, 1,
                GetDoubleAttribute("volume_level"), "volume_level", 0.01);
            if (IsSupported(SupportedFeatures.VolumeStep))
            {
                root.Items.Add(CreateMenuItem(dispatcher, "volume_up", "Volume Up"));
                root.Items.Add(CreateMenuItem(dispatcher, "volume_down", "Volume Down"));
            }

            AddSliderIfSupported(dispatcher, root, SupportedFeatures.Seek, 0, GetDoubleAttribute("media_duration"),
                GetDoubleAttribute("media_position"), "seek_position");
            if (IsSupported(SupportedFeatures.SelectSource))
            {
                var currentSource = GetAttribute("source");
                var sources = GetListAttribute("source_list");
                var sourcesRoot = new MenuItem {Header = "Source"};
                sources.ForEach(source => sourcesRoot.Items.Add(CreateMenuItem(dispatcher, "select_source", source,
                    currentSource == source, data: Tuple.Create<string, object>("source", source))));
                root.Items.Add(sourcesRoot);
            }

            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.ClearPlaylist);
            if (IsSupported(SupportedFeatures.ShuffleSet))
            {
                root.Items.Add(CreateMenuItem(dispatcher, "shuffle_set", "Shuffle", GetBoolAttribute("shuffle"),
                    data: Tuple.Create<string, object>("shuffle", !GetBoolAttribute("shuffle"))));
            }

            AddMenuItemIfSupported(dispatcher, root, SupportedFeatures.SelectSoundMode);
            if (IsSupported(SupportedFeatures.SelectSoundMode))
            {
                var currentSoundMode = GetAttribute("sound_mode");
                var soundModes = GetListAttribute("sound_mode_list");
                var soundModesRoot = new MenuItem {Header = "Sound Mode"};
                soundModes.ForEach(soundMode => soundModesRoot.Items.Add(CreateMenuItem(dispatcher, "select_sound_mode",
                    soundMode, currentSoundMode == soundMode,
                    data: Tuple.Create<string, object>("sound_mode", soundMode))));
                root.Items.Add(soundModesRoot);
            }

            return root;
        }

        private static class SupportedFeatures
        {
            public const int Pause = 1;
            public const int Seek = 2;
            public const int VolumeSet = 4;
            public const int VolumeMute = 8;
            public const int PreviousTrack = 16;
            public const int NextTrack = 32;
            public const int TurnOn = 128;
            public const int TurnOff = 256;
            public const int PlayMedia = 512;
            public const int VolumeStep = 1024;
            public const int SelectSource = 2048;
            public const int Stop = 4096;
            public const int ClearPlaylist = 8192;
            public const int Play = 16384;
            public const int ShuffleSet = 32768;
            public const int SelectSoundMode = 65536;

            public static readonly List<int> All = new List<int>
            {
                TurnOn, TurnOff, Play, Pause, Stop, NextTrack, PreviousTrack, VolumeMute, VolumeSet, VolumeStep, Seek,
                SelectSource, ClearPlaylist, ShuffleSet, PlayMedia, SelectSoundMode
            };


            public static readonly Dictionary<int, (string service, string header)> ServiceMap =
                new Dictionary<int, (string service, string header)>
                {
                    {TurnOn, (service: "turn_on", header: "Turn On")},
                    {TurnOff, (service: "turn_off", header: "Turn Off")},
                    {Play, (service: "media_play", header: "Play")},
                    {Pause, (service: "media_pause", header: "Pause")},
                    {Stop, (service: "media_stop", header: "Stop")},
                    {NextTrack, (service: "media_next_track", header: "Next")},
                    {PreviousTrack, (service: "media_previous_track", header: "Previous")},
                    {VolumeMute, (service: "volume_mute", header: "Mute")},
                    {VolumeSet, (service: "volume_set", header: "Volume")},
                    {VolumeStep, (service: "volume_up", header: "Volume Up")},
                    {Seek, (service: "media_seek", header: "Seek")},
                    {SelectSource, (service: "select_source", header: "Source")},
                    {ClearPlaylist, (service: "clear_playlist", header: "Clear Playlist")},
                    {ShuffleSet, (service: "shuffle_set", header: "Shuffle")},
                    {PlayMedia, (service: "play_media", header: "Play Media")},
                    {SelectSoundMode, (service: "select_sound_mode", header: "Sound Mode")}
                };
        }
    }
}