using System;
using System.Collections.Generic;

namespace AlPipe
{
    /// <summary>
    /// Event arguments containing a collection of samples.
    /// </summary>
    /// <typeparam name="T">Type of the samples.</typeparam>
    public class SamplesEventArgs<T> : EventArgs
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
        /// Create a new <see cref="SamplesEventArgs{T}"/> instance.
        /// </summary>
        /// <param name="samples">Read sample data.</param>
        /// <param name="count">Number of samples.</param>
        public SamplesEventArgs(IEnumerable<T> samples, int count)
        {
            Samples = samples;
            Count = count;
        }
    }
}
