using System;

namespace AlPipe
{
    /// <summary>
    /// Base class for <see cref="IStreamConverter{TIn,TOut}"/> implementations.
    /// </summary>
    /// <typeparam name="TIn">Input data type.</typeparam>
    /// <typeparam name="TOut">Output data type.</typeparam>
    public abstract class StreamConverterBase<TIn, TOut> : IStreamConverter<TIn, TOut>
        where TIn : struct
        where TOut : struct
    {
        /// <inheritdoc />
        public virtual ISampleStream<TIn> Stream { get; }

        /// <inheritdoc />
        public virtual bool CanSeek => Stream.CanSeek;

        /// <inheritdoc />
        public virtual long Length => Stream.Length;

        public TimeSpan Duration => Stream.Duration;

        /// <inheritdoc />
        public virtual long SamplePosition
        {
            get => Stream.SamplePosition;
            set => Stream.SamplePosition = value;
        }

        /// <inheritdoc />
        public virtual TimeSpan TimePosition
        {
            get => Stream.TimePosition;
            set => Stream.TimePosition = value;
        }

        /// <inheritdoc />
        public virtual Format Format => Stream.Format;

        /// <summary>
        /// Create a new <see cref="StreamConverterBase{TIn,TOut}"/>.
        /// </summary>
        /// <param name="source">Source to convert.</param>
        protected StreamConverterBase(ISampleStream<TIn> source)
        {
            Stream = source;
        }

        /// <inheritdoc />
        public abstract int Read(TOut[] samples, int offset, int count);

        /// <inheritdoc />
        public event EventHandler<EventArgs> SamplesRead;
    }
}
