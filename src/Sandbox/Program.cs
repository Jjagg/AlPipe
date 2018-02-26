﻿using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using AlPipe;
using AlPipe.Synth;
using AlPipe.Vorbis;
using CSharpCurses;
using CSharpCurses.Controls;
using OalSoft.NET;

namespace OggPlayer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var filename = args.Length > 0 ? args[0] : null;
            var startPos = args.Length >= 2 ? TimeSpan.FromSeconds(int.Parse(args[1])) : TimeSpan.Zero;

            BufferManager.Logger.SetLogFile("log.txt");

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
                        } else if (input == "sine")
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

            return;


            /*var screen = ConsoleApp.Screen;

            OpenAl.Setup();

            using (_stream = new OggStream(filename))
            {
                _stream.Prepare();

                const int buffersPerSecond = 4;
                var alBufferSize = _stream.Channels * _stream.SampleRate / buffersPerSecond;

                using (_streamer = new OggStreamer(alBufferSize, 2 * buffersPerSecond))
                {
                    // buffer should store 1s of data
                    var ourBufferSize = _stream.SampleRate;

                    _sampleBuffer = new CircularBuffer<float>(ourBufferSize);

                    _streamer.ReadSamples += StoreSamples;

                    CreateUI(Path.GetFileNameWithoutExtension(filename), screen);

                    //var sa = new SampleAggregator(_streamer, _stream, 2048);
                    //sa.PerformFft = true;

                    _stream.Play();
                    _stream.SeekToPosition(startPos);

                    ConsoleApp.Run(TimeSpan.FromMilliseconds(10), Update, KeyDown);

                    _stream.Stop();
                }
            }

            OpenAl.Shutdown();*/
        }

        /*
        private static int _readSampleCount;

        private static void StoreSamples(object sender, ReadSamplesEventArgs e)
        {
            _readSampleCount++;

            _sampleBuffer.Enqueue(e.Buffer, e.Start, e.Length);

            _bufferStartLabel.Text = "Start: " + _sampleBuffer.End;
            _eventCountLabel.Text = "ReadSamples: " + _readSampleCount;
            _debugLabel.Text = "Processed: " + _streamer.ProcessedBuffers;
        }

        private static void UpdateBars()
        {
            var bins = e.Result.Length / _progressBars.Length;
            for (var i = 0; i < _progressBars.Length; i++)
            {
                var sum = 0f;
                for (var j = i; j < i + bins; j++)
                    sum += e.Result[j].X;

                _progressBars[i].Progress = sum;
            }
        }

        #region UI

        private static void Update()
        {
            _timeLabel.Text = "Time: " + _stream.GetPosition().ToString(@"mm\:ss")
                                       + " / " + _stream.GetLength().ToString(@"mm\:ss");
        }

        public static void KeyDown(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Escape || keyInfo.Key == ConsoleKey.Q)
                ConsoleApp.Quit();
        }

        private const int Bars = 10;
        private static BlockProgressBar[] _progressBars;
        private static Label _label;
        private static Label _debugLabel;
        private static Label _metadataLabel;
        private static Label _timeLabel;
        private static Label _bufferStartLabel;
        private static Label _eventCountLabel;

        public static void CreateUI(string filename, Screen screen)
        {
            screen.HasBorder = true;
            _label = new Label(10, 1, filename);
            _debugLabel = new Label(50, 1, string.Empty);
            _metadataLabel = new Label(10, 2, "SampleRate: " + _stream.SampleRate + "; Channels: " + _stream.Channels + "; buffer size: " + _sampleBuffer.Capacity);
            _stream.Ready += (s, e) => _metadataLabel.Text += _stream.SampleRate;
            _timeLabel = new Label(10, 3, "Time: ");
            _bufferStartLabel = new Label(10, 4, "Start: 0");
            _eventCountLabel = new Label(10, 5, "ReadSamples: 0");

            _progressBars = new BlockProgressBar[10];
            var width = BufferManager.Width;

            var step = (width - 20) / Bars;
            var r = new Random();

            for (var i = 0; i < Bars; i++)
            {
                _progressBars[i] = new BlockProgressBar(10 + i * step, 5, 20, true);
                _progressBars[i].Progress = (float) r.NextDouble();
                screen.Add(_progressBars[i]);
            }
            screen.Add(_label);
            screen.Add(_debugLabel);
            screen.Add(_metadataLabel);
            screen.Add(_timeLabel);
            screen.Add(_bufferStartLabel);
            screen.Add(_eventCountLabel);
        }

        #endregion
        */
    }
}
