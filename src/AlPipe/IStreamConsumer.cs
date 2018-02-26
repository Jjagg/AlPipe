namespace AlPipe
{
    /// <summary>
    /// Consumer of audio data.
    /// </summary>
    /// <typeparam name="T">Type of the input data.</typeparam>
    public interface IStreamConsumer<T> where T : struct
    {
        /// <summary>
        /// Stream to consume.
        /// </summary>
        ISampleStream<T> Source { get; }
    }
}
