using System;
using OalSoft.NET;

namespace AlPipe
{
    public class AudioPlayer : IAudioPlayer
    {
        private IAudioBufferTracker _tracker;
        private bool _loop;

        /// <inheritdoc />
        public ISampleStream<float> Source { get; private set; }
        /// <inheritdoc />
        public AlStreamingSource AlSource { get; }
        /// <inheritdoc />
        public AlDevice Device { get; }
        /// <inheritdoc />
        public PlaybackState PlaybackState
        {
            get
            {
                var ss = AlSource.SourceState;
                switch (ss)
                {
                    case SourceState.Playing:
                        return PlaybackState.Playing;
                    case SourceState.Paused:
                        return PlaybackState.Paused;
                    case SourceState.Initial:
                    case SourceState.Stopped:
                        return PlaybackState.Stopped;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <inheritdoc />
        public TimeSpan Duration => Source?.Duration ?? TimeSpan.Zero;
        /// <inheritdoc />
        public TimeSpan Position => Source?.TimePosition ?? TimeSpan.Zero;

        /// <inheritdoc />
        public bool Loop
        {
            get => _loop;
            set
            {
                if (value && !Source.IsStatic)
                    throw new NotSupportedException("Can't loop a stream that does not support seeking.");
                _loop = value;
            }
        }

        /// <summary>
        /// Create a new AudioPlayer for the default output device with the specified <see cref="IAudioBufferTracker"/>.
        /// </summary>
        /// <param name="tracker"></param>
        public AudioPlayer(IAudioBufferTracker tracker = null)
            : this(AlDevice.GetDefault().MainContext, tracker)
        {
        }

        public AudioPlayer(AlDevice device, IAudioBufferTracker tracker = null)
            : this(device.MainContext, tracker)
        {
        }

        public AudioPlayer(AlContext ctx, IAudioBufferTracker tracker = null)
        {
            if (ctx == null)
                throw new ArgumentNullException(nameof(ctx));
            _tracker = tracker ?? AudioBufferTracker.Default;
            Device = ctx.Device;
            AlSource = ctx.CreateStreamingSource();
            AlSource.ContextDestroyed += OnSourceContextDestroyed;
        }

        private void OnSourceContextDestroyed(object sender, EventArgs e)
        {
            // TODO handle context destroyed
        }

        /// <inheritdoc />
        public void Load(ISampleStream<float> stream, int bufferCount = 3, int bufferSize = 4096, int preCache = 3)
        {
            Stop();
            _tracker.Untrack(this);

            Source = stream;
            if (!stream.IsStatic)
                Loop = false;

            _tracker.Track(this, bufferCount, bufferSize, preCache);
        }

        /// <inheritdoc />
        public void Play()
        {
            if (Source == null)
                return;
            if (AlSource.SourceState != SourceState.Playing)
            {
                _tracker.EnsureBuffers(this, 1);
                Playing?.Invoke(this, EventArgs.Empty);
                AlSource.Play();
            }
        }

        /// <inheritdoc />
        public void Pause()
        {
            if (Source == null)
                return;
            if (AlSource.SourceState == SourceState.Playing)
            {
                Paused?.Invoke(this, EventArgs.Empty);
                AlSource.Pause();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (Source == null)
                return;
            var ss = AlSource.SourceState;
            if (ss == SourceState.Playing || ss == SourceState.Paused)
            {
                Stopped?.Invoke(this, EventArgs.Empty);
                AlSource.Stop();
            }
        }

        internal void OnFinish()
        {
            Finished?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> Playing;
        /// <inheritdoc />
        public event EventHandler<EventArgs> Paused;
        /// <inheritdoc />
        public event EventHandler<EventArgs> Stopped;
        /// <inheritdoc />
        public event EventHandler<EventArgs> Finished;

        public void Dispose()
        {
            Stop();
            _tracker.Untrack(this);
            AlSource.Dispose();
        }
    }

}
