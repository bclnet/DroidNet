using System;
using System.NumericsX;
using System.NumericsX.OpenStack;
using System.Runtime.CompilerServices;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace Gengine.Render
{
    static unsafe partial class R
    {
        static void R_WriteTGA(string filename, byte[] data, int width, int height, bool flipVertical)
        {
            var bufferSize = width * height * 4 + 18;
            const int imgStart = 18;

            var buffer = new byte[bufferSize];
            //memset(buffer, 0, 18);
            buffer[2] = 2;      // uncompressed type
            buffer[12] = (byte)(width & 255);
            buffer[13] = (byte)(width >> 8);
            buffer[14] = (byte)(height & 255);
            buffer[15] = (byte)(height >> 8);
            buffer[16] = 32;    // pixel size
            if (!flipVertical) buffer[17] = (1 << 5);  // flip bit, for normal top to bottom raster order

            // swap rgb to bgr
            for (var i = imgStart; i < bufferSize; i += 4)
            {
                buffer[i] = data[i - imgStart + 2];     // blue
                buffer[i + 1] = data[i - imgStart + 1];     // green
                buffer[i + 2] = data[i - imgStart + 0];     // red
                buffer[i + 3] = data[i - imgStart + 3];     // alpha
            }

            fileSystem.WriteFile(filename, buffer, bufferSize);
        }

        static void R_WritePalTGA(string filename, byte[] data, byte[] palette, int width, int height, bool flipVertical)
        {
            int i;
            var bufferSize = (width * height) + (256 * 3) + 18;
            const int palStart = 18;
            const int imgStart = 18 + (256 * 3);

            var buffer = new byte[bufferSize];
            //memset(buffer, 0, 18);
            buffer[1] = 1;      // color map type
            buffer[2] = 1;      // uncompressed color mapped image
            buffer[5] = 0;      // number of palette entries (lo)
            buffer[6] = 1;      // number of palette entries (hi)
            buffer[7] = 24;     // color map bpp
            buffer[12] = (byte)(width & 255);
            buffer[13] = (byte)(width >> 8);
            buffer[14] = (byte)(height & 255);
            buffer[15] = (byte)(height >> 8);
            buffer[16] = 8; // pixel size
            if (!flipVertical) buffer[17] = (1 << 5);  // flip bit, for normal top to bottom raster order

            // store palette, swapping rgb to bgr
            for (i = palStart; i < imgStart; i += 3)
            {
                buffer[i] = palette[i - palStart + 2];      // blue
                buffer[i + 1] = palette[i - palStart + 1];      // green
                buffer[i + 2] = palette[i - palStart + 0];      // red
            }

            // store the image data
            for (i = imgStart; i < bufferSize; i++) buffer[i] = data[i - imgStart];

            fileSystem.WriteFile(filename, buffer, bufferSize);
        }

        // PCX files are used for 8 bit images
        unsafe struct Pcx
        {
            public char manufacturer;
            public char version;
            public char encoding;
            public char bits_per_pixel;
            public ushort xmin, ymin, xmax, ymax;
            public ushort hres, vres;
            public fixed byte palette[48];
            public char reserved;
            public char color_planes;
            public ushort bytes_per_line;
            public ushort palette_type;
            public fixed char filler[58];
            public byte data;           // unbounded
        }

        // TGA files are used for 24/32 bit images
        unsafe struct TargaHeader
        {
            public byte id_length, colormap_type, image_type;
            public ushort colormap_index, colormap_length;
            public byte colormap_size;
            public ushort x_origin, y_origin, width, height;
            public byte pixel_size, attributes;
        }

        // BMP LOADING
        unsafe struct BMPHeader
        {
            public fixed byte id[2];
            public uint fileSize;
            public uint reserved0;
            public uint bitmapDataOffset;
            public uint bitmapHeaderSize;
            public uint width;
            public uint height;
            public ushort planes;
            public ushort bitsPerPixel;
            public uint compression;
            public uint bitmapDataSize;
            public uint hRes;
            public uint vRes;
            public uint colors;
            public uint importantColors;
            public fixed byte palette[256 * 4];
        }

        static void LoadBMP(string name, ref byte[] pic, out int width, out int height, out DateTime timestamp)
        {
            int columns, rows, numPixels;
            byte* pixbuf;
            int row, column;
            int length;
            BMPHeader bmpHeader;
            byte* bmpRGBA;

            if (pic == byteX.Empty) { fileSystem.ReadFile(name, out timestamp); width = default; height = default; return; } // just getting timestamp

            pic = null;

            // load the file
            length = fileSystem.ReadFile(name, out var buffer, out timestamp);
            if (buffer == null) { width = default; height = default; return; }

            fixed (byte* bufferB = buffer)
            {
                var buf_p = bufferB;
                bmpHeader.id[0] = *buf_p++;
                bmpHeader.id[1] = *buf_p++;
                bmpHeader.fileSize = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.reserved0 = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.bitmapDataOffset = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.bitmapHeaderSize = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.width = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.height = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.planes = LittleUShort(*(ushort*)buf_p); buf_p += 2;
                bmpHeader.bitsPerPixel = LittleUShort(*(ushort*)buf_p); buf_p += 2;
                bmpHeader.compression = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.bitmapDataSize = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.hRes = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.vRes = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.colors = LittleUInt(*(uint*)buf_p); buf_p += 4;
                bmpHeader.importantColors = LittleUInt(*(uint*)buf_p); buf_p += 4;

                Unsafe.CopyBlock(bmpHeader.palette, buf_p, 256 * 4);

                if (bmpHeader.bitsPerPixel == 8) buf_p += 1024;
                if (bmpHeader.id[0] != 'B' && bmpHeader.id[1] != 'M') common.Error($"LoadBMP: only Windows-style BMP files supported ({name})\n");
                if (bmpHeader.fileSize != length) common.Error($"LoadBMP: header size does not match file size ({bmpHeader.fileSize} vs. {length}) ({name})\n");
                if (bmpHeader.compression != 0) common.Error($"LoadBMP: only uncompressed BMP files supported ({name})\n");
                if (bmpHeader.bitsPerPixel < 8) common.Error($"LoadBMP: monochrome and 4-bit BMP files not supported ({name})\n");

                columns = (int)bmpHeader.width;
                rows = (int)bmpHeader.height;
                if (rows < 0) rows = -rows;
                numPixels = columns * rows;

                width = columns;
                height = rows;

                bmpRGBA = (byte*)TR.R_StaticAlloc<byte[]>(numPixels * 4);
                pic = bmpRGBA;

                for (row = rows - 1; row >= 0; row--)
                {
                    pixbuf = bmpRGBA + row * columns * 4;

                    for (column = 0; column < columns; column++)
                    {
                        byte red, green, blue, alpha; int palIndex; ushort shortPixel;

                        switch (bmpHeader.bitsPerPixel)
                        {
                            case 8:
                                palIndex = *buf_p++;
                                *pixbuf++ = bmpHeader.palette[palIndex >> 2 + 2];
                                *pixbuf++ = bmpHeader.palette[palIndex >> 2 + 1];
                                *pixbuf++ = bmpHeader.palette[palIndex >> 2 + 0];
                                *pixbuf++ = 0xff;
                                break;
                            case 16:
                                shortPixel = *(ushort*)pixbuf; pixbuf += 2;
                                *pixbuf++ = (byte)((shortPixel & (31 << 10)) >> 7);
                                *pixbuf++ = (byte)((shortPixel & (31 << 5)) >> 2);
                                *pixbuf++ = (byte)((shortPixel & (31)) << 3);
                                *pixbuf++ = 0xff;
                                break;
                            case 24:
                                blue = *buf_p++;
                                green = *buf_p++;
                                red = *buf_p++;
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = 255;
                                break;
                            case 32:
                                blue = *buf_p++;
                                green = *buf_p++;
                                red = *buf_p++;
                                alpha = *buf_p++;
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = alpha;
                                break;
                            default: common.Error($"LoadBMP: illegal pixel_size '{bmpHeader.bitsPerPixel}' in file '{name}'\n"); break;
                        }
                    }
                }
            }
            fileSystem.FreeFile(buffer);
        }

        #region PCX LOADING

        static void LoadPCX(string filename, ref byte[] pic, out byte[] palette, out int width, out int height, out DateTime timestamp)
        {
            Pcx pcx;
            int x, y;
            int len;
            int dataByte, runLength;
            byte* o, *pix;
            int xmax, ymax;

            if (pic == byteX.Empty) { fileSystem.ReadFile(filename, out timestamp); goto Error; } // just getting timestamp

            pic = null;
            palette = null;

            // load the file
            len = fileSystem.ReadFile(filename, out var raw, out timestamp);
            if (raw == null) goto Error;

            // parse the PCX file
            pcx = (Pcx)raw;
            raw = &pcx.data;

            xmax = LittleUShort(pcx.xmax);
            ymax = LittleUShort(pcx.ymax);

            if (pcx.manufacturer != 0x0a || pcx.version != 5 || pcx.encoding != 1 || pcx.bits_per_pixel != 8 || xmax >= 1024 || ymax >= 1024) { common.Printf($"Bad pcx file {filename} ({xmax + 1} x {ymax + 1}) ({pcx.xmax} x {pcx.ymax})\n"); goto Error; }

            o = (byte*)TR.R_StaticAlloc((ymax + 1) * (xmax + 1));

            pic = o;

            pix = o;

            palette = (byte*)TR.R_StaticAlloc(768);
            memcpy(palette, (byte*)pcx + len - 768, 768);

            width = xmax + 1;
            height = ymax + 1;
            // FIXME: use bytes_per_line here?

            for (y = 0; y <= ymax; y++, pix += xmax + 1)
                for (x = 0; x <= xmax;)
                {
                    dataByte = *raw++;

                    if ((dataByte & 0xC0) == 0xC0) { runLength = dataByte & 0x3F; dataByte = *raw++; }
                    else runLength = 1;

                    while (runLength-- > 0) pix[x++] = dataByte;
                }

            if (raw - (byte*)pcx > len) { common.Printf($"PCX file {filename} was malformed"); R_StaticFree(pic); pic = null; }

            fileSystem.FreeFile(pcx);
            return;

        Error:
            palette = default; width = default; height = default;
            return;
        }

        static void LoadPCX32(string filename, out byte[] pic, out int width, out int height, out DateTime timestamp)
        {
            byte* palette;
            byte* pic8;
            int i, c, p;
            byte* pic32;

            if (!pic) { fileSystem.ReadFile(filename, null, timestamp); return; } // just getting timestamp
            LoadPCX(filename, &pic8, &palette, width, height, timestamp);
            if (!pic8) { *pic = null; return; }

            c = (width) * (height);
            pic32 = pic = (byte*)TR.R_StaticAlloc(4 * c);
            for (i = 0; i < c; i++)
            {
                p = pic8[i];
                pic32[0] = palette[p * 3];
                pic32[1] = palette[p * 3 + 1];
                pic32[2] = palette[p * 3 + 2];
                pic32[3] = 255;
                pic32 += 4;
            }

            R_StaticFree(pic8);
            R_StaticFree(palette);
        }


        #endregion

        #region TARGA LOADING

        static void LoadTGA(string name, out byte[] pic, out int width, out int height, out DateTime timestamp)
        {
            int columns, rows, numPixels, fileSize, numBytes;
            byte* pixbuf;
            int row, column;
            byte* buf_p;
            byte* buffer;
            TargaHeader targa_header;
            byte* targa_rgba;

            if (!pic) { fileSystem.ReadFile(name, null, timestamp); return; } // just getting timestamp

            pic = null;

            // load the file
            fileSize = fileSystem.ReadFile(name, (void**)&buffer, timestamp);
            if (buffer == null) return;

            buf_p = buffer;

            targa_header.id_length = *buf_p++;
            targa_header.colormap_type = *buf_p++;
            targa_header.image_type = *buf_p++;

            targa_header.colormap_index = LittleShort(*(short*)buf_p); buf_p += 2;
            targa_header.colormap_length = LittleShort(*(short*)buf_p); buf_p += 2;
            targa_header.colormap_size = *buf_p++;
            targa_header.x_origin = LittleShort(*(short*)buf_p); buf_p += 2;
            targa_header.y_origin = LittleShort(*(short*)buf_p); buf_p += 2;
            targa_header.width = LittleShort(*(short*)buf_p); buf_p += 2;
            targa_header.height = LittleShort(*(short*)buf_p); buf_p += 2;
            targa_header.pixel_size = *buf_p++;
            targa_header.attributes = *buf_p++;

            if (targa_header.image_type != 2 && targa_header.image_type != 10 && targa_header.image_type != 3) common.Error($"LoadTGA({name}): Only type 2 (RGB), 3 (gray), and 10 (RGB) TGA images supported\n");

            if (targa_header.colormap_type != 0) common.Error($"LoadTGA({name}): colormaps not supported\n");

            if ((targa_header.pixel_size != 32 && targa_header.pixel_size != 24) && targa_header.image_type != 3) common.Error($"LoadTGA({name}): Only 32 or 24 bit images supported (no colormaps)\n");

            if (targa_header.image_type == 2 || targa_header.image_type == 3)
            {
                numBytes = targa_header.width * targa_header.height * (targa_header.pixel_size >> 3);
                if (numBytes > fileSize - 18 - targa_header.id_length) common.Error($"LoadTGA({name}): incomplete file\n");
            }

            columns = targa_header.width;
            rows = targa_header.height;
            numPixels = columns * rows;

            width = columns;
            height = rows;

            targa_rgba = (byte*)R_StaticAlloc(numPixels * 4);
            pic = targa_rgba;

            if (targa_header.id_length != 0) buf_p += targa_header.id_length;  // skip TARGA image comment

            if (targa_header.image_type == 2 || targa_header.image_type == 3)
            {
                // Uncompressed RGB or gray scale image
                for (row = rows - 1; row >= 0; row--)
                {
                    pixbuf = targa_rgba + row * columns * 4;
                    for (column = 0; column < columns; column++)
                    {
                        byte red, green, blue, alphabyte;
                        switch (targa_header.pixel_size)
                        {

                            case 8:
                                blue = *buf_p++;
                                green = blue;
                                red = blue;
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = 255;
                                break;

                            case 24:
                                blue = *buf_p++;
                                green = *buf_p++;
                                red = *buf_p++;
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = 255;
                                break;
                            case 32:
                                blue = *buf_p++;
                                green = *buf_p++;
                                red = *buf_p++;
                                alphabyte = *buf_p++;
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = alphabyte;
                                break;
                            default:
                                common.Error($"LoadTGA({name}): illegal pixel_size '{targa_header.pixel_size}'\n");
                                break;
                        }
                    }
                }
            }
            else if (targa_header.image_type == 10)
            { // Runlength encoded RGB images
                byte red, green, blue, alphabyte, packetHeader, packetSize, j;

                red = 0;
                green = 0;
                blue = 0;
                alphabyte = 0xff;

                for (row = rows - 1; row >= 0; row--)
                {
                    pixbuf = targa_rgba + row * columns * 4;
                    for (column = 0; column < columns;)
                    {
                        packetHeader = *buf_p++;
                        packetSize = 1 + (packetHeader & 0x7f);
                        if (packetHeader & 0x80)
                        {        // run-length packet
                            switch (targa_header.pixel_size)
                            {
                                case 24:
                                    blue = *buf_p++;
                                    green = *buf_p++;
                                    red = *buf_p++;
                                    alphabyte = 255;
                                    break;
                                case 32:
                                    blue = *buf_p++;
                                    green = *buf_p++;
                                    red = *buf_p++;
                                    alphabyte = *buf_p++;
                                    break;
                                default: common.Error($"LoadTGA({name}): illegal pixel_size '{targa_header.pixel_size}'\n"); break;
                            }

                            for (j = 0; j < packetSize; j++)
                            {
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = alphabyte;
                                column++;
                                if (column == columns)
                                { // run spans across rows
                                    column = 0;
                                    if (row > 0) row--;
                                    else goto breakOut;
                                    pixbuf = targa_rgba + row * columns * 4;
                                }
                            }
                        }
                        else
                        {                          // non run-length packet
                            for (j = 0; j < packetSize; j++)
                            {
                                switch (targa_header.pixel_size)
                                {
                                    case 24:
                                        blue = *buf_p++;
                                        green = *buf_p++;
                                        red = *buf_p++;
                                        *pixbuf++ = red;
                                        *pixbuf++ = green;
                                        *pixbuf++ = blue;
                                        *pixbuf++ = 255;
                                        break;
                                    case 32:
                                        blue = *buf_p++;
                                        green = *buf_p++;
                                        red = *buf_p++;
                                        alphabyte = *buf_p++;
                                        *pixbuf++ = red;
                                        *pixbuf++ = green;
                                        *pixbuf++ = blue;
                                        *pixbuf++ = alphabyte;
                                        break;
                                    default: common.Error($"LoadTGA({name}): illegal pixel_size '{targa_header.pixel_size}'\n"); break;
                                }
                                column++;
                                if (column == columns)
                                { // pixel packet run spans across rows
                                    column = 0;
                                    if (row > 0) row--;
                                    else goto breakOut;
                                    pixbuf = targa_rgba + row * columns * 4;
                                }
                            }
                        }
                    }
                breakOut:
                    ;
                }
            }

            // image flp bit
            if ((targa_header.attributes & (1 << 5))) _VerticalFlip(*pic, *width, *height);


            fileSystem.FreeFile(buffer);
        }

        static void LoadJPG(string filename, out byte[] pic, out int width, out int height, out DateTime timestamp)
        {
            // This struct contains the JPEG decompression parameters and pointers to working space (which is allocated as needed by the JPEG library).
            jpeg_decompress_struct cinfo;
            // We use our private extension JPEG error handler. Note that this struct must live as long as the main JPEG parameter struct, to avoid dangling-pointer problems.

            // This struct represents a JPEG error handler.  It is declared separately because applications often want to supply a specialized error handler
            // (see the second half of this file for an example).  But here we just take the easy way out and use the standard error handler, which will
            // print a message on stderr and call exit() if compression fails. Note that this struct must live as long as the main JPEG parameter struct, to avoid dangling-pointer problems.

            jpeg_error_mgr jerr;
            // More stuff
            JSAMPARRAY buffer;      // Output row buffer
            int row_stride;     // physical row width in output buffer
            byte* o;
            byte* fbuffer;
            byte* bbuf;

            // In this example we want to open the input file before doing anything else, so that the setjmp() error recovery below can assume the file is open.
            // VERY IMPORTANT: use "b" option to fopen() if you are on a machine that requires it in order to read binary files.

            // JDC: because fill_input_buffer() blindly copies INPUT_BUF_SIZE bytes, we need to make sure the file buffer is padded or it may crash
            pic = null;        // until proven otherwise

            int len;
            VFile f;

            f = fileSystem.OpenFileRead(filename);
            if (f == null) return;
            len = f.Length();
            timestamp = f.Timestamp();
            if (!pic) { fileSystem.CloseFile(f); return; } // just getting timestamp
            fbuffer = (byte*)Mem_ClearedAlloc(len + 4096);
            f.Read(fbuffer, len);
            fileSystem.CloseFile(f);

            // Step 1: allocate and initialize JPEG decompression object

            // We have to set up the error handler first, in case the initialization step fails.  (Unlikely, but it could happen if you are out of memory.)
            // This routine fills in the contents of struct jerr, and returns jerr's address which we place into the link field in cinfo.
            cinfo.err = jpeg_std_error(&jerr);

            // Now we can initialize the JPEG decompression object.
            jpeg_create_decompress(&cinfo);

            // Step 2: specify data source (eg, a file)

            jpeg_mem_src(&cinfo, fbuffer, len);

            // Step 3: read file parameters with jpeg_read_header()

            jpeg_read_header(&cinfo, true);
            // We can ignore the return value from jpeg_read_header since
            //   (a) suspension is not possible with the stdio data source, and
            //   (b) we passed TRUE to reject a tables-only JPEG file as an error.
            // See libjpeg.doc for more info.

            // Step 4: set parameters for decompression

            // In this example, we don't need to change any of the defaults set by jpeg_read_header(), so we do nothing here.

            // Step 5: Start decompressor

            jpeg_start_decompress(&cinfo);
            // We can ignore the return value since suspension is not possible with the stdio data source.


            // We may need to do some setup of our own at this point before reading the data.  After jpeg_start_decompress() we have the correct scaled
            // output image dimensions available, as well as the output colormap if we asked for color quantization.
            // In this example, we need to make an output work buffer of the right size.

            // JSAMPLEs per row in output buffer
            row_stride = cinfo.output_width * cinfo.output_components;

            if (cinfo.output_components != 4) common.DWarning($"JPG {filename} is unsupported color depth ({cinfo.output_components})");
            o = (byte*)R_StaticAlloc(cinfo.output_width * cinfo.output_height * 4);

            pic = o;
            width = cinfo.output_width;
            height = cinfo.output_height;

            // Step 6: while (scan lines remain to be read)
            //           jpeg_read_scanlines(...); 

            // Here we use the library's state variable cinfo.output_scanline as the loop counter, so that we don't have to keep track ourselves.
            while (cinfo.output_scanline < cinfo.output_height)
            {
                // jpeg_read_scanlines expects an array of pointers to scanlines. Here the array is only one element long, but you could ask for more than one scanline at a time if that's more convenient.
                bbuf = ((o + (row_stride * cinfo.output_scanline)));
                buffer = &bbuf;
                jpeg_read_scanlines(&cinfo, buffer, 1);
            }

            // clear all the alphas to 255
            {
                int i, j;
                byte* buf;

                buf = *pic;

                j = cinfo.output_width * cinfo.output_height * 4;
                for (i = 3; i < j; i += 4) buf[i] = 255;
            }

            // Step 7: Finish decompression

            jpeg_finish_decompress(&cinfo);
            // We can ignore the return value since suspension is not possible with the stdio data source.

            // Step 8: Release JPEG decompression object

            // This is an important step since it will release a good deal of memory.
            jpeg_destroy_decompress(&cinfo);

            // After finish_decompress, we can close the input file. Here we postpone it until after no more JPEG errors are possible,
            // so as to simplify the setjmp error logic above.  (Actually, I don't think that jpeg_destroy can do an error exit, but why assume anything...)
            Mem_Free(fbuffer);

            // At this point you may want to check to see whether any corrupt-data warnings occurred (test whether jerr.pub.num_warnings is nonzero).

            // And we're done!
        }

        #endregion

        // Loads any of the supported image types into a cannonical 32 bit format.
        // Automatically attempts to load .jpg files if .tga files fail to load.
        // *pic will be null if the load failed.
        // Anything that is going to make this into a texture would use makePowerOf2 = true, but something loading an image as a lookup table of some sort would leave it in identity form.
        // It is important to do this at image load time instead of texture load time for bump maps.
        // Timestamp may be null if the value is going to be ignored
        // If pic is null, the image won't actually be loaded, it will just find the timestamp.
        static void R_LoadImage(string cname, out byte[] pic, out int width, out int height, out DateTime timestamp, bool makePowerOf2)
        {
            var name = cname;

            pic = null;
            timestamp = 0xFFFFFFFF;
            width = 0;
            height = 0;

            name = PathX.DefaultFileExtension(name, ".tga");

            if (name.Length < 5) return;

            name = name.ToLowerInvariant();
            PathX.ExtractFileExtension(name, out var ext);

            if (ext == "tga")
            {
                LoadTGA(name, out pic, out width, out height, out timestamp); // try tga first
                if ((pic && *pic == 0) || (timestamp && *timestamp == -1)) LoadJPG(PathX.DefaultFileExtension(PathX.StripFileExtension(name), ".jpg"), pic, width, height, timestamp);
            }
            else if (ext == "pcx") LoadPCX32(name, out pic, out width, out height, out timestamp);
            else if (ext == "bmp") LoadBMP(name, out pic, out width, out height, out timestamp);
            else if (ext == "jpg") LoadJPG(name, out pic, out width, out height, out timestamp);

            if ((width && *width < 1) || (height && *height < 1))
                if (pic && *pic) { TR.R_StaticFree(*pic); *pic = 0; }

            // convert to exact power of 2 sizes
            if (pic && *pic && makePowerOf2)
            {
                int w, h;
                int scaled_width, scaled_height;
                byte* resampledBuffer;

                w = width;
                h = height;

                for (scaled_width = 1; scaled_width < w; scaled_width <<= 1) ;
                for (scaled_height = 1; scaled_height < h; scaled_height <<= 1) ;

                if (scaled_width != w || scaled_height != h)
                {
                    if (ImageManager.image_roundDown.Bool && scaled_width > w) scaled_width >>= 1;
                    if (ImageManager.image_roundDown.Bool && scaled_height > h) scaled_height >>= 1;

                    resampledBuffer = TR.R_ResampleTexture(pic, w, h, scaled_width, scaled_height);
                    TR.R_StaticFree(pic);
                    pic = resampledBuffer;
                    width = scaled_width;
                    height = scaled_height;
                }
            }
        }

        // Loads six files with proper extensions
        static bool R_LoadCubeImages(string imgName, CubeFiles extensions, byte[] pics, out int outSize, out DateTime timestamp)
        {
            int i, j;
            string[] cameraSides = { "_forward.tga", "_back.tga", "_left.tga", "_right.tga", "_up.tga", "_down.tga" };
            string[] axisSides = { "_px.tga", "_nx.tga", "_py.tga", "_ny.tga", "_pz.tga", "_nz.tga" };
            string[] sides;
            string fullName;
            int width, height, size = 0;

            sides = extensions == CF_CAMERA ? cameraSides : axisSides;

            if (pics != null) memset(pics, 0, 6 * sizeof(pics[0]));
            timestamp = 0;

            for (i = 0; i < 6; i++)
            {
                fullName = $"{imgName}{sides[i]}";

                DateTime thisTime;
                R_LoadImageProgram(fullName, pics != null ? null : pics[i], out width, out height, out thisTime);
                if (thisTime == FILE_NOT_FOUND_TIMESTAMP) break;
                if (i == 0) size = width;
                if (width != size || height != size) { common.Warning($"Mismatched sizes on cube map '{imgName}'"); break; }
                if (thisTime > timestamp) timestamp = thisTime;
                if (pics && extensions == CF_CAMERA)
                    // convert from "camera" images to native cube map images
                    switch (i)
                    {
                        // forward
                        case 0: R_RotatePic(pics[i], width); break;
                        // back
                        case 1: R_RotatePic(pics[i], width); R_HorizontalFlip(pics[i], width, height); R_VerticalFlip(pics[i], width, height); break;
                        // left
                        case 2: R_VerticalFlip(pics[i], width, height); break;
                        // right
                        case 3: R_HorizontalFlip(pics[i], width, height); break;
                        // up
                        case 4: R_RotatePic(pics[i], width); break;
                        // down
                        case 5: R_RotatePic(pics[i], width); break;
                    }
            }

            if (i != 6)
            {
                // we had an error, so free everything
                if (pics != null) for (j = 0; j < i; j++) TR.R_StaticFree(pics[j]);
                timestamp = 0;
                return false;
            }

            outSize = size;
            return true;
        }
    }
}