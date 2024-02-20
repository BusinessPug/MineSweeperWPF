using System;
using System.Collections.Generic;
using System.Media;

namespace MineSweeper
{
    public static class Sounds
    {
        private static readonly string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string soundDirectory = System.IO.Path.Combine(baseDirectory, "Sounds");
        private static readonly Dictionary<string, SoundPlayer> soundPlayers = new Dictionary<string, SoundPlayer>();

        public static void PreloadSounds()
        {
            string[] soundFiles = new string[] { "BlockBreak.wav", "ManyBlockBreak.wav", "Flag.wav", "Explosion.wav" };

            foreach (string fileName in soundFiles)
            {
                string filePath = System.IO.Path.Combine(soundDirectory, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        SoundPlayer player = new SoundPlayer(filePath);
                        player.Load(); // Load the sound into memory
                        soundPlayers.Add(fileName, player);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error loading sound: " + ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Sound file not found: " + filePath);
                }
            }
        }

        public static void PlaySingleBlockBreakSound()
        {
            PlaySound("BlockBreak.wav");
        }

        public static void PlayManyBlockBreakSound()
        {
            PlaySound("ManyBlockBreak.wav");
        }

        public static void PlayFlagSound()
        {
            PlaySound("Flag.wav");
        }

        public static void PlayBombSound()
        {
            PlaySound("Explosion.wav");
        }

        private static void PlaySound(string fileName)
        {
            if (soundPlayers.ContainsKey(fileName))
            {
                try
                {
                    soundPlayers[fileName].Play();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error playing sound: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Sound file not found: " + fileName);
            }
        }
    }
}
