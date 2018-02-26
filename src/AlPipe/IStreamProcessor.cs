namespace AlPipe
{
    /// <summary>
    /// Interface for audio data processors.
    /// </summary>
    /// <typeparam name="TIn">Input data type.</typeparam>
    /// <typeparam name="TOut">Output data type.</typeparam>
    public interface IStreamProcessor<TIn, TOut> : IStreamConsumer<TIn>, ISampleStream<TOut>
        where TIn : struct
        where TOut : struct
    {
    }
}
