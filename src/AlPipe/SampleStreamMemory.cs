using System;

namespace AlPipe
{
    /// <summary>
    /// Listens for a <see cref="ISampleStream{T}"/> being read and stores the read data in a <see cref="CircularBuffer{T}"/>.
    /// </summary>
    /// <typeparam name="T">Type of data provided by the <see cref="Stream"/>.</typeparam>
    public class SampleStreamMemory<T> where T : struct
    {
        public CircularBuffer<T> Buffer { get; }
        public ISampleStream<T> Stream { get; private set; }

        /// <summary>
        /// Create a <see cref="SampleStreamMemory{T}"/> instance.
        /// </summary>
        /// <param name="stream">Stream to listen to.</param>
        /// <param name="capacity">Capacity of the <see cref="Buffer"/>.</param>
        public SampleStreamMemory(ISampleStream<T> stream, int capacity)
        {
            Buffer = new CircularBuffer<T>(capacity);
            Stream = stream;
            stream.SamplesRead += OnSamplesRead;
        }

        private void OnSamplesRead(object sender, SamplesEventArgs<T> e)
        {
            Buffer.Enqueue(e.Samples, e.Count);
            ReceivedSamples?.Invoke(this, e);
        }

        /// <summary>
        /// Invoked after new samples are received and stored.
        /// </summary>
        public event EventHandler<SamplesEventArgs<T>> ReceivedSamples;
    }
}
