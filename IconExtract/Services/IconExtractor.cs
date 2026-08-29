using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IconExtract.Services
{
    public class IconExtractor
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LockResource(IntPtr hResData);

        [DllImport("kernel32.dll")]
        private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        private delegate bool EnumResNameProc(IntPtr hModule, IntPtr lpType, IntPtr lpName, IntPtr lParam);
        private delegate bool EnumTypesProc(IntPtr hModule, IntPtr lpType, IntPtr lParam);
        private delegate bool EnumLanguageProc(IntPtr hModule, IntPtr lpType, IntPtr lpName, ushort wLang, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpType, EnumResNameProc lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumResourceTypes(IntPtr hModule, EnumTypesProc lpEnumFunc, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumResourceLanguages(IntPtr hModule, IntPtr lpType, IntPtr lpName, EnumLanguageProc lpEnumFunc, IntPtr lParam);

        private const uint DONT_RESOLVE_DLL_REFERENCES = 0x00000001;
        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        private const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;
        private static readonly IntPtr RT_ICON = 3;
        private static readonly IntPtr RT_GROUP_ICON = 14;

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        private struct ICONDIR
        {
            public ushort idReserved;
            public ushort idType;
            public ushort idCount;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ICONDIRENTRY
        {
            public byte bWidth;
            public byte bHeight;
            public byte bColorCount;
            public byte bReserved;
            public ushort wPlanes;
            public ushort wBitCount;
            public uint dwBytesInRes;
            public uint dwImageOffset;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 2)]
        public struct GRPICONDIRENTRY
        {
            public byte bWidth;
            public byte bHeight;
            public byte bColorCount;
            public byte bReserved;
            public ushort wPlanes;
            public ushort wBitCount;
            public uint dwBytesInRes;
            public ushort nID;
        }

        public static List<byte[]> ExtractIcons(string filePath)
        {
            IntPtr hModule = LoadLibraryEx(filePath, IntPtr.Zero, LOAD_LIBRARY_AS_IMAGE_RESOURCE);
            if (hModule == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "LoadLibraryEx() failed");
            }

            try
            {
                List<byte[]> result = [];
                EnumResourceNames(hModule, RT_GROUP_ICON, (h, type, name, param) =>
                {
                    byte[] icons = BuildIcon(h, type, name, 0);
                    if (icons.Length > 0)
                    {
                        result.Add(icons);
                    }
                    return true;
                }, IntPtr.Zero);

                return result;
            }
            finally
            {
                FreeLibrary(hModule);
            }
        }

        private static byte[] BuildIcon(IntPtr hModule, IntPtr lpType, IntPtr lpName, ushort wLang)
        {
            IntPtr hResInfo = FindResource(hModule, lpName, lpType);
            if (hResInfo == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine(Marshal.GetLastWin32Error());
                return [];
            }

            IntPtr hResData = LoadResource(hModule, hResInfo);
            IntPtr pGroupData = LockResource(hResData);
            if (pGroupData == IntPtr.Zero)
            {
                return [];
            }

            ICONDIR dir = Marshal.PtrToStructure<ICONDIR>(pGroupData);
            int entryCount = dir.idCount;

            GRPICONDIRENTRY[] groupEntries = new GRPICONDIRENTRY[entryCount];
            IntPtr pEntry = IntPtr.Add(pGroupData, Marshal.SizeOf<ICONDIR>());
            int groupEntrySize = Marshal.SizeOf<GRPICONDIRENTRY>();

            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms);

            using MemoryStream dataStream = new();
            using BinaryWriter dataWriter = new(dataStream);

            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.Write((ushort)entryCount);

            int headerSize = 6 + 16 * entryCount;
            uint currentOffset = (uint)headerSize;

            for (int i = 0; i < entryCount; i++)
            {
                GRPICONDIRENTRY entry = Marshal.PtrToStructure<GRPICONDIRENTRY>(IntPtr.Add(pEntry, i * groupEntrySize));
                IntPtr hIconResInfo = FindResource(hModule, entry.nID, RT_ICON);
                if (hIconResInfo == IntPtr.Zero)
                {
                    continue;
                }

                IntPtr hIconResData = LoadResource(hModule, hIconResInfo);
                IntPtr pIconData = LockResource(hIconResData);
                uint size = SizeofResource(hModule, hIconResInfo);

                byte[] buffer = new byte[size];
                Marshal.Copy(pIconData, buffer, 0, (int)size);

                writer.Write(entry.bWidth);
                writer.Write(entry.bHeight);
                writer.Write(entry.bColorCount);
                writer.Write(entry.bReserved);
                writer.Write(entry.wPlanes);
                writer.Write(entry.wBitCount);
                writer.Write(size);
                writer.Write(currentOffset);

                currentOffset += size;

                dataWriter.Write(buffer);
            }

            writer.Write(dataStream.ToArray());

            return ms.ToArray();
        }
    }
}
