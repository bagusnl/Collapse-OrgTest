﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using master._7zip.Legacy;

namespace ManagedLzma._7zip.Decoder
{
    internal class SegmentedStream : Stream
    {
        // private const int tmpBufferSize = 0x10000;
        private const int tmpBufferSize = 4 << 10;
        private const int tmpSkipBufferSize = 0x10000;

        private Stream sourceStream;
        private int bufferOffset;
        private long bufferLength;
        private CancellationToken cancelToken;

        internal SegmentedStream(Stream sourceStream, long fileLength, CancellationToken token = new CancellationToken())
        {
            this.sourceStream = sourceStream;
            this.bufferLength = fileLength;
            this.cancelToken = token;
        }

        public void SkipLength(long skipLength)
        {
            int buffLength = (int)Math.Min(skipLength, tmpSkipBufferSize);
            long lastLength = bufferLength;

            bufferLength = skipLength;
            if (buffLength > 0)
                while ((bufferOffset += Read(new byte[tmpSkipBufferSize], 0, buffLength)) < skipLength)
                {
                    cancelToken.ThrowIfCancellationRequested();
                }

            bufferLength = lastLength;
            bufferOffset = 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            cancelToken.ThrowIfCancellationRequested();
            base.CopyTo(destination, bufferSize);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bufferOffset == bufferLength)
                return 0;

            count = sourceStream.Read(buffer, offset, (int)Math.Min(count, bufferLength - bufferOffset));
            bufferOffset += count;
            return count;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Length
        {
            get { return bufferLength; }
        }

        public override long Position
        {
            get { return bufferOffset; }
            set
            {
                if (value < 0 || value > bufferLength)
                    throw new ArgumentOutOfRangeException("value");

                bufferOffset = (int)value;
            }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }
    }

    internal class FileBufferedDecoderStream : Stream
    {
        // Extracted from non-public System.IO.FileStream.DefaultBufferSize via reflection because all FileStream
        // constructors taking FileOptions also take a buffer size, but we don't really want to specify one.
        private const int kStreamBufferSize = 0x1000;

        private byte[] mTemp = new byte[4 << 10];
        private Stream mBuffer;
        private Stream mStream;
        private CFileItem mItem;
        private int mOffset;
        private int mEnding;
        private int mLength;

        internal FileBufferedDecoderStream(Stream stream, CFileItem item)
        {
            mItem = item;
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
                throw new ArgumentException("Stream must support reading.", "stream");

            mStream = stream;
            mLength = checked((int)stream.Length);

            // string tempFileName = Path.GetTempFileName();
            Console.WriteLine($"Processing: {item.Name} Size: {item.Size}");
            // mBuffer = new FileStream(tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Delete, kStreamBufferSize, FileOptions.DeleteOnClose);
            mBuffer = new MemoryStream();
            if (mBuffer.Length != 0) // if this happens some other process tries to mess with our files - same if above ctor throws an exception
                throw new InvalidOperationException("Someone else took control of our temporary file while we were creating it.");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mBuffer.Dispose();
                mStream.Dispose();
            }

            base.Dispose(disposing);
        }

        /*
        private void Fetch()
        {
            int size = mStream.Read(mTemp, 0, mTemp.Length);
            if (size <= 0 || size > mLength - mEnding)
                throw new InvalidDataException("Decoded stream has been corrupted.");

            mBuffer.Position = mEnding;
            mEnding += size;
            mBuffer.Write(mTemp, 0, size);
        }
        */

        private void Fetch()
        {
            int size = mStream.Read(mTemp, 0, mTemp.Length);
            if (size <= 0 || size > mLength - mEnding)
                throw new InvalidDataException("Decoded stream has been corrupted.");

            mBuffer.Position = mEnding;
            mEnding += size;
            mBuffer.Write(mTemp, 0, size);
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override long Length
        {
            get { return mLength; }
        }

        public override long Position
        {
            get { return mOffset; }
            set
            {
                if (value < 0 || value > mLength)
                    throw new ArgumentOutOfRangeException("value");

                mOffset = (int)value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    return Position = offset;
                case SeekOrigin.Current:
                    return Position = checked(mOffset + offset);
                case SeekOrigin.End:
                    return Position = checked(mLength + offset);
                default:
                    throw new ArgumentOutOfRangeException("origin");
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (count < 0 || count > buffer.Length - offset)
                throw new ArgumentOutOfRangeException("count");

            if (count == 0 || mOffset == mLength)
                return 0;


            while (mOffset >= mEnding)
                Fetch();


            mBuffer.Position = mOffset;
            count = mBuffer.Read(buffer, offset, Math.Min(count, mEnding - mOffset));
            mOffset += count;
            return count;
        }

        /*
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0 || offset > buffer.Length)
                throw new ArgumentOutOfRangeException("offset");

            if (count < 0 || count > buffer.Length - offset)
                throw new ArgumentOutOfRangeException("count");

            if (count == 0 || mOffset == mLength)
                return 0;

            while (mOffset >= mEnding)
                Fetch();

            mBuffer.Position = mOffset;
            count = mBuffer.Read(buffer, offset, Math.Min(count, mEnding - mOffset));
            mOffset += count;
            return count;
        }
        */

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }
    }
}