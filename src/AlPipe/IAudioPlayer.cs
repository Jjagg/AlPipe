using System;
using OalSoft.NET;

namespace AlPipe
{
    public interface IAudioPlayer : IStreamConsumer<float>, IDisposable
    {
        /// <summary>
        /// Get the OpenAL source.
        /// </summary>
        AlStreamingSource AlSource { get; }
        /// <summary>
        /// Get the OpenAL device this player uses.
        /// </summary>
        AlDevice Device { get; }
        /// <summary>
        /// Get the playback state of the stream.
        /// </summary>
        PlaybackState PlaybackState { get; }

        /// <summary>
        /// Total duration of the stream.
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Get the position in the stream.
        /// </summary>
        TimeSpan Position { get; }

        /// <summary>
        /// Indicates if the player should replay the stream when it is finished.
        /// </summary>
        bool Loop { get; set; }

        /// <summary>
        /// Load a stream for playback.
        /// </summary>
        /// <param name="stream">Stream to load.</param>
        /// <param name="bufferCount">Maximum number of buffers to queue.</param>
        /// <param name="bufferSize">Buffer size in samples</param>
        /// <param name="preCache">Number of buffers to load synchronously. Defaults to 0.</param>
        void Load(ISampleStream<float> stream, int bufferCount = 3, int bufferSize = 4096, int preCache = 0);

        /// <summary>
        /// Start to play back the source.
        /// Ignored if <see cref="PlaybackState"/> is already <see cref="AlPipe.PlaybackState.Playing"/>.
        /// </summary>
        void Play();

        /// <summary>
        /// Pause source playback.
        /// Ignored if <see cref="PlaybackState"/> is already <see cref="AlPipe.PlaybackState.Paused"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///   If <see cref="PlaybackState"/> is <see cref="AlPipe.PlaybackState.Stopped"/>.
        /// </exception>
        void Pause();

        /// <summary>
        /// Stop playing. Ignored if <see cref="PlaybackState"/> is already <see cref="AlPipe.PlaybackState.Stopped"/>.
        /// </summary>
        void Stop();

        /// <summary>
        /// Invoked at the start of <see cref="Play"/>.
        /// </summary>
        event EventHandler<EventArgs> Playing;
        /// <summary>
        /// Invoked at the start of <see cref="Pause"/>.
        /// </summary>
        event EventHandler<EventArgs> Paused;
        /// <summary>
        /// Invoked at the start of <see cref="Stop"/>.
        /// </summary>
        event EventHandler<EventArgs> Stopped;
        /// <summary>
        /// Invoked when the player played all of the audio file.
        /// </summary>
        event EventHandler<EventArgs> Finished;
    }
}
