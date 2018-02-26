using System;

namespace AlPipe
{
    /// <summary>
    /// Base class for stream processors that convert from one format to another.
    /// </summary>
    /// <typeparam name="TIn">Input data type.</typeparam>
    /// <typeparam name="TOut">Output data type.</typeparam>
    public abstract class StreamConverterBase<TIn, TOut> : StreamProcessorBase<TIn, TOut>
        where TIn : struct
        where TOut : struct
    {
        /// <summary>
        /// Buffer used to store read data from the source stream.
        /// </summary>
        protected TIn[] Buffer;

        protected StreamConverterBase(ISampleStream<TIn> source, int capacity = 0)
            : base(source)
        {
            Buffer = new TIn[capacity];
        }

        protected void EnsureBufferSize(int size)
        {
            if (Buffer.Length < size)
                Buffer = new TIn[size];
        }

        protected virtual TOut Convert(TIn input)
        {
            throw new NotSupportedException();
        }

        public override int Read(TOut[] samples, int offset, int count)
        {
            EnsureBufferSize(count);
            var read = Source.Read(Buffer, 0, count);
            for (var i = 0; i < read; i++)
            {
                var sample = Buffer[i];
                var converted = Convert(sample);
                samples[offset + i] = converted;
            }

            return read;
        }
    }
}
