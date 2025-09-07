using BaddieFs.passthrough;
using Fsp;
using Fsp.Interop;
using System.Collections.Concurrent;
using System.Security.AccessControl;

namespace BaddieFs
{

    internal sealed class BaddieFs : Ptfs
    {
        private readonly long _degradataionStartsAt;

        private readonly int _minMs;

        private readonly int _maxMs;

        private readonly Random _rand = Random.Shared;

        // Map methods to strings and store their count
        private readonly ConcurrentDictionary<string, int> _operationalStats;
        // used to create a quick copy of the stats for printing
        private readonly object _statsLock = new Object();

        #region method names
        private const string METHOD_CAN_DELETE = "CanDelete";
        private const string METHOD_CLEANUP = "Cleanup";
        private const string METHOD_CLOSE = "Close";
        private const string METHOD_CREATE = "Create";
        private const string METHOD_EXCEPTION_HANDLER = "ExceptionHandler";
        private const string METHOD_FLUSH = "Flush";
        private const string METHOD_GET_FILE_INFO = "GetFileInfo";
        private const string METHOD_GET_SECURITY = "GetSecurity";
        private const string METHOD_GET_SECURITY_BY_NAME = "GetSecurityByName";
        private const string METHOD_GET_VOLUME_INFO = "GetVolumeInfo";
        private const string METHOD_INIT = "Init";
        private const string METHOD_MOUNTED = "Mounted";
        private const string METHOD_OPEN = "Open";
        private const string METHOD_OVERWRITE = "Overwrite";
        private const string METHOD_READ = "Read";
        private const string METHOD_READ_DIRECTORY_ENTRY = "ReadDirectoryEntry";
        private const string METHOD_RENAME = "Rename";
        private const string METHOD_SET_BASIC_INFO = "SetBasicInfo";
        private const string METHOD_SET_FILE_SIZE = "SetFileSize";
        private const string METHOD_SET_SECURITY = "SetSecurity";
        private const string METHOD_WRITE = "Write";
        #endregion

        public BaddieFs(string mirrorDir, TimeSpan timeSpan, int minDegradationMs, int maxDegradationMs) : base(mirrorDir)
        {
            _degradataionStartsAt = DateTime.Now.Add(timeSpan).Ticks;
            _minMs = minDegradationMs;
            _maxMs = maxDegradationMs;
            _operationalStats = new ConcurrentDictionary<string, int>();
        }

        public override int Mounted(object Host)
        {
            PrintStatsPeriodically();
            AddStat(METHOD_MOUNTED);
            return 0;
        }

        private void AddStat(string methodName) => _operationalStats.AddOrUpdate(methodName, 1, (key, oldValue) => oldValue + 1);

        private void PrintStatsPeriodically()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    // Copy the stats to avoid locking during printing
                    ConcurrentDictionary<string, int> statsCopy;
                    lock (_statsLock)
                    {
                        statsCopy = new ConcurrentDictionary<string, int>(_operationalStats);
                    }

                    Console.WriteLine($" ---- Operational Stats: {DateTime.Now} ----");
                    foreach (var kvp in statsCopy)
                    {
                        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                    Console.WriteLine(" ----------------------------");
                }
            });
        }

        public override int CanDelete(object FileNode, object FileDesc, string FileName)
        {
            AddStat(METHOD_CAN_DELETE);
            MaybeDegrade();
            return base.CanDelete(FileNode, FileDesc, FileName);
        }

        public override void Cleanup(object FileNode, object FileDesc, string FileName, uint Flags)
        {
            AddStat(METHOD_CLEANUP);
            MaybeDegrade();
            base.Cleanup(FileNode, FileDesc, FileName, Flags);
        }

        public override void Close(object FileNode, object FileDesc)
        {
            AddStat(METHOD_CLOSE);
            MaybeDegrade();
            base.Close(FileNode, FileDesc);
        }

        public override int Create(string FileName, uint CreateOptions, uint GrantedAccess, uint FileAttributes, byte[] SecurityDescriptor, ulong AllocationSize, out object FileNode, out object FileDesc, out Fsp.Interop.FileInfo FileInfo, out string NormalizedName)
        {
            AddStat(METHOD_CREATE);
            MaybeDegrade();
            return base.Create(FileName, CreateOptions, GrantedAccess, FileAttributes, SecurityDescriptor, AllocationSize, out FileNode, out FileDesc, out FileInfo, out NormalizedName);
        }

        public override int ExceptionHandler(Exception ex)
        {
            AddStat(METHOD_EXCEPTION_HANDLER);
            MaybeDegrade();
            return base.ExceptionHandler(ex);
        }

        public override int Flush(object FileNode, object FileDesc, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_FLUSH);
            MaybeDegrade();
            return base.Flush(FileNode, FileDesc, out FileInfo);
        }

        public override int GetFileInfo(object FileNode, object FileDesc, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_GET_FILE_INFO);
            MaybeDegrade();
            return base.GetFileInfo(FileNode, FileDesc, out FileInfo);
        }

        public override int GetSecurity(object FileNode, object FileDesc, ref byte[] SecurityDescriptor)
        {
            AddStat(METHOD_GET_SECURITY);
            MaybeDegrade();
            return base.GetSecurity(FileNode, FileDesc, ref SecurityDescriptor);
        }

        public override int GetSecurityByName(string FileName, out uint FileAttributes, ref byte[] SecurityDescriptor)
        {
            AddStat(METHOD_GET_SECURITY_BY_NAME);
            MaybeDegrade();
            return base.GetSecurityByName(FileName, out FileAttributes, ref SecurityDescriptor);
        }

        public override int GetVolumeInfo(out VolumeInfo VolumeInfo)
        {
            AddStat(METHOD_GET_VOLUME_INFO);
            MaybeDegrade();
            return base.GetVolumeInfo(out VolumeInfo);
        }

        public override int Init(object Host)
        {
            AddStat(METHOD_INIT);
            return base.Init(Host);
        }

        public override int Open(string FileName, uint CreateOptions, uint GrantedAccess, out object FileNode, out object FileDesc, out Fsp.Interop.FileInfo FileInfo, out string NormalizedName)
        {
            AddStat(METHOD_OPEN);
            MaybeDegrade();
            return base.Open(FileName, CreateOptions, GrantedAccess, out FileNode, out FileDesc, out FileInfo, out NormalizedName);
        }

        public override int Overwrite(object FileNode, object FileDesc, uint FileAttributes, bool ReplaceFileAttributes, ulong AllocationSize, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_OVERWRITE);
            MaybeDegrade();
            return base.Overwrite(FileNode, FileDesc, FileAttributes, ReplaceFileAttributes, AllocationSize, out FileInfo);
        }

        public override int Read(object FileNode, object FileDesc, nint Buffer, ulong Offset, uint Length, out uint BytesTransferred)
        {
            AddStat(METHOD_READ);
            MaybeDegrade();
            return base.Read(FileNode, FileDesc, Buffer, Offset, Length, out BytesTransferred);
        }

        public override bool ReadDirectoryEntry(object FileNode, object FileDesc, string Pattern, string Marker, ref object Context, out string FileName, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_READ_DIRECTORY_ENTRY);
            MaybeDegrade();
            return base.ReadDirectoryEntry(FileNode, FileDesc, Pattern, Marker, ref Context, out FileName, out FileInfo);
        }

        public override int Rename(object FileNode, object FileDesc, string FileName, string NewFileName, bool ReplaceIfExists)
        {
            AddStat(METHOD_RENAME);
            MaybeDegrade();
            return base.Rename(FileNode, FileDesc, FileName, NewFileName, ReplaceIfExists);
        }

        public override int SetBasicInfo(object FileNode, object FileDesc, uint FileAttributes, ulong CreationTime, ulong LastAccessTime, ulong LastWriteTime, ulong ChangeTime, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_SET_BASIC_INFO);
            MaybeDegrade();
            return base.SetBasicInfo(FileNode, FileDesc, FileAttributes, CreationTime, LastAccessTime, LastWriteTime, ChangeTime, out FileInfo);
        }

        public override int SetFileSize(object FileNode, object FileDesc, ulong NewSize, bool SetAllocationSize, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_SET_FILE_SIZE);
            MaybeDegrade();
            return base.SetFileSize(FileNode, FileDesc, NewSize, SetAllocationSize, out FileInfo);
        }

        public override int SetSecurity(object FileNode, object FileDesc, AccessControlSections Sections, byte[] SecurityDescriptor)
        {
            AddStat(METHOD_SET_SECURITY);
            return base.SetSecurity(FileNode, FileDesc, Sections, SecurityDescriptor);
        }

        public override int Write(object FileNode, object FileDesc, nint Buffer, ulong Offset, uint Length, bool WriteToEndOfFile, bool ConstrainedIo, out uint BytesTransferred, out Fsp.Interop.FileInfo FileInfo)
        {
            AddStat(METHOD_WRITE);
            MaybeDegrade();
            return base.Write(FileNode, FileDesc, Buffer, Offset, Length, WriteToEndOfFile, ConstrainedIo, out BytesTransferred, out FileInfo);
        }

        private void MaybeDegrade()
        {
            if (DateTime.Now.Ticks > _degradataionStartsAt)
            {
                int delay = _rand.Next(_minMs, _maxMs);
                Thread.Sleep(delay);
            }
        }
    }

    // Taken from PtfsService.cs in passthrough-dotnet and salvaged for my needs cutely
    class BaddieFsService : Service
    {
        private class CommandLineUsageException : Exception
        {
            public CommandLineUsageException(String Message = null) : base(Message)
            {
                HasMessage = null != Message;
            }

            public bool HasMessage;
        }

        private const String PROGNAME = "baddiefs";

        public BaddieFsService() : base("BaddieFsService")
        {
        }

        protected override void OnStart(String[] Args)
        {
            try
            {
                String DebugLogFile = null;
                UInt32 DebugFlags = 0;
                String VolumePrefix = null;
                String PassThrough = null;
                String MountPoint = null;
                IntPtr DebugLogHandle = (IntPtr)(-1);
                FileSystemHost Host = null;
                Ptfs Ptfs = null;
                int I;

                for (I = 1; Args.Length > I; I++)
                {
                    String Arg = Args[I];
                    if ('-' != Arg[0])
                        break;
                    switch (Arg[1])
                    {
                        case '?':
                            throw new CommandLineUsageException();
                        case 'd':
                            argtol(Args, ref I, ref DebugFlags);
                            break;
                        case 'D':
                            argtos(Args, ref I, ref DebugLogFile);
                            break;
                        case 'm':
                            argtos(Args, ref I, ref MountPoint);
                            break;
                        case 'p':
                            argtos(Args, ref I, ref PassThrough);
                            break;
                        case 'u':
                            argtos(Args, ref I, ref VolumePrefix);
                            break;
                        default:
                            throw new CommandLineUsageException();
                    }
                }

                if (Args.Length > I)
                    throw new CommandLineUsageException();

                if (null == PassThrough && null != VolumePrefix)
                {
                    I = VolumePrefix.IndexOf('\\');
                    if (-1 != I && VolumePrefix.Length > I && '\\' != VolumePrefix[I + 1])
                    {
                        I = VolumePrefix.IndexOf('\\', I + 1);
                        if (-1 != I &&
                            VolumePrefix.Length > I + 1 &&
                            (
                            ('A' <= VolumePrefix[I + 1] && VolumePrefix[I + 1] <= 'Z') ||
                            ('a' <= VolumePrefix[I + 1] && VolumePrefix[I + 1] <= 'z')
                            ) &&
                            '$' == VolumePrefix[I + 2])
                        {
                            PassThrough = String.Format("{0}:{1}", VolumePrefix[I + 1], VolumePrefix.Substring(I + 3));
                        }
                    }
                }

                if (null == PassThrough || null == MountPoint)
                    throw new CommandLineUsageException();

                if (null != DebugLogFile)
                    if (0 > FileSystemHost.SetDebugLogFile(DebugLogFile))
                        throw new CommandLineUsageException("cannot open debug log file");

                Host = new FileSystemHost(new BaddieFs(PassThrough, TimeSpan.FromHours(120), 5_000, 10_000));

                Host.Prefix = VolumePrefix;
                if (0 > Host.Mount(MountPoint, null, true, DebugFlags))
                    throw new IOException("cannot mount file system");
                MountPoint = Host.MountPoint();
                _Host = Host;

                Log(EVENTLOG_INFORMATION_TYPE, String.Format("{0}{1}{2} -p {3} -m {4}",
                    PROGNAME,
                    null != VolumePrefix && 0 < VolumePrefix.Length ? " -u " : "",
                        null != VolumePrefix && 0 < VolumePrefix.Length ? VolumePrefix : "",
                    PassThrough,
                    MountPoint));
            }
            catch (CommandLineUsageException ex)
            {
                Log(EVENTLOG_ERROR_TYPE, String.Format(
                    "{0}" +
                    "usage: {1} OPTIONS\n" +
                    "\n" +
                    "options:\n" +
                    "    -d DebugFlags       [-1: enable all debug logs]\n" +
                    "    -D DebugLogFile     [file path; use - for stderr]\n" +
                    "    -u \\Server\\Share    [UNC prefix (single backslash)]\n" +
                    "    -p Directory        [directory to expose as pass through file system]\n" +
                    "    -m MountPoint       [X:|*|directory]\n",
                    ex.HasMessage ? ex.Message + "\n" : "",
                    PROGNAME));
                throw;
            }
            catch (Exception ex)
            {
                Log(EVENTLOG_ERROR_TYPE, String.Format("{0}", ex.Message));
                throw;
            }
        }
        protected override void OnStop()
        {
            _Host.Unmount();
            _Host = null;
        }

        private static void argtos(String[] Args, ref int I, ref String V)
        {
            if (Args.Length > ++I)
                V = Args[I];
            else
                throw new CommandLineUsageException();
        }

        private static void argtol(String[] Args, ref int I, ref UInt32 V)
        {
            Int32 R;
            if (Args.Length > ++I)
                V = Int32.TryParse(Args[I], out R) ? (UInt32)R : V;
            else
                throw new CommandLineUsageException();
        }

        private FileSystemHost _Host;
    }
}
