using System;

namespace AlPipe
{
    /// <summary>
    /// Can track <see cref="IAudioPlayer"/> instances to ensure their buffers are filled for streaming.
    /// </summary>
    public interface IAudioBufferTracker : IDisposable
    {
        /// <summary>
        /// Delay between updates.
        /// </summary>
        TimeSpan UpdateDelay { get; }

        /// <summary>
        /// Start tracking a source to make sure the audio data is queued in time for seamless playback.
        /// </summary>
        /// <param name="player">Audio player to track.</param>
        /// <param name="bufferCount">Number of buffers to queue.</param>
        /// <param name="bufferSize">Number of samples in a buffer.</param>
        /// <param name="preCache">Number of buffers to fill synchronously.</param>
        void Track(IAudioPlayer player, int bufferCount, int bufferSize, int preCache);

        /// <summary>
        /// Stop tracking a source.
        /// </summary>
        /// <param name="player">Audio player to stop tracking.</param>
        void Untrack(IAudioPlayer player);

        /// <summary>
        /// Ensure the given tracked player has at least n filled buffers.
        /// Fills and queues buffers synchronously up to n or the number of buffers, whichever is smaller.
        /// </summary>
        /// <param name="player">Player to ensure buffers for.</param>
        /// <param name="n">Number of buffers to ensure.</param>
        void EnsureBuffers(IAudioPlayer player, int n);
    }
}
