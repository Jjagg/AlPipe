namespace AlPipe
{
    /// <summary>
    /// State of an <see cref="IAudioPlayer"/>.
    /// </summary>
    public enum PlaybackState
    {
        /// <summary>
        /// The stream is playing.
        /// </summary>
        Playing,
        /// <summary>
        /// The stream is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// The stream is stopped.
        /// </summary>
        Stopped
    }
}
