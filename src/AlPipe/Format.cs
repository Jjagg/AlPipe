namespace AlPipe
{
    /// <summary>
    /// Format of audio data.
    /// </summary>
    public struct Format
    {
        /// <summary>
        /// Number of channels. 1 for mono, 2 for stereo.
        /// </summary>
        public int Channels { get; }
        /// <summary>
        /// Sample rate in Hz.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Create a new <see cref="Format"/> instance.
        /// </summary>
        /// <param name="channels">Number of channels.</param>
        /// <param name="sampleRate">Sample rate in Hz.</param>
        public Format(int channels, int sampleRate)
        {
            Channels = channels;
            SampleRate = sampleRate;
        }
    }
}
