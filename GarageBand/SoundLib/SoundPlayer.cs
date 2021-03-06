﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SoundLib
{
    public class SoundPlayer : IDisposable
    {
        private const string Dir = @"Resources\";
        private const string HiHat = "HiHat ";
        private const string Kick = "Kick ";
        private const string Snare = "Snare ";
        private const string Extension = ".wav";

        private static Task tHihat;
        private static Task tKick;
        private static Task tSnare;

        private static ManualResetEvent eHihat = new ManualResetEvent(false);
        private static ManualResetEvent eKick = new ManualResetEvent(false);
        private static ManualResetEvent eSnare = new ManualResetEvent(false);

        private static bool isDisposed = false;

        private static volatile string SubTypeHiHat = "1";
        private static volatile string SubTypeKick = "1";
        private static volatile string SubTypeSnare = "1";

        private static volatile float volume = 0.5f;

        public SoundPlayer()
        {
            tHihat = new Task(playHiHat);
            tKick = new Task(playKick);
            tSnare = new Task(playSnare);
            tHihat.Start();
            tKick.Start();
            tSnare.Start();

            Volume = 1.0f; //bug causes sound to randomly play on program start //muted
        }

        public void PlaySound(InstrumentType sound, string SoundType) //calling this method to fast will cause a note not to play
        {
            switch (sound)
            {
                case InstrumentType.Hihat:
                    SubTypeHiHat = SoundType;
                    eHihat.Set();
                    break;
                case InstrumentType.Basskick:
                    SubTypeKick = SoundType;
                    eKick.Set();
                    break;
                case InstrumentType.Snaar:
                    SubTypeSnare = SoundType;
                    eSnare.Set();
                    break;
                default:
                    break;
            }
        }

        private void playHiHat()
        {
            Dictionary<string, MediaPlayer> HiHats = new Dictionary<string, MediaPlayer>();

            for (int i = 1; i < 6; i++)
            {
                MediaPlayer m = new MediaPlayer();
                m.Open(new Uri(Dir + HiHat + i + Extension, UriKind.Relative));
                m.Volume = 0;

                HiHats.Add(i.ToString(), m);
            }

            while (!isDisposed)
            {
                eHihat.WaitOne();
                if (isDisposed)
                {
                    break;
                }
                if (HiHats.TryGetValue(SubTypeHiHat, out MediaPlayer m))
                {
                    checkVolume(m);
                    m.Play();
                    m.Position = new TimeSpan(0);
                }

                eHihat.Reset();
            }
        }
        private void playSnare()
        {
            Dictionary<string, MediaPlayer> snares = new Dictionary<string, MediaPlayer>();

            for (int i = 1; i < 6; i++)
            {
                MediaPlayer m = new MediaPlayer();
                m.Open(new Uri(Dir + Snare + i + Extension, UriKind.Relative));
                m.Volume = 0;

                snares.Add(i.ToString(), m);
            }

            while (!isDisposed)
            {
                eSnare.WaitOne();
                if (isDisposed)
                {
                    break;
                }
                if (snares.TryGetValue(SubTypeSnare, out MediaPlayer m))
                {
                    checkVolume(m);
                    m.Play();
                    m.Position = new TimeSpan(0);
                }
                eSnare.Reset();
            }
        }
        private void playKick()
        {
            Dictionary<string, MediaPlayer> kicks = new Dictionary<string, MediaPlayer>();

            for (int i = 1; i < 6; i++)
            {
                MediaPlayer m = new MediaPlayer();
                m.Open(new Uri(Dir + Kick + i + Extension, UriKind.Relative));
                m.Volume = 0;

                kicks.Add(i.ToString(), m);
            }

            while (!isDisposed)
            {
                eKick.WaitOne();
                if (isDisposed)
                {
                    break;
                }
                if (kicks.TryGetValue(SubTypeSnare, out MediaPlayer m))
                {
                    checkVolume(m);
                    m.Play();
                    m.Position = new TimeSpan(0);
                }
                eKick.Reset();
            }
        }

        public void Dispose()
        {
            isDisposed = true;
            Volume = 0f;

            eHihat.Set();
            eKick.Set();
            eSnare.Set();

            tSnare.Wait();
            tKick.Wait();
            tHihat.Wait();
        }

        public float Volume
        {
            get { return volume; }
            set
            {
                if (value > 1.0f)
                {
                    volume = 1.0f;
                }
                else if (value < 0f)
                {
                    volume = 0f;
                }
                else
                {
                    volume = value;
                }
            }
        }

        private void checkVolume(MediaPlayer m)
        {
            if (m.Volume != Volume)
            {
                m.Volume = volume;
            }
        }
    }
}
