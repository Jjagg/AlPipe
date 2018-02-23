namespace AlPipe
{
    /// <summary>
    /// Interface for audio data converters.
    /// </summary>
    /// <typeparam name="TIn">Input data type.</typeparam>
    /// <typeparam name="TOut">Output data type.</typeparam>
    public interface IStreamConverter<TIn, TOut> : IStreamConsumer<TIn>, ISampleStream<TOut>
        where TIn : struct
        where TOut : struct
    {
    }
}
