using System;
using System.IO;

namespace DIKUArcade.GUI;

public readonly struct Bmp
{
    //HEADER
    private const ushort FILE_TYPE = 19778; //BM 
    private const ushort RESERVED = 0;
    private const uint PIXEL_ARRAY_OFFSET = 54;

    //INFO HEADER
    private const uint FILE_HEADER_SIZE = 40;
    private const ushort NUMBER_OF_COLOR_PLANES = 1;
    private readonly ushort _bitsPerPixel;
    private const uint COMPRESSION_METHOD = 0;
    private const uint HORIZONTAL_RESOLUTION = 0;
    private const uint VERTICAL_RESOLUTION = 0;
    private const uint NUMBER_OF_COLORS_IN_COLOR_TABLE = 0;
    private const uint NUMBER_OF_SIGNIFICANT_COLORS = 0;

    private readonly byte[] _data;
    private readonly uint _width;
    private readonly uint _height;
    private readonly uint _pixelArraySize;
    private readonly uint _fileSize;

    public Bmp(uint width, uint height, ushort channels)
    {
        _bitsPerPixel = (ushort)(channels * 8);
        _width = width;
        _height = height;

        //const int padding = 31;
        //var rowSize = (_width * BITS_PER_PIXEL + padding) / 32 * 4;
        _pixelArraySize = _width * _height * channels;
        _fileSize = _pixelArraySize + PIXEL_ARRAY_OFFSET;

        // Create a byte array to hold the .bmp file data
        _data = new byte[_fileSize];
        WriteHeader();
    }

    public void WritePixelData(byte[] pixelData)
    {
        Array.Copy(pixelData, 0, _data, PIXEL_ARRAY_OFFSET, pixelData.Length);
    }

    public bool Save(string outputPath)
    {
        if (outputPath is null || File.Exists(outputPath))
        {
            return false;
        } 
        File.WriteAllBytes(outputPath, _data);
        return true;
    }
    
    public static Bmp ConvertPixelDataToBmp(byte[] pixelData, uint width, uint height, ushort channels)
    {
        Bmp bmp = new Bmp(width, height, channels);
        bmp.WritePixelData(pixelData);
        return bmp;
    }
    
    public static Bmp ConvertPixelDataToBmp(Span<byte> pixelData, uint width, uint height, ushort channels)
    {
        Bmp bmp = new Bmp(width, height, channels);
        bmp.WritePixelData(pixelData.ToArray());
        return bmp;
    }
    
    private void WriteHeader()
    {
        // Set the file header data
        WriteUInt16(_data, 0, FILE_TYPE);
        WriteUInt32(_data, 2, _fileSize);
        WriteUInt16(_data, 6, RESERVED); // Reserved
        WriteUInt16(_data, 8, RESERVED); // Reserved
        WriteUInt32(_data, 10, PIXEL_ARRAY_OFFSET); // The offset to the pixel array

        // Set the bitmap info header data
        WriteUInt32(_data, 14, FILE_HEADER_SIZE); // Header Size
        WriteUInt32(_data, 18, _width); // Width
        WriteUInt32(_data, 22, _height); // Height
        WriteUInt16(_data, 26, NUMBER_OF_COLOR_PLANES); // Number of color planes
        WriteUInt16(_data, 28, _bitsPerPixel); // Bits per pixel
        WriteUInt32(_data, 30, COMPRESSION_METHOD); // Compression method (0 = uncompressed)
        WriteUInt32(_data, 34, _pixelArraySize); // Size of the pixel array
        WriteUInt32(_data, 38, HORIZONTAL_RESOLUTION); // Horizontal resolution (pixels per meter)
        WriteUInt32(_data, 42, VERTICAL_RESOLUTION); // Vertical resolution (pixels per meter)
        WriteUInt32(_data, 46, NUMBER_OF_COLORS_IN_COLOR_TABLE); // Number of colors in the color palette
        WriteUInt32(_data, 50, NUMBER_OF_SIGNIFICANT_COLORS); // Number of important colors (0 = all)
    }
    
    private static void WriteUInt16(byte[] data, int offset, ushort value)
    {
        //Convert the value to little-endian byte representation
        data[offset + 0] = (byte)(value & 0xFF);
        data[offset + 1] = (byte)(value >> 8);
    }

    private static void WriteUInt32(byte[] data, int offset, uint value)
    {
        //Convert the value to little-endian byte representation
        data[offset + 0] = (byte)(value & 0xFF);
        data[offset + 1] = (byte)((value >> 8) & 0xFF);
        data[offset + 2] = (byte)((value >> 16) & 0xFF);
        data[offset + 3] = (byte)(value >> 24);
    }
}