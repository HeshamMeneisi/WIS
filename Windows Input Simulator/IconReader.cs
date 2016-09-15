using System.Runtime.InteropServices;
using System.Drawing;
using System;
using System.IO;
public class IconReader
{
    // Fields
    private const uint conFILE_ATTRIBUTE_DIRECTORY = 0x10;
    private const uint conFILE_ATTRIBUTE_NORMAL = 0x80;
    private const int conMAX_PATH = 260;

    // Methods
    [DllImport("User32.dll")]
    private static extern int DestroyIcon(IntPtr hIcon);
    public static Icon GetFileIcon(string filePath, EnumIconSize size, bool addLinkOverlay)
    {
        EnumFileInfoFlags flags = EnumFileInfoFlags.ICON | EnumFileInfoFlags.USEFILEATTRIBUTES;
        if (addLinkOverlay)
        {
            flags |= EnumFileInfoFlags.LINKOVERLAY;
        }
        if (size == EnumIconSize.Small)
        {
            flags |= EnumFileInfoFlags.SMALLICON;
        }
        ShellFileInfo psfi = new ShellFileInfo();
        SHGetFileInfo(filePath, 0x80, ref psfi, (uint)Marshal.SizeOf(psfi), (uint)flags);
        Icon icon = (Icon)Icon.FromHandle(psfi.hIcon).Clone();
        DestroyIcon(psfi.hIcon);
        return icon;
    }

    public static Icon GetFileIconByExt(string fileExt, EnumIconSize size, bool addLinkOverlay)
    {
        Icon icon;
        string path = Path.GetTempPath() + Guid.NewGuid().ToString("N") + fileExt;
        try
        {
            File.WriteAllBytes(path, new byte[1]);
            icon = GetFileIcon(path, size, addLinkOverlay);
        }
        finally
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
            }
        }
        return icon;
    }

    public static Icon GetFolderIcon(EnumIconSize size, EnumFolderType folderType)
    {
        return GetFolderIcon(null, size, folderType);
    }

    public static Icon GetFolderIcon(string folderPath, EnumIconSize size, EnumFolderType folderType)
    {
        EnumFileInfoFlags flags = EnumFileInfoFlags.ICON | EnumFileInfoFlags.USEFILEATTRIBUTES;
        if (folderType == EnumFolderType.Open)
        {
            flags |= EnumFileInfoFlags.OPENICON;
        }
        if (EnumIconSize.Small == size)
        {
            flags |= EnumFileInfoFlags.SMALLICON;
        }
        ShellFileInfo psfi = new ShellFileInfo();
        SHGetFileInfo(folderPath, 0x10, ref psfi, (uint)Marshal.SizeOf(psfi), (uint)flags);
        Icon icon = (Icon)Icon.FromHandle(psfi.hIcon).Clone();
        DestroyIcon(psfi.hIcon);
        return icon;
    }

    [DllImport("Shell32.dll")]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShellFileInfo psfi, uint cbFileInfo, uint uFlags);

    // 
    [Flags]
    private enum EnumFileInfoFlags : uint
    {
        ADDOVERLAYS = 0x20,
        ATTR_SPECIFIED = 0x20000,
        ATTRIBUTES = 0x800,
        DISPLAYNAME = 0x200,
        EXETYPE = 0x2000,
        ICON = 0x100,
        ICONLOCATION = 0x1000,
        LARGEICON = 0,
        LINKOVERLAY = 0x8000,
        OPENICON = 2,
        OVERLAYINDEX = 0x40,
        PIDL = 8,
        SELECTED = 0x10000,
        SHELLICONSIZE = 4,
        SMALLICON = 1,
        SYSICONINDEX = 0x4000,
        TYPENAME = 0x400,
        USEFILEATTRIBUTES = 0x10
    }

    public enum EnumFolderType
    {
        Open,
        Closed
    }

    public enum EnumIconSize
    {
        Large,
        Small
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ShellFileInfo
    {
        public const int conNameSize = 80;
        public IntPtr hIcon;
        public int iIndex;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }
}

