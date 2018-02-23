using System;

namespace AlPipe
{
    public class SampleBuffer<T> : IStreamConsumer<T> where T : struct
    {
        // TODO
        public ISampleStream<T> Stream { get; }
        public CircularBuffer<T> Buffer { get; }

        public SampleBuffer(ISampleStream<T> source, TimeSpan bufferSize)
        {
            Stream = source;
            var capacity = (int) (source.Format.SampleRate * bufferSize.TotalSeconds);
            Buffer = new CircularBuffer<T>(capacity);
        }
    }
}
