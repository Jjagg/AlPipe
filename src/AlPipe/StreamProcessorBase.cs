using System;
using System.Collections.Generic;

namespace AlPipe
{
    /// <summary>
    /// Base class for <see cref="IStreamProcessor{TIn,TOut}"/> implementations.
    /// </summary>
    /// <typeparam name="TIn">Input data type.</typeparam>
    /// <typeparam name="TOut">Output data type.</typeparam>
    public abstract class StreamProcessorBase<TIn, TOut> : IStreamProcessor<TIn, TOut>
        where TIn : struct
        where TOut : struct
    {
        /// <inheritdoc />
        public ISampleStream<TIn> Source { get; }

        /// <inheritdoc />
        public virtual bool IsStatic => Source.IsStatic;

        /// <inheritdoc />
        public virtual long Length => Source.Length;

        public virtual TimeSpan Duration => Source.Duration;

        /// <inheritdoc />
        public virtual long SamplePosition
        {
            get => Source.SamplePosition;
            set => Source.SamplePosition = value;
        }

        /// <inheritdoc />
        public virtual TimeSpan TimePosition
        {
            get => Source.TimePosition;
            set => Source.TimePosition = value;
        }

        /// <inheritdoc />
        public virtual Format Format => Source.Format;

        /// <summary>
        /// Create a new <see cref="StreamProcessorBase{TIn,TOut}"/>.
        /// </summary>
        /// <param name="source">Source to convert.</param>
        protected StreamProcessorBase(ISampleStream<TIn> source)
        {
            Source = source;
        }

        /// <inheritdoc />
        public abstract int Read(TOut[] samples, int offset, int count);

        /// <summary>
        /// Invokes <see cref="SamplesRead"/>.
        /// </summary>
        /// <param name="samples">Read sample data.</param>
        /// <param name="count">Number of samples.</param>
        protected virtual void OnSamplesRead(IEnumerable<TOut> samples, int count)
        {
            if (SamplesRead != null)
            {
                var args = new SamplesEventArgs<TOut>(samples, count);
                SamplesRead?.Invoke(this, args);
            }
        }

        /// <inheritdoc />
        public event EventHandler<SamplesEventArgs<TOut>> SamplesRead;
    }
}
