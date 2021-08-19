using Gengine.Framework;
using Gengine.Render;
using System;
using System.NumericsX;
using System.NumericsX.Core;
using SoundSample = System.Object;
using ChannelType = System.Int32; // the game uses its own series of enums, and we don't want to require casts
using FourCC = System.Int32;
using ALuint = System.UInt32;
using System.Collections.Generic;
using static System.NumericsX.Lib;
using System.Diagnostics;

namespace Gengine.Sound
{
    public class WaveFile
    {
        WaveformatExtensible mpwfx;        // Pointer to waveformatex structure
        VFile mhmmio;         // I/O handle for the WAVE
        Mminfo mck;           // Multimedia RIFF chunk
        Mminfo mckRiff;       // used when opening a WAVE file
        int mdwSize;      // size in samples
        int mMemSize;     // size of the wave data in memory
        int mseekBase;
        DateTime mfileTime;

        bool mbIsReadingFromMemory;
        short[] mpbData;
        short[] mpbDataCur;
        int mulDataSize;

        object ogg;          // only !null when !s_realTimeDecoding
        bool isOgg;

        // Constructs the class.  Call Open() to open a wave file for reading.
        // Then call Read() as needed.  Calling the destructor or Close() will close the file.
        public WaveFile()
        {
            mpwfx.memset();
            mhmmio = null;
            mdwSize = 0;
            mseekBase = 0;
            mbIsReadingFromMemory = false;
            mpbData = null;
            ogg = null;
            isOgg = false;
        }

        // Destructs the class
        public void Dispose()
        {
            Close();

            if (mbIsReadingFromMemory && mpbData != null)
                mpbData = null;

            mpwfx.memset();
        }

        // Opens a wave file for reading
        public int Open(string strFileName, Action<WaveformatEx> pwfx = null)
        {
            mbIsReadingFromMemory = false;

            mpbData = null;
            mpbDataCur = mpbData;

            if (strFileName == null)
                return -1;

            var name = strFileName;

            // note: used to only check for .wav when making a build
            name.SetFileExtension(".ogg");
            if (fileSystem.ReadFile(name, out var _) != -1)
                return OpenOGG(name, pwfx);

            mpwfx.memset();

            mhmmio = fileSystem.OpenFileRead(strFileName);
            if (mhmmio == null) { mdwSize = 0; return -1; }
            if (mhmmio.Length <= 0) { mhmmio = null; return -1; }
            if (ReadMMIO() != 0) { Close(); return -1; } // ReadMMIO will fail if its an not a wave file
            mfileTime = mhmmio.Timestamp;
            if (ResetFile() != 0) { Close(); return -1; }

            // After the reset, the size of the wav file is mck.cksize so store it now
            mdwSize = mck.cksize / sizeof(short);
            mMemSize = mck.cksize;

            if (mck.cksize != 0xffffffff)
            {
                pwfx?.Invoke(mpwfx.Format);
                return 0;
            }
            return -1;
        }

        // copy data to idWaveFile member variable from memory
        public int OpenFromMemory(short[] pbData, int ulDataSize, WaveformatExtensible pwfx)
        {
            mpwfx = pwfx;
            mulDataSize = ulDataSize;
            mpbData = pbData;
            mpbDataCur = mpbData;
            mdwSize = ulDataSize / sizeof(short);
            mMemSize = ulDataSize;
            mbIsReadingFromMemory = true;

            return 0;
        }

        // Support function for reading from a multimedia I/O stream. mhmmio must be valid before calling.  This function uses it to update mckRiff, and mpwfx.
        int ReadMMIO()
        {
            Mminfo ckIn;           // chunk info. for general use.
            Pcmwaveformat pcmWaveFormat;  // Temp PCM structure to load in.

            mpwfx.memset();

            mhmmio.Read(mckRiff, 12);
            Debug.Assert(!isOgg);
            mckRiff.ckid = LittleInt(mckRiff.ckid);
            mckRiff.cksize = LittleInt(mckRiff.cksize);
            mckRiff.fccType = LittleInt(mckRiff.fccType);
            mckRiff.dwDataOffset = 12;

            // Check to make sure this is a valid wave file
            if (mckRiff.ckid != fourcc_riff || mckRiff.fccType != mmioFOURCC('W', 'A', 'V', 'E'))
                return -1;

            // Search the input file for for the 'fmt ' chunk.
            ckIn.dwDataOffset = 12;
            do
            {
                if (mhmmio.Read(&ckIn, 8) != 8)
                    return -1;
                Debug.Assert(!isOgg);
                ckIn.ckid = LittleInt(ckIn.ckid);
                ckIn.cksize = LittleInt(ckIn.cksize);
                ckIn.dwDataOffset += ckIn.cksize - 8;
            } while (ckIn.ckid != mmioFOURCC('f', 'm', 't', ' '));

            // Expect the 'fmt' chunk to be at least as large as <PCMWAVEFORMAT>; if there are extra parameters at the end, we'll ignore them
            if (ckIn.cksize < sizeof(pcmwaveformat_t))
                return -1;

            // Read the 'fmt ' chunk into <pcmWaveFormat>.
            if (mhmmio.Read(&pcmWaveFormat, sizeof(pcmWaveFormat)) != sizeof(pcmWaveFormat))
                return -1;
            Debug.Assert(!isOgg);
            pcmWaveFormat.wf.wFormatTag = LittleShort(pcmWaveFormat.wf.wFormatTag);
            pcmWaveFormat.wf.nChannels = LittleShort(pcmWaveFormat.wf.nChannels);
            pcmWaveFormat.wf.nSamplesPerSec = LittleInt(pcmWaveFormat.wf.nSamplesPerSec);
            pcmWaveFormat.wf.nAvgBytesPerSec = LittleInt(pcmWaveFormat.wf.nAvgBytesPerSec);
            pcmWaveFormat.wf.nBlockAlign = LittleShort(pcmWaveFormat.wf.nBlockAlign);
            pcmWaveFormat.wBitsPerSample = LittleShort(pcmWaveFormat.wBitsPerSample);

            // Copy the bytes from the pcm structure to the waveformatex_t structure
            memcpy(&mpwfx, &pcmWaveFormat, sizeof(pcmWaveFormat));

            // Allocate the waveformatex_t, but if its not pcm format, read the next word, and thats how many extra bytes to allocate.
            if (pcmWaveFormat.wf.wFormatTag == WAVE_FORMAT_TAG_PCM)
                mpwfx.Format.cbSize = 0;
            else
            {
                return -1;  // we don't handle these (32 bit wavefiles, etc)
#if false
                // Read in length of extra bytes.
                word cbExtraBytes = 0L;
                if (mhmmio.Read((char*)&cbExtraBytes, sizeof(word)) != sizeof(word))
                    return -1;

                mpwfx.Format.cbSize = cbExtraBytes;

                // Now, read those extra bytes into the structure, if cbExtraAlloc != 0.
                if (mhmmio.Read((char*)(((byte*)&(mpwfx.Format.cbSize)) + sizeof(word)), cbExtraBytes) != cbExtraBytes)
                {
                    memset(&mpwfx, 0, sizeof(waveformatextensible_t));
                    return -1;
                }
#endif
            }

            return 0;
        }

        // Resets the internal mck pointer so reading starts from the beginning of the file again
        public int ResetFile()
        {
            if (mbIsReadingFromMemory)
                mpbDataCur = mpbData;
            else
            {
                if (mhmmio == null)
                    return -1;

                // Seek to the data
                if (mhmmio.Seek(mckRiff.dwDataOffset + sizeof(fourcc), FS_SEEK_SET) == -1)
                    return -1;

                // Search the input file for for the 'fmt ' chunk.
                mck.ckid = 0;
                do
                {
                    byte ioin;
                    if (mhmmio.Read(&ioin, 1) == 0)
                        return -1;
                    mck.ckid = (mck.ckid >> 8) | (ioin << 24);
                } while (mck.ckid != mmioFOURCC('d', 'a', 't', 'a'));

                mhmmio.Read(&mck.cksize, 4);
                Debug.Assert(!isOgg);
                mck.cksize = LittleInt(mck.cksize);
                mseekBase = mhmmio.Tell;
            }

            return 0;
        }

        // Reads section of data from a wave file into pBuffer and returns how much read in pdwSizeRead, reading not more than dwSizeToRead.
        // This uses mck to determine where to start reading from.  So subsequent calls will be continue where the last left off unless
        // Reset() is called.
        public int Read(byte[] pBuffer, int dwSizeToRead, out int pdwSizeRead)
        {
            if (ogg != null)
                return ReadOGG(pBuffer, dwSizeToRead, pdwSizeRead);
            else if (mbIsReadingFromMemory)
            {
                if (mpbDataCur == null)
                    return -1;
                if ((byte*)(mpbDataCur + dwSizeToRead) > (byte*)(mpbData + mulDataSize))
                    dwSizeToRead = mulDataSize - (int)(mpbDataCur - mpbData);
                SIMDProcessor.Memcpy(pBuffer, mpbDataCur, dwSizeToRead);
                mpbDataCur += dwSizeToRead;

                if (pdwSizeRead != null)
                    *pdwSizeRead = dwSizeToRead;
                return dwSizeToRead;

            }
            else
            {
                if (mhmmio == null)
                    return -1;
                if (pBuffer == null)
                    return -1;

                dwSizeToRead = mhmmio.Read(pBuffer, dwSizeToRead);
                // this is hit by ogg code, which does it's own byte swapping internally
                if (!isOgg)
                    LittleRevBytes(pBuffer, 2, dwSizeToRead / 2);

                if (pdwSizeRead != null)
                    *pdwSizeRead = dwSizeToRead;

                return dwSizeToRead;
            }
        }

        public int Seek(int offset)
        {
            if (ogg != null)
                common.FatalError("WaveFile::Seek: cannot seek on an OGG file\n");
            else if (mbIsReadingFromMemory)
                mpbDataCur = mpbData + offset;
            else
            {
                if (mhmmio == null)
                    return -1;

                if ((offset + mseekBase) == mhmmio.Tell)
                    return 0;
                mhmmio.Seek(offset + mseekBase, FS_SEEK.SET);
                return 0;
            }
            return -1;
        }

        // Closes the wave file
        public int Close()
        {
            if (ogg != null)
                return CloseOGG();
            if (mhmmio != null)
            {
                fileSystem.CloseFile(mhmmio);
                mhmmio = null;
            }
            return 0;
        }

        public int OutputSize => mdwSize;

        public int MemorySize => mMemSize;

        int OpenOGG(string strFileName, WaveformatEx pwfx = null);

        int ReadOGG(byte[] pBuffer, int dwSizeToRead, out int pdwSizeRead);

        int CloseOGG();
    }
}
