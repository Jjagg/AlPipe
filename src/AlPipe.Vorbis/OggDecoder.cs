using System;
using System.IO;
using NVorbis;

namespace AlPipe.Vorbis
{
    /// <summary>
    /// Decoder for Ogg audio streams.
    /// </summary>
    public sealed class OggDecoder : ISampleStream<float>, IDisposable
    {
        private readonly VorbisReader _reader;
        private bool _disposed;

        /// <inheritdoc />
        public bool CanSeek => true;
        /// <inheritdoc />
        public long Length => _reader.TotalSamples;
        /// <inheritdoc />
        public TimeSpan Duration => _reader.TotalTime;

        /// <inheritdoc />
        public long SamplePosition
        {
            get => _reader.DecodedPosition;
            set => _reader.DecodedPosition = value;
        }

        /// <inheritdoc />
        public TimeSpan TimePosition
        {
            get => _reader.DecodedTime;
            set => _reader.DecodedTime = value;
        }

        /// <inheritdoc />
        public Format Format { get; }

        /// <summary>
        /// Create a new <see cref="OggDecoder"/> that reads the file with the given path.
        /// </summary>
        /// <param name="fileName">Filename for the .ogg file to decode.</param>
        public OggDecoder(string fileName)
            : this(File.OpenRead(fileName), true)
        {
        }

        /// <summary>
        /// Create a new <see cref="OggDecoder"/> that reads the file with the given path.
        /// </summary>
        /// <param name="stream">Stream to decode.</param>
        /// <param name="closeStreamOnDispose">Indicates if the decoder should dispose the stream when disposed.</param>
        public OggDecoder(Stream stream, bool closeStreamOnDispose = false)
        {
            _reader = new VorbisReader(stream, closeStreamOnDispose);

            Format = new Format(_reader.Channels, _reader.SampleRate);
        }

        /// <inheritdoc />
        public int Read(float[] samples, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().FullName, "Cannot use a disposed decoder.");
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset can't be negative.");
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be larger than 0.");
            if (samples.Length < offset + count)
                throw new ArgumentException("Samples array not large enough.", nameof(samples));

            var read = _reader.ReadSamples(samples, offset, count);

            if (SamplesRead != null)
            {
                var args = new SamplesReadEventArgs<float>(samples, read);
                SamplesRead?.Invoke(this, args);
            }

            return read;
        }

        /// <inheritdoc />
        public event EventHandler<EventArgs> SamplesRead;

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
                _reader.Dispose();
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~OggDecoder()
        {
            Dispose(false);
        }
    }
}
