namespace AlPipe
{
    /// <summary>
    /// Base class for <see cref="IStreamConsumer{T}"/> implementations.
    /// </summary>
    /// <typeparam name="T">Type of the input data.</typeparam>
    public abstract class StreamConsumerBase<T> : IStreamConsumer<T> where T : struct
    {
        /// <inheritdoc />
        public ISampleStream<T> Stream { get; }

        /// <summary>
        /// Create a new SinkBase.
        /// </summary>
        /// <param name="source">Source for the sink.</param>
        protected StreamConsumerBase(ISampleStream<T> source)
        {
            Stream = source;
        }
    }
}
