using System;
using System.Collections.Generic;

namespace AlPipe
{
    /// <summary>
    /// Stream of audio data.
    /// </summary>
    /// <typeparam name="T">Type of the data.</typeparam>
    public interface ISampleStream<T> where T : struct
    {
        /// <summary>
        /// Indicates if this stream supports seeking.
        /// </summary>
        bool CanSeek { get; }
        /// <summary>
        /// Number of <see cref="T"/> in this stream.
        /// </summary>
        long Length { get; }
        /// <summary>
        /// Duration of the entire stream.
        /// </summary>
        TimeSpan Duration { get; }
        /// <summary>
        /// Get or set the sample position. <see cref="CanSeek"/> indicates if setting is supported.
        /// </summary>
        long SamplePosition { get; set; }
        /// <summary>
        /// Get or set the time position. <see cref="CanSeek"/> indicates if setting is supported.
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
        event EventHandler<EventArgs> SamplesRead;
    }

    /// <summary>
    /// Event arguments for the <see cref="ISampleStream{T}.SamplesRead"/> event.
    /// </summary>
    /// <typeparam name="T">Type of the samples.</typeparam>
    public class SamplesReadEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Read sample data.
        /// </summary>
        public readonly IEnumerable<T> Samples;
        /// <summary>
        /// Number of samples.
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Create a new <see cref="SamplesReadEventArgs{T}"/> instance.
        /// </summary>
        /// <param name="samples">Read sample data.</param>
        /// <param name="count">Number of samples.</param>
        public SamplesReadEventArgs(IEnumerable<T> samples, int count)
        {
            Samples = samples;
            Count = count;
        }
    }
}
