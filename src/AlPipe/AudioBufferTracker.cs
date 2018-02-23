using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OalSoft.NET;

namespace AlPipe
{
        /// <inheritdoc />
    public class AudioBufferTracker : IAudioBufferTracker
    {
        private float[] _sampleBuffer;

        private readonly List<TrackingData> _trackedStreams;

        private readonly object _taskLock = new object();
        private Task _updateLoopTask;
        private CancellationTokenSource _cts;

        // caching to reduce allocations
        private uint[] _idBuffer;

        /// <inheritdoc />
        public TimeSpan UpdateDelay { get; }

        /// <summary>
        /// Create a new <see cref="AudioBufferTracker"/> with the specified update rate and initial capacity.
        /// </summary>
        /// <param name="updateDelay">Time between updates, default is 10ms.</param>
        /// <param name="initialCapacity">
        ///   Initial sample buffer capacity, default is 0 (buffer size will grow if necessary).
        /// </param>
        public AudioBufferTracker(TimeSpan updateDelay = default, int initialCapacity = 0)
        {
            UpdateDelay = updateDelay == default ? TimeSpan.FromMilliseconds(10) : updateDelay;
            _sampleBuffer = new float[initialCapacity];
            _trackedStreams = new List<TrackingData>();
            _updateLoopDelegate = UpdateLoop;
            _idBuffer = new uint[0];
        }

        /// <summary>
        /// Start the tracking <see cref="Task"/>.
        /// </summary>
        /// <exception cref="Exception">If already tracking.</exception>
        public void Start()
        {
            lock (_taskLock)
            {
                if (_updateLoopTask != null || (_cts != null && _cts.IsCancellationRequested))
                    throw new Exception("Already running.");
                _cts = new CancellationTokenSource();
                _updateLoopTask = Task.Run(_updateLoopDelegate, _cts.Token);
            }
        }

        /// <summary>
        /// Stops the tracking <see cref="Task"/>.
        /// </summary>
        /// <exception cref="Exception">If not running.</exception>
        public void Stop()
        {
            lock (_taskLock)
            {
                if (_cts == null)
                    throw new Exception("Not running.");
                _cts.Cancel();
            }
        }

        /// <inheritdoc />
        public void Track(IAudioPlayer player, int bufferCount, int bufferSize, int preCache)
        {
            lock (_trackedStreams)
            {
                if (_sampleBuffer.Length < bufferSize)
                    _sampleBuffer = new float[bufferSize];
                // TODO cache buffer arrays!
                var buffers = new uint[bufferCount];
                player.Device.CreateBuffers(buffers, bufferCount);
                var td = new TrackingData(player, buffers, bufferSize);
                _trackedStreams.Add(td);
                for (var i = 0; i < Math.Min(bufferCount, preCache); i++)
                {
                    var done = FillBuffer(td, buffers[i]);
                    player.AlSource.QueueBuffer(buffers[i]);
                    if (done)
                        break;
                }
            }
        }

        /// <inheritdoc />
        public void Untrack(IAudioPlayer player)
        {
            TrackingData td = null;
            lock (_trackedStreams)
            {
                for (var i = 0; i < _trackedStreams.Count; i++)
                {
                    if (_trackedStreams[i].Player == player)
                    {
                        td = _trackedStreams[i];
                        _trackedStreams.RemoveAt(i);
                        break;
                    }
                }
            }
            if (td != null)
                td.Player.Device.DeleteBuffers(td.Buffers, td.BufferCount);
        }

        /// <inheritdoc />
        public void EnsureBuffers(IAudioPlayer player, int n)
        {
            lock (_trackedStreams)
            {
                TrackingData td = null;
                for (var i = 0; i < _trackedStreams.Count; i++)
                {
                    if (_trackedStreams[i].Player == player)
                    {
                        td = _trackedStreams[i];
                        break;
                    }
                }

                if (td == null)
                    throw new Exception("Player is not tracked.");
                var source = td.Player.AlSource;
                var queued = source.QueuedBuffers();
                var toQueue = Math.Min(n, td.BufferCount) - queued;
                for (var i = 0; i < toQueue; i++)
                {
                    var idx = (td.NextBuffer + i) % td.BufferCount;
                    var bufName = td.Buffers[idx];
                    FillBuffer(td, bufName);
                    source.QueueBuffer(bufName);
                }
            }
        }

        private readonly Action _updateLoopDelegate;

        private void UpdateLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (UpdateDelay > TimeSpan.Zero)
                    Task.Delay(UpdateDelay, _cts.Token).Wait();

                Update();
            }
        }

        private void Update()
        {
            lock (_trackedStreams)
            {
                foreach (var td in _trackedStreams)
                {
                    if (_cts.Token.IsCancellationRequested)
                        break;
                    EnsureBuffersFilled(td);
                }
            }
        }

        private void EnsureBuffersFilled(TrackingData td)
        {
            var source = td.Player.AlSource;

            if (source.SourceState == SourceState.Stopped)
                return;

            var queued = source.QueuedBuffers();
            var processed = source.ProcessedBuffers();

            if (processed == 0 && queued >= td.BufferCount)
                return;

            IEnumerable<uint> nextBuffers;
            if (processed > 0)
            {
                if (_idBuffer.Length < processed)
                    _idBuffer = new uint[processed];
                for (var i = 0; i < processed; i++)
                    _idBuffer[i] = source.UnqueueBuffer();
                nextBuffers = _idBuffer.Take(processed);
            }
            else if (queued < td.BufferCount)
            {
                // Note that this can only occur the first update call for a tracked player
                nextBuffers = td.Buffers.Skip(queued);
            }
            else
            {
                nextBuffers = Enumerable.Empty<uint>();
            }

            if (td.PendingFinish)
            {
                if (queued - processed == 0)
                    ((AudioPlayer) td.Player).OnFinish();
                return;
            }

            foreach (var buffer in nextBuffers)
            {
                var done = FillBuffer(td, buffer);
                source.QueueBuffer(buffer);
                td.NextBuffer = (td.NextBuffer + 1) % td.BufferCount;
                if (done)
                    break;
            }
        }

        private bool FillBuffer(TrackingData td, uint buffer)
        {
            var stream = td.Player.Stream;
            var read = stream.Read(_sampleBuffer, 0, td.BufferSize);
            if (td.Player.Loop)
            {
                // loop around and keep reading until the buffer is full
                while (read < td.BufferSize)
                {
                    stream.SamplePosition = 0;
                    read += stream.Read(_sampleBuffer, read, td.BufferSize - read);
                }
            }
            else if (read < td.BufferSize)
            {
                // should finish when leftover buffers are processed
                td.PendingFinish = true;
            }

            var format = stream.Format;
            if (read > 0)
                AlBuffer.BufferData(buffer, (Channels) format.Channels, _sampleBuffer, read, format.SampleRate);

            return td.PendingFinish;
        }

        private class TrackingData
        {
            public IAudioPlayer Player { get; }
            public uint[] Buffers { get; }
            public int BufferSize { get; }
            public bool PendingFinish { get; set; }

            public int NextBuffer { get; set; }

            public int BufferCount => Buffers.Length;

            public TrackingData(IAudioPlayer player, uint[] buffers, int bufferSize)
            {
                Player = player;
                Buffers = buffers;
                BufferSize = bufferSize;
            }
        }

        private static readonly object DefaultLock = new object();
        private static AudioBufferTracker _default;

        public static AudioBufferTracker Default
        {
            get
            {
                lock (DefaultLock)
                {
                    if (_default == null)
                        _default = new AudioBufferTracker();
                }

                return _default;
            }
        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release buffers
        }

        private void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
                _cts?.Cancel();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AudioBufferTracker()
        {
            Dispose(false);
        }
    }
}
