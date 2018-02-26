using System;
using System.Linq;
using AlPipe;
using AlPipe.Synth;
using AlPipe.Vorbis;
using OalSoft.NET;

namespace ConsolePlayer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var filename = args.Length > 0 ? args[0] : null;
            var startPos = args.Length >= 2 ? TimeSpan.FromSeconds(int.Parse(args[1])) : TimeSpan.Zero;

            Console.WriteLine("Found devices:");
            var devices = AlDevice.GetDevices().ToArray();
            for (var i = 0; i < devices.Length; i++)
                Console.WriteLine($"[{i}] {devices[i]}");

            Console.Write($"Select one (default {AlDevice.DefaultDeviceName}): ");
            var selection = Console.ReadLine();
            Console.WriteLine();

            AudioBufferTracker.Default.Start();

            using (var device = int.TryParse(selection, out var index)
                ? AlDevice.Create(devices[index])
                : AlDevice.GetDefault())
            {
                Console.WriteLine("Opened the device!");

                using (var player = new AudioPlayer(device))
                {
                    var oggDecoder = filename == null ? null : new OggDecoder(filename);
                    var synth = new SynthStream(1000);

                    player.Load(oggDecoder);
                    player.Play();
                    player.Source.TimePosition = startPos;

                    Console.Write("Press Enter to stop.");
                    string input;
                    while (!string.IsNullOrWhiteSpace(input = Console.ReadLine()))
                    {
                        if (int.TryParse(input, out var freq))
                        {
                            if (freq >= 20 && freq <= 20000)
                            {
                                synth.Frequency = freq;
                                player.Load(synth);
                            }
                        }
                        else if (input == "song")
                        {
                            if (oggDecoder != null)
                                player.Load(oggDecoder);
                        }
                        else if (input == "pause")
                            player.Pause();
                        else if (input == "play")
                            player.Play();
                        else if (input.StartsWith("pitch"))
                        {
                            var s = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            if (s.Length >= 2 && float.TryParse(s[1], out var val))
                                player.AlSource.Pitch = val;
                        }
                        else if (input.StartsWith("gain"))
                        {
                            var s = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            if (s.Length >= 2 && float.TryParse(s[1], out var val))
                                player.AlSource.Gain = val;
                        }
                        else if (input.StartsWith("pos"))
                        {
                            if (oggDecoder == null)
                                continue;
                            var s = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                            if (s.Length >= 2 && int.TryParse(s[1], out var val))
                                oggDecoder.TimePosition = TimeSpan.FromSeconds(val);
                        }
                        else if (input == "sine")
                            synth.Oscillator = OscillatorFunctions.Sine;
                        else if (input == "triangle")
                            synth.Oscillator = OscillatorFunctions.Triangle;
                        else if (input == "square")
                            synth.Oscillator = OscillatorFunctions.Square;
                        else if (input == "saw")
                            synth.Oscillator = OscillatorFunctions.SawTooth;
                    }

                    oggDecoder?.Dispose();
                }
            }
        }
    }
}
