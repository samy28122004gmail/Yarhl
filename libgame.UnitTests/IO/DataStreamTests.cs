﻿//
// DataStreamTests.cs
//
// Author:
//       Benito Palacios Sánchez <benito356@gmail.com>
//
// Copyright (c) 2017 Benito Palacios Sánchez
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace Libgame.UnitTests.IO
{
    using System;
    using System.IO;
    using Libgame.IO;
    using NUnit.Framework;

    [TestFixture]
    public class DataStreamTests
    {
        [Test]
        public void ConstructorSetStream()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0x1, 0x1);
            Assert.AreSame(baseStream, stream.BaseStream);
            Assert.AreEqual(0x1, stream.Offset);
            Assert.AreEqual(0x1, stream.Length);
            Assert.AreEqual(0x0, stream.Position);
        }

        [Test]
        public void ConstructorThrowExceptionIfNullStream()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DataStream((Stream)null, 0x10, 0x10));
        }

        [Test]
        public void ConstructorThrowExceptionIfNegativeOffset()
        {
            Stream baseStream = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DataStream(baseStream, -1, 4));
        }

        [Test]
        public void ConstructorThrowExceptionIfOffsetBiggerThanLength()
        {
            Stream baseStream = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DataStream(baseStream, 1, 0));
        }

        [Test]
        public void ConstructorThrowExceptionIfLengthLessThanMinusOne()
        {
            Stream baseStream = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DataStream(baseStream, 0, -2));
        }

        [Test]
        public void DataStreamLengthLargerThanBaseLengthIsNotAllowed()
        {
            Stream baseStream = new MemoryStream();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new DataStream(baseStream, 0, 100));
        }

        [Test]
        public void ConstructorSetLengthIfMinusOne()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, -1);
            Assert.AreEqual(2, stream.Length);
        }

        [Test]
        public void DisposeChangesDisposed()
        {
            DataStream stream = new DataStream();
            Assert.IsFalse(stream.Disposed);
            stream.Dispose();
            Assert.IsTrue(stream.Disposed);
        }

        [Test]
        public void DiposingLastStreamDisposeBaseStream()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);

            DataStream stream1 = new DataStream(baseStream);
            DataStream stream2 = new DataStream(baseStream);

            stream1.Dispose();
            Assert.DoesNotThrow(() => baseStream.ReadByte());

            stream2.Dispose();
            Assert.Throws<ObjectDisposedException>(() => baseStream.ReadByte());
        }

        [Test]
        public void DiposeCloseCorrectStream()
        {
            Stream baseStream1 = new MemoryStream();
            baseStream1.WriteByte(0xCA);
            Stream baseStream2 = new MemoryStream();
            baseStream2.WriteByte(0xCA);

            DataStream stream1 = new DataStream(baseStream1);
            DataStream stream2 = new DataStream(baseStream2);

            stream1.Dispose();
            Assert.Throws<ObjectDisposedException>(() => baseStream1.ReadByte());
            Assert.DoesNotThrow(() => baseStream2.ReadByte());

            stream2.Dispose();
        }

        [Test]
        public void ConstructorWithOnlyStreamSetOffsetAndLength()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream);
            Assert.AreSame(baseStream, stream.BaseStream);
            Assert.AreEqual(0x0, stream.Offset);
            Assert.AreEqual(0x2, stream.Length);
            Assert.AreEqual(0x0, stream.Position);
        }

        [Test]
        public void DefaultConstructorCreatesAMemoryStream()
        {
            DataStream stream = new DataStream();
            Assert.IsInstanceOf<MemoryStream>(stream.BaseStream);
            Assert.IsTrue(stream.BaseStream.CanWrite);
            Assert.IsTrue(stream.BaseStream.CanRead);
            Assert.AreEqual(0x0, stream.Offset);
            Assert.AreEqual(0x0, stream.Length);
            Assert.AreEqual(0x0, stream.Position);
        }

        [Test]
        public void FilePathConstructorOpenFile()
        {
            string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            DataStream writeStream = new DataStream(tempFile, FileOpenMode.Write);
            Assert.IsInstanceOf<FileStream>(writeStream.BaseStream);
            Assert.IsTrue(writeStream.BaseStream.CanWrite);
            Assert.IsFalse(writeStream.BaseStream.CanRead);
            Assert.AreEqual(0x0, writeStream.Offset);
            Assert.AreEqual(0x0, writeStream.Length);
            Assert.AreEqual(0x0, writeStream.Position);
            writeStream.WriteByte(0xCA);
            writeStream.Dispose();

            DataStream readStream = new DataStream(tempFile, FileOpenMode.Read);
            Assert.IsInstanceOf<FileStream>(readStream.BaseStream);
            Assert.IsFalse(readStream.BaseStream.CanWrite);
            Assert.IsTrue(readStream.BaseStream.CanRead);
            Assert.AreEqual(0x0, readStream.Offset);
            Assert.AreEqual(0x1, readStream.Length);
            Assert.AreEqual(0x0, readStream.Position);
            Assert.AreEqual(0xCA, readStream.ReadByte());
            readStream.Dispose();

            File.Delete(tempFile);
        }

        [Test]
        public void ConstructorFromDataStreamSetProperties()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            baseStream.WriteByte(0xBE);
            baseStream.WriteByte(0xBA);
            DataStream stream1 = new DataStream(baseStream, 1, 3);

            DataStream stream2 = new DataStream(stream1, 1, 1);
            Assert.AreSame(baseStream, stream2.BaseStream);
            Assert.AreEqual(0x2, stream2.Offset);
            Assert.AreEqual(0x1, stream2.Length);
            Assert.AreEqual(0x0, stream2.Position);
            Assert.AreEqual(0xBE, stream2.ReadByte());
        }

        [Test]
        public void ConstructorThrowExceptionIfNullDataStream()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DataStream((DataStream)null, 0x10, 0x10));
        }

        [Test]
        public void SetPositionChangesProperty()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Position = 1;
            Assert.AreEqual(1, stream.Position);
            stream.Position += 1;
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void SetPositionAfterDisposeThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.Position = 2);
        }

        [Test]
        public void SetNegativePositionThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = -1);
        }

        [Test]
        public void SetOutofRangePositionThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Position = 10);
        }

        [Test]
        public void SetLengthMinusOneTakesBaseStream()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 1);
            Assert.AreEqual(1, stream.Length);
            stream.Length = -1;
            Assert.AreEqual(2, stream.Length);
        }

        [Test]
        public void LengthAfterDisposeThrowException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            DataStream stream = new DataStream(baseStream, 0, 1);
            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.Length = 1);
        }

        [Test]
        public void OutOfRangeLengthThrowException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            DataStream stream = new DataStream(baseStream, 0, 0);
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Length = -2);
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Length = 2);
        }

        [Test]
        public void DecreaseLengthAdjustPosition()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Seek(2, SeekMode.Start);
            Assert.AreEqual(2, stream.Position);
            stream.Length = 1;
            Assert.AreEqual(1, stream.Position);
        }

        [Test]
        public void EndOfStreamIsSet()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            Assert.IsFalse(stream.EndOfStream);
            stream.Seek(2, SeekMode.Start);
            Assert.IsTrue(stream.EndOfStream);
        }

        [Test]
        public void GetValidAbsolutePosition()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 1, 1);
            stream.Seek(1, SeekMode.Start);
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(2, stream.AbsolutePosition);
        }

        [Test]
        public void SeekFromOrigin()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Seek(1, SeekMode.Start);
            Assert.AreEqual(1, stream.Position);
        }

        [Test]
        public void SeekFromCurrent()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Seek(1, SeekMode.Start);
            Assert.AreEqual(1, stream.Position);
            stream.Seek(1, SeekMode.Current);
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void SeekFromEnd()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Seek(0, SeekMode.End);
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void SeekWhenDisposedThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.Seek(1, SeekMode.Start));
            Assert.AreEqual(0, stream.Position);
        }

        [Test]
        public void SeekToNegativeThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(10, SeekMode.End));
            Assert.AreEqual(0, stream.Position);
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-10, SeekMode.Current));
            Assert.AreEqual(0, stream.Position);
        }

        [Test]
        public void SeekToOutOfRangeThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Position = 1;
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(10, SeekMode.Start));
            Assert.AreEqual(1, stream.Position);
        }

        [Test]
        public void ReadsByte()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            Assert.AreEqual(0xCA, stream.ReadByte());
            Assert.AreEqual(1, stream.Position);
            Assert.AreEqual(0xFE, stream.ReadByte());
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void ReadByteAfterDisposeThrowException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.ReadByte());
        }

        [Test]
        public void ReadByteWhenEOSThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Seek(0, SeekMode.End);
            Assert.Throws<EndOfStreamException>(() => stream.ReadByte());
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void ReadSetBaseStreamPosition()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            baseStream.Position = 1;
            Assert.AreEqual(0xCA, stream.ReadByte());
        }

        [Test]
        public void ReadsBuffer()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            byte[] buffer = new byte[2];
            Assert.AreEqual(1, stream.Read(buffer, 0, 1));
            Assert.AreEqual(0xCA, buffer[0]);
            Assert.AreEqual(0x00, buffer[1]);
            Assert.AreEqual(1, stream.Position);

            Assert.AreEqual(1, stream.Read(buffer, 1, 1));
            Assert.AreEqual(0xCA, buffer[0]);
            Assert.AreEqual(0xFE, buffer[1]);
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void ReadBufferfterDisposeThrowException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Dispose();
            byte[] buffer = new byte[2];
            Assert.Throws<ObjectDisposedException>(() => stream.Read(buffer, 0, 0));
        }

        [Test]
        public void ReadBufferWhenEOSThrowsException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            stream.Seek(0, SeekMode.End);
            byte[] buffer = new byte[2];
            Assert.Throws<EndOfStreamException>(() => stream.Read(buffer, 0, 1));
            Assert.AreEqual(2, stream.Position);
        }

        [Test]
        public void ReadBufferSetBaseStreamPosition()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            baseStream.Position = 1;
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            Assert.AreEqual(0xCA, buffer[0]);
        }

        [Test]
        public void ReadBufferZeroBytes()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            byte[] buffer = new byte[2];
            Assert.DoesNotThrow(() => stream.Read(buffer, 10, 0));
            Assert.AreEqual(0, stream.Read(buffer, 10, 0));
        }

        [Test]
        public void ReadBufferButNullThrowException()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            Assert.DoesNotThrow(() => stream.Read(null, 0, 0));
            Assert.Throws<ArgumentNullException>(() => stream.Read(null, 0, 1));
        }

        [Test]
        public void ReadBufferOutOfRange()
        {
            Stream baseStream = new MemoryStream();
            baseStream.WriteByte(0xCA);
            baseStream.WriteByte(0xFE);
            DataStream stream = new DataStream(baseStream, 0, 2);
            byte[] buffer = new byte[2];
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(buffer, 10, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Read(buffer, 0, 10));
        }

        [Test]
        public void WritesAByteAndIncreasePosition()
        {
            MemoryStream baseStream = new MemoryStream(2);
            DataStream stream = new DataStream(baseStream);
            stream.WriteByte(0xCA);
            Assert.AreEqual(1, stream.Position);
            stream.Position = 0;
            Assert.AreEqual(0xCA, stream.ReadByte());
        }

        [Test]
        public void WriteByteIncreaseLength()
        {
            DataStream stream = new DataStream();
            Assert.DoesNotThrow(() => stream.WriteByte(0xCA));
            Assert.AreEqual(1, stream.Length);
            Assert.AreEqual(1, stream.Position);
            stream.Position = 0;
            Assert.AreEqual(0xCA, stream.ReadByte());
        }

        [Test]
        public void WriteByteAfterDisposeThrowException()
        {
            DataStream stream = new DataStream();
            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.WriteByte(0xCA));
        }

        [Test]
        public void WriteByteSetBaseStreamPosition()
        {
            MemoryStream baseStream = new MemoryStream(2);
            DataStream stream = new DataStream(baseStream);
            baseStream.Position = 1;
            stream.WriteByte(0xCA);
            baseStream.Position = 0;
            Assert.AreEqual(0xCA, baseStream.ReadByte());
        }

        [Test]
        public void WriteBufferAndIncreasePosition()
        {
            MemoryStream baseStream = new MemoryStream(2);
            DataStream stream = new DataStream(baseStream);
            byte[] buffer = {0x00, 0xCA};
            stream.Write(buffer, 1, 1);
            Assert.AreEqual(1, stream.Position);
            stream.Position = 0;
            Assert.AreEqual(0xCA, stream.ReadByte());
        }

        [Test]
        public void WriteBufferAndIncreaseLength()
        {
            MemoryStream baseStream = new MemoryStream();
            baseStream.WriteByte(0xFF);
            DataStream stream = new DataStream(baseStream);
            Assert.AreEqual(1, stream.Length);
            byte[] buffer = {0xCA, 0xFE};
            Assert.DoesNotThrow(() => stream.Write(buffer, 0, 2));
            Assert.AreEqual(2, stream.Position);
            Assert.AreEqual(2, stream.Length);
            stream.Position = 0;
            Assert.AreEqual(0xCA, stream.ReadByte());
            Assert.AreEqual(0xFE, stream.ReadByte());
        }

        [Test]
        public void WriteBufferWhenEOSIncreaseLength()
        {
            DataStream stream = new DataStream();
            byte[] buffer = {0xCA, 0xFE};
            Assert.DoesNotThrow(() => stream.Write(buffer, 0, 2));
            Assert.AreEqual(2, stream.Position);
            Assert.AreEqual(2, stream.Length);
            stream.Position = 0;
            Assert.AreEqual(0xCA, stream.ReadByte());
            Assert.AreEqual(0xFE, stream.ReadByte());
        }

        [Test]
        public void WriteBufferAfterDisposeThrowException()
        {
            DataStream stream = new DataStream();
            byte[] buffer = {0xCA};
            stream.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream.Write(buffer, 0, 1));
        }

        [Test]
        public void WriteBufferSetBaseStreamPosition()
        {
            MemoryStream baseStream = new MemoryStream();
            baseStream.WriteByte(0xFF);
            DataStream stream = new DataStream(baseStream);
            Assert.AreEqual(1, baseStream.Position);
            byte[] buffer = {0xCA};
            stream.Write(buffer, 0, 1);
            baseStream.Position = 0;
            Assert.AreEqual(0xCA, baseStream.ReadByte());
        }

        [Test]
        public void WriteBufferButNullThrowException()
        {
            DataStream stream = new DataStream();
            Assert.Throws<ArgumentNullException>(() => stream.Write(null, 0, 1));
        }

        [Test]
        public void WriteBufferOutOfRangeThrowException()
        {
            DataStream stream = new DataStream();
            byte[] buffer = {0xCA};
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(buffer, 10, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(buffer, 0, 10));
        }

        [Test]
        public void WriteBufferWithZeroBytes()
        {
            DataStream stream = new DataStream();
            byte[] buffer = {0xCA};
            Assert.DoesNotThrow(() => stream.Write(null, 0, 0));
            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(0, stream.Length);
            Assert.DoesNotThrow(() => stream.Write(buffer, 10, 0));
            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(0, stream.Length);
            stream.Write(buffer, 0, 0);
            Assert.AreEqual(0, stream.Position);
            Assert.AreEqual(0, stream.Length);
        }

        [Test]
        public void CompareTwoEqualStreams()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x00);
            stream2.WriteByte(0xFF);
            Assert.IsTrue(stream1.Compare(stream2));
            Assert.IsTrue(stream2.Compare(stream1));
        }

        [Test]
        public void CompareTwoDifferentContentStreams()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x01);
            stream2.WriteByte(0xFF);
            Assert.IsFalse(stream1.Compare(stream2));
            Assert.IsFalse(stream2.Compare(stream1));
        }

        [Test]
        public void CompareTwoDifferentLengthStreams()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x00);
            Assert.IsFalse(stream1.Compare(stream2));
            Assert.IsFalse(stream2.Compare(stream1));
        }

        [Test]
        public void CompareWithNullStream()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            Assert.Throws<ArgumentNullException>(() => stream1.Compare(null));
        }

        [Test]
        public void CompareWithDisposedStream()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            stream2.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream1.Compare(stream2));
        }

        [Test]
        public void CompareAfterDispose()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            stream1.Dispose();
            Assert.Throws<ObjectDisposedException>(() => stream1.Compare(stream2));
        }

        [Test]
        public void CompareStartsFromBeginning()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x00);
            stream2.WriteByte(0xFF);
            stream1.Position = 1;
            Assert.IsTrue(stream1.Compare(stream2));
        }

        [Test]
        public void CompareDoesNotResetPosition()
        {
            DataStream stream1 = new DataStream();
            stream1.WriteByte(0xCA);
            stream1.WriteByte(0xFE);
            stream1.WriteByte(0x00);
            stream1.WriteByte(0xFF);
            DataStream stream2 = new DataStream();
            stream2.WriteByte(0xCA);
            stream2.WriteByte(0xFE);
            stream2.WriteByte(0x00);
            stream2.WriteByte(0xFF);
            stream1.Position = 1;
            stream2.Position = 2;
            Assert.IsTrue(stream1.Compare(stream2));
            Assert.AreEqual(1, stream1.Position);
            Assert.AreEqual(2, stream2.Position);
        }
    }
}
