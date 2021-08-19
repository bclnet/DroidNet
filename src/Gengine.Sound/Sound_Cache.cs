//#define USE_SOUND_CACHE_ALLOCATOR
using Gengine.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.NumericsX;
using System.NumericsX.Core;
using static Gengine.Lib;
using static System.NumericsX.Lib;
using ALuint = System.UInt32;

namespace Gengine.Sound
{
    public class SoundCache
    {
#if USE_SOUND_CACHE_ALLOCATOR
		public static DynamicBlockAlloc<byte> soundCacheAllocator = new(1 << 20, 1 << 10);
#else
        public static DynamicAlloc<byte> soundCacheAllocator = new(1 << 20, 1 << 10);
#endif

        bool insideLevelLoad;
        List<SoundSample> listCache = new();

        public SoundCache()
        {
            soundCacheAllocator.Init();
            soundCacheAllocator.SetLockMemory(true);
            listCache.AssureSize(1024, null);
            listCache.SetGranularity(256);
            insideLevelLoad = false;
        }

        public void Dispose()
        {
            listCache.Clear();
            soundCacheAllocator.Shutdown();
        }

        // Adds a sound object to the cache and returns a handle for it.
        public SoundSample FindSound(string filename, bool loadOnDemandOnly)
        {
            var fname = filename.Replace('\\', '/').ToLowerInvariant();

            declManager.MediaPrint($"{fname}\n");

            // check to see if object is already in cache
            SoundSample def;
            for (var i = 0; i < listCache.Count; i++)
            {
                def = listCache[i];
                if (def != null && def.name == fname)
                {
                    def.levelLoadReferenced = true;
                    if (def.purged && !loadOnDemandOnly)
                        def.Load();
                    return def;
                }
            }

            // create a new entry
            def = new SoundSample();

            var shandle = listCache.FindIndex(x => x == null);
            if (shandle != -1) listCache[shandle] = def;
            else shandle = listCache.Add_(def);

            def.name = fname;
            def.levelLoadReferenced = true;
            def.onDemand = loadOnDemandOnly;
            def.purged = true;

            // this may make it a default sound if it can't be loaded
            if (!loadOnDemandOnly)
                def.Load();

            return def;
        }

        public int NumObjects
            => listCache.Count;

        // returns a single cached object pointer
        public SoundSample GetObject(int index)
            => index < 0 || index > listCache.Count ? null : listCache[index];

        // Completely nukes the current cache
        public void ReloadSounds(bool force)
        {
            for (var i = 0; i < listCache.Count; i++)
            {
                var def = listCache[i];
                def?.Reload(force);
            }
        }

        // Mark all file based images as currently unused, but don't free anything.  Calls to ImageFromFile() will
        // either mark the image as used, or create a new image without loading the actual data.
        public void BeginLevelLoad()
        {
            insideLevelLoad = true;

            for (var i = 0; i < listCache.Count; i++)
            {
                var sample = listCache[i];
                if (sample == null)
                    continue;

                if (C.com_purgeAll.Bool)
                    sample.PurgeSoundSample();

                sample.levelLoadReferenced = false;
            }

            soundCacheAllocator.FreeEmptyBaseBlocks();
        }

        // Free all samples marked as unused
        public void EndLevelLoad()
        {
            int useCount, purgeCount;
            common.Printf("----- SoundCache::EndLevelLoad -----\n");

            insideLevelLoad = false;

            // purge the ones we don't need
            useCount = 0;
            purgeCount = 0;
            for (var i = 0; i < listCache.Count; i++)
            {
                var sample = listCache[i];
                if (sample == null)
                    continue;
                if (sample.purged)
                    continue;
                if (!sample.levelLoadReferenced)
                {
                    //common.Printf($"Purging {sample.name}\n");
                    purgeCount += sample.objectMemSize;
                    sample.PurgeSoundSample();
                }
                else useCount += sample.objectMemSize;
            }

            soundCacheAllocator.FreeEmptyBaseBlocks();

            common.Printf($"{useCount / 1024:5}k referenced\n");
            common.Printf($"{purgeCount / 1024:5}k purged\n");
        }

        public void PrintMemInfo(MemInfo mi)
        {
            int i, j, num = 0, total = 0;
            int[] sortIndex;
            VFile f;

            f = fileSystem.OpenFileWrite($"{mi.filebase}_sounds.txt");
            if (f == null)
                return;

            // count
            for (i = 0; i < listCache.Count; i++, num++)
                if (listCache[i] == null)
                    break;

            // sort first
            sortIndex = new int[num];

            for (i = 0; i < num; i++)
                sortIndex[i] = i;

            for (i = 0; i < num - 1; i++)
                for (j = i + 1; j < num; j++)
                    if (listCache[sortIndex[i]].objectMemSize < listCache[sortIndex[j]].objectMemSize)
                    {
                        var temp = sortIndex[i];
                        sortIndex[i] = sortIndex[j];
                        sortIndex[j] = temp;
                    }

            // print next
            for (i = 0; i < num; i++)
            {
                var sample = listCache[sortIndex[i]];

                // this is strange
                if (sample == null)
                    continue;

                total += sample.objectMemSize;
                f.Printf($"{sample.objectMemSize:n} {sample.name}\n");
            }

            mi.soundAssetsTotal = total;

            f.Printf("\nTotal sound bytes allocated: {total:n}\n");
            fileSystem.CloseFile(f);
        }
    }

    public class SoundSample
    {
        public const int SCACHE_SIZE = SIMD.MIXBUFFER_SAMPLES * 20; // 1/2 of a second (aroundabout)

        public SoundSample()
        {
            objectInfo.memset();
            objectSize = 0;
            objectMemSize = 0;
            nonCacheData = null;
            amplitudeData = null;
            openalBuffer = 0;
            hardwareBuffer = false;
            defaultSound = false;
            onDemand = false;
            purged = false;
            levelLoadReferenced = false;
        }

        public void Dispose()
            => PurgeSoundSample();

        public string name;                     // name of the sample file
        public DateTime timestamp;                    // the most recent of all images used in creation, for reloadImages command

        public WaveformatEx objectInfo;                  // what are we caching
        public int objectSize;                 // size of waveform in samples, excludes the header
        public int objectMemSize;              // object size in memory
        public byte[] nonCacheData;             // if it's not cached
        public byte[] amplitudeData;                // precomputed min,max amplitude pairs
        public ALuint openalBuffer;                // openal buffer
        public bool hardwareBuffer;
        public bool defaultSound;
        public bool onDemand;
        public bool purged;
        public bool levelLoadReferenced;       // so we can tell which samples aren't needed any more

        // objectSize is samples
        public int LengthIn44kHzSamples
        {
            get
            {
                if (objectInfo.nSamplesPerSec == 11025) return objectSize << 2;
                else if (objectInfo.nSamplesPerSec == 22050) return objectSize << 1;
                else return objectSize << 0;
            }
        }

        public DateTime NewTimeStamp
        {
            get
            {
                fileSystem.ReadFile(name, out var timestamp);
                if (timestamp == DateTime.MinValue)
                {
                    var oggName = $"{name.Substring(0, name.Length - Path.GetExtension(name).Length)}.ogg";
                    fileSystem.ReadFile(oggName, out timestamp);
                }
                return timestamp;
            }
        }

        public void MakeDefault()             // turns it into a beep
        {
            int i;
            float v;
            int sample;

            objectInfo.memset();

            objectInfo.nChannels = 1;
            objectInfo.wBitsPerSample = 16;
            objectInfo.nSamplesPerSec = 44100;

            objectSize = SIMD.MIXBUFFER_SAMPLES * 2;
            objectMemSize = objectSize * sizeof(short);

            nonCacheData = (byte*)SoundCache.soundCacheAllocator.Alloc(objectMemSize);

            short* ncd = (short*)nonCacheData;

            for (i = 0; i < SIMD.MIXBUFFER_SAMPLES; i++)
            {
                v = sin(MathX.PI * 2 * i / 64);
                sample = v * 0x4000;
                ncd[i * 2 + 0] = sample;
                ncd[i * 2 + 1] = sample;
            }

            alGetError();
            alGenBuffers(1, &openalBuffer);
            if (alGetError() != AL_NO_ERROR)
                common.Error("idSoundCache: error generating OpenAL hardware buffer");

            alGetError();
            alBufferData(openalBuffer, objectInfo.nChannels == 1 ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, nonCacheData, objectMemSize, objectInfo.nSamplesPerSec);
            if (alGetError() != AL_NO_ERROR)
            {
                common.Warning("idSoundCache: error loading data into OpenAL hardware buffer");
                hardwareBuffer = false;
            }
            else hardwareBuffer = true;

            defaultSound = true;
        }

        // Loads based on name, possibly doing a MakeDefault if necessary
        public void Load()                        // loads the current sound based on name
        {
            defaultSound = false;
            purged = false;
            hardwareBuffer = false;

            timestamp = NewTimeStamp;

            if (timestamp == FILE_NOT_FOUND_TIMESTAMP)
            {
                common.Warning($"Couldn't load sound '{name}' using default");
                MakeDefault();
                return;
            }

            // load it
            WaveFile fh = new();
            WaveformatEx info;

            if (fh.Open(name, info) == -1)
            {
                common.Warning($"Couldn't load sound '{name}' using default");
                MakeDefault();
                return;
            }

            if (info.nChannels != 1 && info.nChannels != 2)
            {
                common.Warning($"SoundSample: {name} has {info.nChannels} channels, using default");
                fh.Close();
                MakeDefault();
                return;
            }

            if (info.wBitsPerSample != 16)
            {
                common.Warning($"SoundSample: {name} is {info.wBitsPerSample}bits, expected 16bits using default");
                fh.Close();
                MakeDefault();
                return;
            }

            if (info.nSamplesPerSec != 44100 && info.nSamplesPerSec != 22050 && info.nSamplesPerSec != 11025)
            {
                common.Warning("SoundCache: {name} is {info.nSamplesPerSec}Hz, expected 11025, 22050 or 44100 Hz. Using default");
                fh.Close();
                MakeDefault();
                return;
            }

            objectInfo = info;
            objectSize = fh.OutputSize;
            objectMemSize = fh.MemorySize;

            nonCacheData = (byte*)soundCacheAllocator.Alloc(objectMemSize);
            fh.Read(nonCacheData, objectMemSize, out var _);

            // optionally convert it to 22kHz to save memory
            CheckForDownSample();

            // create hardware audio buffers. PCM loads directly
            if (objectInfo.wFormatTag == WAVE_FORMAT_TAG.PCM)
            {
                alGetError();
                alGenBuffers(1, &openalBuffer);
                if (alGetError() != AL_NO_ERROR)
                    common.Error("idSoundCache: error generating OpenAL hardware buffer");
                if (alIsBuffer(openalBuffer))
                {
                    alGetError();
                    alBufferData(openalBuffer, objectInfo.nChannels == 1 ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, nonCacheData, objectMemSize, objectInfo.nSamplesPerSec);
                    if (alGetError() != AL_NO_ERROR)
                    {
                        common.Warning("SoundCache: error loading data into OpenAL hardware buffer");
                        hardwareBuffer = false;
                    }
                    else hardwareBuffer = true;
                }

                // OGG decompressed at load time (when smaller than s_decompressionLimit seconds, 6 seconds by default)
                if (objectInfo.wFormatTag == WAVE_FORMAT_TAG.OGG && objectSize < (objectInfo.nSamplesPerSec * SoundSystemLocal.s_decompressionLimit.Integer != 0))
                {
                    alGetError();
                    alGenBuffers(1, &openalBuffer);
                    if (alGetError() != AL_NO_ERROR)
                        common.Error("SoundCache: error generating OpenAL hardware buffer");
                    if (alIsBuffer(openalBuffer))
                    {
                        var decoder = SampleDecoder.Alloc();
                        var destData = (float[])SoundCache.soundCacheAllocator.Alloc((LengthIn44kHzSamples + 1) * sizeof(float));

                        // Decoder *always* outputs 44 kHz data
                        decoder.Decode(this, 0, LengthIn44kHzSamples, destData);

                        // Downsample back to original frequency (save memory)
                        if (objectInfo.nSamplesPerSec == 11025)
                            for (var i = 0; i < objectSize; i++)
                            {
                                if (destData[i * 4] < -32768.0f) ((short*)destData)[i] = -32768;
                                else if (destData[i * 4] > 32767.0f) ((short*)destData)[i] = 32767;
                                else ((short*)destData)[i] = MathX.FtoiFast(destData[i * 4]);
                            }
                        else if (objectInfo.nSamplesPerSec == 22050)
                            for (var i = 0; i < objectSize; i++)
                            {
                                if (destData[i * 2] < -32768.0f) ((short*)destData)[i] = -32768;
                                else if (destData[i * 2] > 32767.0f) ((short*)destData)[i] = 32767;
                                else ((short*)destData)[i] = MathX.FtoiFast(destData[i * 2]);
                            }
                        else
                            for (var i = 0; i < objectSize; i++)
                            {
                                if (destData[i] < -32768.0f) ((short*)destData)[i] = -32768;
                                else if (destData[i] > 32767.0f) ((short*)destData)[i] = 32767;
                                else ((short*)destData)[i] = MathX.FtoiFast(destData[i]);
                            }

                        alGetError();
                        alBufferData(openalBuffer, objectInfo.nChannels == 1 ? AL_FORMAT_MONO16 : AL_FORMAT_STEREO16, destData, objectSize * sizeof(short), objectInfo.nSamplesPerSec);
                        if (alGetError() != AL_NO_ERROR)
                        {
                            common.Warning("SoundCache: error loading data into OpenAL hardware buffer");
                            hardwareBuffer = false;
                        }
                        else hardwareBuffer = true;

                        soundCacheAllocator.Free(destData);
                        SampleDecoder.Free(decoder);
                    }
                }
            }

            fh.Close();
        }

        public void Reload(bool force)        // reloads if timestamp has changed, or always if force
        {
            if (!force)
            {
                ID_TIME_T newTimestamp;

                // check the timestamp
                newTimestamp = GetNewTimeStamp();

                if (newTimestamp == FILE_NOT_FOUND_TIMESTAMP)
                {
                    if (!defaultSound)
                    {
                        common.Warning("Couldn't load sound '%s' using default", name.c_str());
                        MakeDefault();
                    }
                    return;
                }
                if (newTimestamp == timestamp)
                {
                    return; // don't need to reload it
                }
            }

            common.Printf("reloading %s\n", name.c_str());
            PurgeSoundSample();
            Load();
        }

        public void PurgeSoundSample()            // frees all data
        {
            purged = true;

            alGetError();
            alDeleteBuffers(1, &openalBuffer);
            if (alGetError() != AL_NO_ERROR)
            {
                common.Warning("idSoundCache: error unloading data from OpenAL hardware buffer");
            }

            openalBuffer = 0;
            hardwareBuffer = false;

            if (amplitudeData)
            {
                soundCacheAllocator.Free(amplitudeData);
                amplitudeData = NULL;
            }

            if (nonCacheData)
            {
                soundCacheAllocator.Free(nonCacheData);
                nonCacheData = NULL;
            }
        }

        public void CheckForDownSample()      // down sample if required
        {
            if (!idSoundSystemLocal::s_force22kHz.GetBool())
            {
                return;
            }
            if (objectInfo.wFormatTag != WAVE_FORMAT_TAG_PCM || objectInfo.nSamplesPerSec != 44100)
            {
                return;
            }
            int shortSamples = objectSize >> 1;
            short* converted = (short*)soundCacheAllocator.Alloc(shortSamples * sizeof(short));

            if (objectInfo.nChannels == 1)
            {
                for (int i = 0; i < shortSamples; i++)
                {
                    converted[i] = ((short*)nonCacheData)[i * 2];
                }
            }
            else
            {
                for (int i = 0; i < shortSamples; i += 2)
                {
                    converted[i + 0] = ((short*)nonCacheData)[i * 2 + 0];
                    converted[i + 1] = ((short*)nonCacheData)[i * 2 + 1];
                }
            }
            soundCacheAllocator.Free(nonCacheData);
            nonCacheData = (byte*)converted;
            objectSize >>= 1;
            objectMemSize >>= 1;
            objectInfo.nAvgBytesPerSec >>= 1;
            objectInfo.nSamplesPerSec >>= 1;
        }

        // Returns true on success.
        public bool FetchFromCache(int offset, byte[] output, out int position, out int size, bool allowIO)
        {
            offset &= 0xfffffffe;

            if (objectSize == 0 || offset < 0 || offset > objectSize * (int)sizeof(short) || !nonCacheData)
            {
                return false;
            }

            if (output)
            {
                *output = nonCacheData + offset;
            }
            if (position)
            {
                *position = 0;
            }
            if (size)
            {
                *size = objectSize * sizeof(short) - offset;
                if (*size > SCACHE_SIZE)
                {
                    *size = SCACHE_SIZE;
                }
            }
            return true;
        }

    }
}