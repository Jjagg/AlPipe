using System;

namespace AlPipe
{
    /// <summary>
    /// Stream of audio data.
    /// </summary>
    /// <typeparam name="T">Type of the data.</typeparam>
    public interface ISampleStream<T> where T : struct
    {
        /// <summary>
        /// Indicates if this is a static length stream.
        /// </summary>
        /// <remarks>
        ///   A static length stream is typically a decoder, reading from a file.
        ///   Examples of a non-static length stream are a generator or capture device.
        /// </remarks>
        bool IsStatic { get; }
        /// <summary>
        /// Number of <see cref="T"/> in this stream.
        /// <see cref="IsStatic"/> indicates if this is supported. If it is not this returns 0.
        /// </summary>
        long Length { get; }
        /// <summary>
        /// Duration of the entire stream. <see cref="IsStatic"/> indicates if setting is supported.
        /// <see cref="IsStatic"/> indicates if this is supported. If it is not this returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// Get or set the sample position.
        /// <see cref="IsStatic"/> indicates if this is supported. If it is not, getting this returns 0.
        /// </summary>
        long SamplePosition { get; set; }
        /// <summary>
        /// Get or set the time position. <see cref="IsStatic"/> indicates if setting is supported.
        /// <see cref="IsStatic"/> indicates if this is supported. If it is not, getting this returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        TimeSpan TimePosition { get; set; }
        /// <summary>
        /// Format of the source.
        /// </summary>
        Format Format { get; }

        /// <summary>
        /// Read <paramref name="count"/> samples from this stream.
        /// </summary>
        /// <param name="samples">Array to fill with read samples</param>
        /// <param name="offset">Offset in the array to start writing.</param>
        /// <param name="count">Number of samples to read.</param>
        /// <returns>Number of samples actually read. Might be less than <paramref name="count"/>.</returns>
        int Read(T[] samples, int offset, int count);

        /// <summary>
        /// Invoked when samples are read from the stream.
        /// </summary>
        event EventHandler<SamplesEventArgs<T>> SamplesRead;
    }
}
