using System;

namespace AlPipe
{
    /// <summary>
    /// A stream that can read from another stream in a loop.
    /// </summary>
    /// <typeparam name="T">Type of the source stream data.</typeparam>
    public class LoopStream<T> : StreamProcessorBase<T, T>
        where T : struct
    {
        /// <summary>
        /// Enable looping.
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// Create a new <see cref="LoopStream{T}"/>. <see cref="Enable"/> initializes to <code>true</code>.
        /// </summary>
        /// <param name="source">The source stream to read from.</param>
        /// <exception cref="ArgumentException">If the give source stream is not seekable.</exception>
        public LoopStream(ISampleStream<T> source) : base(source)
        {
            if (!source.IsStatic)
                throw new ArgumentException("Looped source must be seekable.", nameof(source));

            Enable = true;
        }

        /// <summary>
        /// Read from the source stream. Loop around to the start at the end of the source stream if <see cref="Enable"/>
        /// is set to <code>true</code>.
        /// </summary>
        /// <inheritdoc />
        public override int Read(T[] samples, int offset, int count)
        {
            if (Source.Length == 0)
                return 0;

            var read = Source.Read(samples, offset, count);
            if (Enable)
            {
                while (read < count)
                {
                    Source.SamplePosition = 0;
                    read += Source.Read(samples, read + offset, count - read);
                }
            }

            OnSamplesRead(samples, read);

            return read;
        }
    }
}
