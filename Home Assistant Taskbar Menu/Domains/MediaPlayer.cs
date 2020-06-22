using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using HADotNet.Core.Models;

namespace Home_Assistant_Taskbar_Menu.Domains
{
    public class MediaPlayer : Domain
    {
        public const string Name = "media_player";

        private static readonly List<string> OffStates = new List<string> {States.Off, States.Unavailable, States.Idle};

        public static Func<string, bool> IsOn = s => !OffStates.Contains(s);

        public static ItemsControl ToItemsControl(StateObject stateObject, string name, Dispatcher dispatcher)
        {
            var root = new MenuItem
            {
                Header = GetName(stateObject, name),
                IsChecked = IsOn(stateObject.State),
                ToolTip = stateObject.EntityId
            };
            root.Click += (sender, args) =>
            {
                HaClientContext.CallService(dispatcher, Name, "toggle", stateObject);
            };

            // var supportedFeatures = stateObject.GetAttributeValue<long>("supported_features");
            // var features = Constants.Domains.MediaPlayer.DecodeSupportedFeatures((int)supportedFeatures);

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

            public static List<int> All = new List<int>
            {
                Pause, Seek, VolumeSet, VolumeMute, PreviousTrack, NextTrack, TurnOn, TurnOff, PlayMedia, VolumeStep,
                SelectSource, Stop, ClearPlaylist, Play, ShuffleSet, SelectSoundMode
            };

            public static List<int> Decode(int supportedFeatures)
            {
                return All.Where(sf => (sf & supportedFeatures) > 0).ToList();
            }
        }
    }
}