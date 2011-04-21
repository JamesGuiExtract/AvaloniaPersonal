using System.Diagnostics.CodeAnalysis;

namespace Extract
{
    /// <summary>
    /// Enumeration of common windows system error codes.
    /// </summary>
    public enum Win32ErrorCode
    {
        ///<summary>
        /// Success
        ///</summary>
        Success = 0,

        ///<summary>
        /// InvalidFunction
        ///</summary>
        InvalidFunction = 1,

        ///<summary>
        /// FileNotFound
        ///</summary>
        FileNotFound = 2,

        ///<summary>
        /// PathNotFound
        ///</summary>
        PathNotFound = 3,

        ///<summary>
        /// TooManyOpenFiles
        ///</summary>
        TooManyOpenFiles = 4,

        ///<summary>
        /// AccessDenied
        ///</summary>
        AccessDenied = 5,

        ///<summary>
        /// InvalidHandle
        ///</summary>
        InvalidHandle = 6,

        ///<summary>
        /// ArenaTrashed
        ///</summary>
        ArenaTrashed = 7,

        ///<summary>
        /// NotEnoughMemory
        ///</summary>
        NotEnoughMemory = 8,

        ///<summary>
        /// InvalidBlock
        ///</summary>
        InvalidBlock = 9,

        ///<summary>
        /// BadEnvironment
        ///</summary>
        BadEnvironment = 10,

        ///<summary>
        /// BadFormat
        ///</summary>
        BadFormat = 11,

        ///<summary>
        /// InvalidAccess
        ///</summary>
        InvalidAccess = 12,

        ///<summary>
        /// InvalidData
        ///</summary>
        InvalidData = 13,

        ///<summary>
        /// Outofmemory
        ///</summary>
        OutOfMemory = 14,

        ///<summary>
        /// InvalidDrive
        ///</summary>
        InvalidDrive = 15,

        ///<summary>
        /// CurrentDirectory
        ///</summary>
        CurrentDirectory = 16,

        ///<summary>
        /// NotSameDevice
        ///</summary>
        NotSameDevice = 17,

        ///<summary>
        /// NoMoreFiles
        ///</summary>
        NoMoreFiles = 18,

        ///<summary>
        /// WriteProtect
        ///</summary>
        WriteProtect = 19,

        ///<summary>
        /// BadUnit
        ///</summary>
        BadUnit = 20,

        ///<summary>
        /// NotReady
        ///</summary>
        NotReady = 21,

        ///<summary>
        /// BadCommand
        ///</summary>
        BadCommand = 22,

        ///<summary>
        /// Crc
        ///</summary>
        CyclicRedundancyCheck = 23,

        ///<summary>
        /// BadLength
        ///</summary>
        BadLength = 24,

        ///<summary>
        /// Seek
        ///</summary>
        Seek = 25,

        ///<summary>
        /// NotDosDisk
        ///</summary>
        NotDosDisk = 26,

        ///<summary>
        /// SectorNotFound
        ///</summary>
        SectorNotFound = 27,

        ///<summary>
        /// OutOfPaper
        ///</summary>
        OutOfPaper = 28,

        ///<summary>
        /// WriteFault
        ///</summary>
        WriteFault = 29,

        ///<summary>
        /// ReadFault
        ///</summary>
        ReadFault = 30,

        ///<summary>
        /// GeneralFailure
        ///</summary>
        GeneralFailure = 31,

        ///<summary>
        /// SharingViolation
        ///</summary>
        SharingViolation = 32,

        ///<summary>
        /// LockViolation
        ///</summary>
        LockViolation = 33,

        ///<summary>
        /// WrongDisk
        ///</summary>
        WrongDisk = 34,

        ///<summary>
        /// SharingBufferExceeded
        ///</summary>
        SharingBufferExceeded = 36,

        ///<summary>
        /// HandleEof
        ///</summary>
        HandleEndOfFile = 38,

        ///<summary>
        /// HandleDiskFull
        ///</summary>
        HandleDiskFull = 39,

        ///<summary>
        /// NotSupported
        ///</summary>
        NotSupported = 50,

        ///<summary>
        /// RemNotList
        ///</summary>
        RemNotList = 51,

        ///<summary>
        /// DupName
        ///</summary>
        DupName = 52,

        ///<summary>
        /// BadNetpath
        ///</summary>
        BadNetworkPath = 53,

        ///<summary>
        /// NetworkBusy
        ///</summary>
        NetworkBusy = 54,

        ///<summary>
        /// DevNotExist
        ///</summary>
        DevNotExist = 55,

        ///<summary>
        /// TooManyCommands
        ///</summary>
        TooManyCommands = 56,

        ///<summary>
        /// AdapHdwErr
        ///</summary>
        AdapterHardwareError = 57,

        ///<summary>
        /// BadNetResp
        ///</summary>
        BadNetworkResponse = 58,

        ///<summary>
        /// UnexpNetErr
        ///</summary>
        UnexpectedNetworkError = 59,

        ///<summary>
        /// BademAdap
        ///</summary>
        BadEmAdapter = 60,

        ///<summary>
        /// PrintQueueFull
        ///</summary>
        PrintQueueFull = 61,

        ///<summary>
        /// NoSpoolSpace
        ///</summary>
        NoSpoolSpace = 62,

        ///<summary>
        /// PrintCancelled
        ///</summary>
        PrintCanceled = 63,

        ///<summary>
        /// NetnameDeleted
        ///</summary>
        NetworkNameDeleted = 64,

        ///<summary>
        /// NetworkAccessDenied
        ///</summary>
        NetworkAccessDenied = 65,

        ///<summary>
        /// BadDevType
        ///</summary>
        BadDevType = 66,

        ///<summary>
        /// BadNetName
        ///</summary>
        BadNetName = 67,

        ///<summary>
        /// TooManyNames
        ///</summary>
        TooManyNames = 68,

        ///<summary>
        /// TooManySess
        ///</summary>
        TooManySessions = 69,

        ///<summary>
        /// SharingPaused
        ///</summary>
        SharingPaused = 70,

        ///<summary>
        /// ReqNotAccep
        ///</summary>
        RequestNotAccepted = 71,

        ///<summary>
        /// RedirPaused
        ///</summary>
        RedirectionPaused = 72,

        ///<summary>
        /// FileExists
        ///</summary>
        FileExists = 80,

        ///<summary>
        /// CannotMake
        ///</summary>
        CannotMake = 82,

        ///<summary>
        /// FailI24
        ///</summary>
        FailI24 = 83,

        ///<summary>
        /// OutOfStructures
        ///</summary>
        OutOfStructures = 84,

        ///<summary>
        /// AlreadyAssigned
        ///</summary>
        AlreadyAssigned = 85,

        ///<summary>
        /// InvalidPassword
        ///</summary>
        InvalidPassword = 86,

        ///<summary>
        /// InvalidParameter
        ///</summary>
        InvalidParameter = 87,

        ///<summary>
        /// NetWriteFault
        ///</summary>
        NetWriteFault = 88,

        ///<summary>
        /// NoProcSlots
        ///</summary>
        NoProcSlots = 89,

        ///<summary>
        /// TooManySemaphores
        ///</summary>
        TooManySemaphores = 100,

        ///<summary>
        /// ExclSemAlreadyOwned
        ///</summary>
        ExclusiveSemaphoreAlreadyOwned = 101,

        ///<summary>
        /// SemaphoreIsSet
        ///</summary>
        SemaphoreIsSet = 102,

        ///<summary>
        /// TooManySemaphoreRequests
        ///</summary>
        TooManySemaphoreRequests = 103,

        ///<summary>
        /// InvalidAtInterruptTime
        ///</summary>
        InvalidAtInterruptTime = 104,

        ///<summary>
        /// SemaphoreOwnerDied
        ///</summary>
        SemaphoreOwnerDied = 105,

        ///<summary>
        /// SemaphoreUserLimit
        ///</summary>
        SemaphoreUserLimit = 106,

        ///<summary>
        /// DiskChange
        ///</summary>
        DiskChange = 107,

        ///<summary>
        /// DriveLocked
        ///</summary>
        DriveLocked = 108,

        ///<summary>
        /// BrokenPipe
        ///</summary>
        BrokenPipe = 109,

        ///<summary>
        /// OpenFailed
        ///</summary>
        OpenFailed = 110,

        ///<summary>
        /// BufferOverflow
        ///</summary>
        BufferOverflow = 111,

        ///<summary>
        /// DiskFull
        ///</summary>
        DiskFull = 112,

        ///<summary>
        /// NoMoreSearchHandles
        ///</summary>
        NoMoreSearchHandles = 113,

        ///<summary>
        /// InvalidTargetHandle
        ///</summary>
        InvalidTargetHandle = 114,

        ///<summary>
        /// InvalidCategory
        ///</summary>
        InvalidCategory = 117,

        ///<summary>
        /// InvalidVerifySwitch
        ///</summary>
        InvalidVerifySwitch = 118,

        ///<summary>
        /// BadDriverLevel
        ///</summary>
        BadDriverLevel = 119,

        ///<summary>
        /// CallNotImplemented
        ///</summary>
        CallNotImplemented = 120,

        ///<summary>
        /// SemaphoreTimeout
        ///</summary>
        SemaphoreTimeout = 121,

        ///<summary>
        /// InsufficientBuffer
        ///</summary>
        InsufficientBuffer = 122,

        ///<summary>
        /// InvalidName
        ///</summary>
        InvalidName = 123,

        ///<summary>
        /// InvalidLevel
        ///</summary>
        InvalidLevel = 124,

        ///<summary>
        /// NoVolumeLabel
        ///</summary>
        NoVolumeLabel = 125,

        ///<summary>
        /// ModNotFound
        ///</summary>
        ModNotFound = 126,

        ///<summary>
        /// ProcessNotFound
        ///</summary>
        ProcessNotFound = 127,

        ///<summary>
        /// WaitNoChildren
        ///</summary>
        WaitNoChildren = 128,

        ///<summary>
        /// ChildNotComplete
        ///</summary>
        ChildNotComplete = 129,

        ///<summary>
        /// DirectAccessHandle
        ///</summary>
        DirectAccessHandle = 130,

        ///<summary>
        /// NegativeSeek
        ///</summary>
        NegativeSeek = 131,

        ///<summary>
        /// SeekOnDevice
        ///</summary>
        SeekOnDevice = 132,

        ///<summary>
        /// IsJoinTarget
        ///</summary>
        IsJoinTarget = 133,

        ///<summary>
        /// IsJoined
        ///</summary>
        IsJoined = 134,

        ///<summary>
        /// IsSubsted
        ///</summary>
        IsSubstituted = 135,

        ///<summary>
        /// NotJoined
        ///</summary>
        NotJoined = 136,

        ///<summary>
        /// NotSubsted
        ///</summary>
        NotSubstituted = 137,

        ///<summary>
        /// JoinToJoin
        ///</summary>
        JoinToJoin = 138,

        ///<summary>
        /// SubstToSubst
        ///</summary>
        SubstituteToSubstituted = 139,

        ///<summary>
        /// JoinToSubst
        ///</summary>
        JoinToSubstitute = 140,

        ///<summary>
        /// SubstToJoin
        ///</summary>
        SubstitutedToJoin = 141,

        ///<summary>
        /// BusyDrive
        ///</summary>
        BusyDrive = 142,

        ///<summary>
        /// SameDrive
        ///</summary>
        SameDrive = 143,

        ///<summary>
        /// DirNotRoot
        ///</summary>
        DirNotRoot = 144,

        ///<summary>
        /// DirNotEmpty
        ///</summary>
        DirNotEmpty = 145,

        ///<summary>
        /// IsSubstPath
        ///</summary>
        IsSubstitutePath = 146,

        ///<summary>
        /// IsJoinPath
        ///</summary>
        IsJoinPath = 147,

        ///<summary>
        /// PathBusy
        ///</summary>
        PathBusy = 148,

        ///<summary>
        /// IsSubstTarget
        ///</summary>
        IsSubstituteTarget = 149,

        ///<summary>
        /// SystemTrace
        ///</summary>
        SystemTrace = 150,

        ///<summary>
        /// InvalidEventCount
        ///</summary>
        InvalidEventCount = 151,

        ///<summary>
        /// TooManyMutexWaiters
        ///</summary>
        TooManyMutexWaiters = 152,

        ///<summary>
        /// InvalidListFormat
        ///</summary>
        InvalidListFormat = 153,

        ///<summary>
        /// LabelTooLong
        ///</summary>
        LabelTooLong = 154,

        ///<summary>
        /// TooManyTcbs
        ///</summary>
        TooManyTaskControlBlocks = 155,

        ///<summary>
        /// SignalRefused
        ///</summary>
        SignalRefused = 156,

        ///<summary>
        /// Discarded
        ///</summary>
        Discarded = 157,

        ///<summary>
        /// NotLocked
        ///</summary>
        NotLocked = 158,

        ///<summary>
        /// BadThreadIdAddress
        ///</summary>
        BadThreadIdAddress = 159,

        ///<summary>
        /// BadArguments
        ///</summary>
        BadArguments = 160,

        ///<summary>
        /// BadPathname
        ///</summary>
        BadPathName = 161,

        ///<summary>
        /// SignalPending
        ///</summary>
        SignalPending = 162,

        ///<summary>
        /// MaxThreadsReached
        ///</summary>
        MaxThreadsReached = 164,

        ///<summary>
        /// LockFailed
        ///</summary>
        LockFailed = 167,

        ///<summary>
        /// Busy
        ///</summary>
        Busy = 170,

        ///<summary>
        /// CancelViolation
        ///</summary>
        CancelViolation = 173,

        ///<summary>
        /// AtomicLocksNotSupported
        ///</summary>
        AtomicLocksNotSupported = 174,

        ///<summary>
        /// InvalidSegmentNumber
        ///</summary>
        InvalidSegmentNumber = 180,

        ///<summary>
        /// InvalidOrdinal
        ///</summary>
        InvalidOrdinal = 182,

        ///<summary>
        /// AlreadyExists
        ///</summary>
        AlreadyExists = 183,

        ///<summary>
        /// InvalidFlagNumber
        ///</summary>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId="Flag")]
        InvalidFlagNumber = 186,

        ///<summary>
        /// SemaphoreNotFound
        ///</summary>
        SemaphoreNotFound = 187,

        ///<summary>
        /// InvalidStartingCodeseg
        ///</summary>
        InvalidStartingCodeSegment = 188,

        ///<summary>
        /// InvalidStackseg
        ///</summary>
        InvalidStackSegment = 189,

        ///<summary>
        /// InvalidModuletype
        ///</summary>
        InvalidModuleType = 190,

        ///<summary>
        /// InvalidExeSignature
        ///</summary>
        InvalidExeSignature = 191,

        ///<summary>
        /// ExeMarkedInvalid
        ///</summary>
        ExeMarkedInvalid = 192,

        ///<summary>
        /// BadExeFormat
        ///</summary>
        BadExeFormat = 193,

        ///<summary>
        /// IteratedDataExceeds64k
        ///</summary>
        IteratedDataExceeds64K = 194,

        ///<summary>
        /// InvalidMinallocsize
        ///</summary>
        InvalidMinimumAllocationSize = 195,

        ///<summary>
        /// DynlinkFromInvalidRing
        ///</summary>
        DynamicLinkFromInvalidRing = 196,

        ///<summary>
        /// IoplNotEnabled
        ///</summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Iopl")]
        IoplNotEnabled = 197,

        ///<summary>
        /// InvalidSegdpl
        ///</summary>
        InvalidSegmentDpl = 198,

        ///<summary>
        /// AutodatasegExceeds64k
        ///</summary>
        AutoDataSegmentExceeds64K = 199,

        ///<summary>
        /// Ring2SegMustBeMovable
        ///</summary>
        Ring2SegmentMustBeMovable = 200,

        ///<summary>
        /// RelocChainXeedsSeglim
        ///</summary>
        RelocationChainExceedsSegmentLimit = 201,

        ///<summary>
        /// InfloopInRelocChain
        ///</summary>
        InfiniteLoopInRelocationChain = 202,

        ///<summary>
        /// EnvironmentVariableNotFound
        ///</summary>
        EnvironmentVariableNotFound = 203,

        ///<summary>
        /// NoSignalSent
        ///</summary>
        NoSignalSent = 205,

        ///<summary>
        /// FileNameExceedsRange
        ///</summary>
        FileNameExceedsRange = 206,

        ///<summary>
        /// Ring2StackInUse
        ///</summary>
        Ring2StackInUse = 207,

        ///<summary>
        /// MetaExpansionTooLong
        ///</summary>
        MetaExpansionTooLong = 208,

        ///<summary>
        /// InvalidSignalNumber
        ///</summary>
        InvalidSignalNumber = 209,

        ///<summary>
        /// Thread1Inactive
        ///</summary>
        Thread1Inactive = 210,

        ///<summary>
        /// Locked
        ///</summary>
        Locked = 212,

        ///<summary>
        /// TooManyModules
        ///</summary>
        TooManyModules = 214,

        ///<summary>
        /// NestingNotAllowed
        ///</summary>
        NestingNotAllowed = 215,

        ///<summary>
        /// ExeMachineTypeMismatch
        ///</summary>
        ExeMachineTypeMismatch = 216,

        ///<summary>
        /// ExeCannotModifySignedBinary
        ///</summary>
        ExeCannotModifySignedBinary = 217,

        ///<summary>
        /// ExeCannotModifyStrongSignedBinary
        ///</summary>
        ExeCannotModifyStrongSignedBinary = 218,

        ///<summary>
        /// FileCheckedOut
        ///</summary>
        FileCheckedOut = 220,

        ///<summary>
        /// CheckoutRequired
        ///</summary>
        CheckoutRequired = 221,

        ///<summary>
        /// BadFileType
        ///</summary>
        BadFileType = 222,

        ///<summary>
        /// FileTooLarge
        ///</summary>
        FileTooLarge = 223,

        ///<summary>
        /// FormsAuthRequired
        ///</summary>
        FormsAuthRequired = 224,

        ///<summary>
        /// VirusInfected
        ///</summary>
        VirusInfected = 225,

        ///<summary>
        /// VirusDeleted
        ///</summary>
        VirusDeleted = 226,

        ///<summary>
        /// PipeLocal
        ///</summary>
        PipeLocal = 229,

        ///<summary>
        /// BadPipe
        ///</summary>
        BadPipe = 230,

        ///<summary>
        /// PipeBusy
        ///</summary>
        PipeBusy = 231,

        ///<summary>
        /// NoData
        ///</summary>
        NoData = 232,

        ///<summary>
        /// PipeNotConnected
        ///</summary>
        PipeNotConnected = 233,

        ///<summary>
        /// MoreData
        ///</summary>
        MoreData = 234,

        ///<summary>
        /// VcDisconnected
        ///</summary>
        VCDisconnected = 240,

        ///<summary>
        /// InvalidEaName
        ///</summary>
        InvalidEAName = 254,

        ///<summary>
        /// EaListInconsistent
        ///</summary>
        EAListInconsistent = 255,

        ///<summary>
        /// WaitTimeout
        ///</summary>
        WaitTimeout = 258,

        ///<summary>
        /// NoMoreItems
        ///</summary>
        NoMoreItems = 259,

        ///<summary>
        /// CannotCopy
        ///</summary>
        CannotCopy = 266,

        ///<summary>
        /// Directory
        ///</summary>
        Directory = 267,

        ///<summary>
        /// EasDidntFit
        ///</summary>
        ExtendedAttributesDidNotFit = 275,

        ///<summary>
        /// EaFileCorrupt
        ///</summary>
        EAFileCorrupt = 276,

        ///<summary>
        /// EaTableFull
        ///</summary>
        EATableFull = 277,

        ///<summary>
        /// InvalidEaHandle
        ///</summary>
        InvalidEAHandle = 278,

        ///<summary>
        /// EasNotSupported
        ///</summary>
        ExtendedAttributesNotSupported = 282,

        ///<summary>
        /// NotOwner
        ///</summary>
        NotOwner = 288,

        ///<summary>
        /// TooManyPosts
        ///</summary>
        TooManyPosts = 298,

        ///<summary>
        /// PartialCopy
        ///</summary>
        PartialCopy = 299,

        ///<summary>
        /// OplockNotGranted
        ///</summary>
        OpLockNotGranted = 300,

        ///<summary>
        /// InvalidOplockProtocol
        ///</summary>
        InvalidOpLockProtocol = 301,

        ///<summary>
        /// DiskTooFragmented
        ///</summary>
        DiskTooFragmented = 302,

        ///<summary>
        /// DeletePending
        ///</summary>
        DeletePending = 303,

        ///<summary>
        /// IncompatibleWithGlobalShortNameRegistrySetting
        ///</summary>
        IncompatibleWithGlobalShortNameRegistrySetting = 304,

        ///<summary>
        /// ShortNamesNotEnabledOnVolume
        ///</summary>
        ShortNamesNotEnabledOnVolume = 305,

        ///<summary>
        /// SecurityStreamIsInconsistent
        ///</summary>
        SecurityStreamIsInconsistent = 306,

        ///<summary>
        /// InvalidLockRange
        ///</summary>
        InvalidLockRange = 307,

        ///<summary>
        /// ImageSubsystemNotPresent
        ///</summary>
        ImageSubsystemNotPresent = 308,

        ///<summary>
        /// NotificationGuidAlreadyDefined
        ///</summary>
        NotificationGuidAlreadyDefined = 309,

        ///<summary>
        /// MrMidNotFound
        ///</summary>
        MessageRecordForMessageIdNotFound = 317,

        ///<summary>
        /// ScopeNotFound
        ///</summary>
        ScopeNotFound = 318,

        ///<summary>
        /// FailNoactionReboot
        ///</summary>
        FailNoActionReboot = 350,

        ///<summary>
        /// FailShutdown
        ///</summary>
        FailShutdown = 351,

        ///<summary>
        /// FailRestart
        ///</summary>
        FailRestart = 352,

        ///<summary>
        /// MaxSessionsReached
        ///</summary>
        MaxSessionsReached = 353,

        ///<summary>
        /// ThreadModeAlreadyBackground
        ///</summary>
        ThreadModeAlreadyBackground = 400,

        ///<summary>
        /// ThreadModeNotBackground
        ///</summary>
        ThreadModeNotBackground = 401,

        ///<summary>
        /// ProcessModeAlreadyBackground
        ///</summary>
        ProcessModeAlreadyBackground = 402,

        ///<summary>
        /// ProcessModeNotBackground
        ///</summary>
        ProcessModeNotBackground = 403,

        /// <summary>
        /// InvalidAddress
        /// </summary>
        InvalidAddress = 487
    }

    /// <summary>
    /// Extension method class for the win 32 error codes
    /// </summary>
    public static class Win32ErrorCodeExtensions
    {
        /// <summary>
        /// Extension method to convert a <see cref="Win32ErrorCode"/> into a string.
        /// </summary>
        /// <param name="errorCode">The error code to convert.</param>
        /// <returns>A string for the error code.</returns>
        public static string ToString(this Win32ErrorCode errorCode)
        {
            switch (errorCode)
            {
                case Win32ErrorCode.Success: return "The operation completed successfully.";
                case Win32ErrorCode.InvalidFunction: return "Incorrect function.";
                case Win32ErrorCode.BadEnvironment: return "The environment is incorrect.";
                case Win32ErrorCode.TooManySemaphores: return "Cannot create another system semaphore.";
                case Win32ErrorCode.ExclusiveSemaphoreAlreadyOwned: return "The exclusive semaphore is owned by another process.";
                case Win32ErrorCode.SemaphoreIsSet: return "The semaphore is set and cannot be closed.";
                case Win32ErrorCode.TooManySemaphoreRequests: return "The semaphore cannot be set again.";
                case Win32ErrorCode.InvalidAtInterruptTime: return "Cannot request exclusive semaphores at interrupt time.";
                case Win32ErrorCode.SemaphoreOwnerDied: return "The previous ownership of this semaphore has ended.";
                case Win32ErrorCode.SemaphoreUserLimit: return "Insert the diskette for drive %1.";
                case Win32ErrorCode.DiskChange: return "The program stopped because an alternate diskette was not inserted.";
                case Win32ErrorCode.DriveLocked: return "The disk is in use or locked by another process.";
                case Win32ErrorCode.BrokenPipe: return "The pipe has been ended.";
                case Win32ErrorCode.BadFormat: return "An attempt was made to load a program with an incorrect format.";
                case Win32ErrorCode.OpenFailed: return "The system cannot open the device or file specified.";
                case Win32ErrorCode.BufferOverflow: return "The file name is too long.";
                case Win32ErrorCode.DiskFull: return "There is not enough space on the disk.";
                case Win32ErrorCode.NoMoreSearchHandles: return "No more internal file identifiers available.";
                case Win32ErrorCode.InvalidTargetHandle: return "The target internal file identifier is incorrect.";
                case Win32ErrorCode.InvalidCategory: return "The IOCTL call made by the application program is not correct.";
                case Win32ErrorCode.InvalidVerifySwitch: return "The verify-on-write switch parameter value is not correct.";
                case Win32ErrorCode.BadDriverLevel: return "The system does not support the command requested.";
                case Win32ErrorCode.InvalidAccess: return "The access code is invalid.";
                case Win32ErrorCode.CallNotImplemented: return "This function is not supported on this system.";
                case Win32ErrorCode.SemaphoreTimeout: return "The semaphore timeout period has expired.";
                case Win32ErrorCode.InsufficientBuffer: return "The data area passed to a system call is too small.";
                case Win32ErrorCode.InvalidName: return "The filename, directory name, or volume label syntax is incorrect.";
                case Win32ErrorCode.InvalidLevel: return "The system call level is not correct.";
                case Win32ErrorCode.NoVolumeLabel: return "The disk has no volume label.";
                case Win32ErrorCode.ModNotFound: return "The specified module could not be found.";
                case Win32ErrorCode.ProcessNotFound: return "The specified procedure could not be found.";
                case Win32ErrorCode.WaitNoChildren: return "There are no child processes to wait for.";
                case Win32ErrorCode.ChildNotComplete: return "The %1 application cannot be run in Win32 mode.";
                case Win32ErrorCode.InvalidData: return "The data is invalid.";
                case Win32ErrorCode.DirectAccessHandle: return "Attempt to use a file handle to an open disk partition for an operation other than raw disk I/O.";
                case Win32ErrorCode.NegativeSeek: return "An attempt was made to move the file pointer before the beginning of the file.";
                case Win32ErrorCode.SeekOnDevice: return "The file pointer cannot be set on the specified device or file.";
                case Win32ErrorCode.IsJoinTarget: return "A JOIN or SUBST command cannot be used for a drive that contains previously joined drives.";
                case Win32ErrorCode.IsJoined: return "An attempt was made to use a JOIN or SUBST command on a drive that has already been joined.";
                case Win32ErrorCode.IsSubstituted: return "An attempt was made to use a JOIN or SUBST command on a drive that has already been substituted.";
                case Win32ErrorCode.NotJoined: return "The system tried to delete the JOIN of a drive that is not joined.";
                case Win32ErrorCode.NotSubstituted: return "The system tried to delete the substitution of a drive that is not substituted.";
                case Win32ErrorCode.JoinToJoin: return "The system tried to join a drive to a directory on a joined drive.";
                case Win32ErrorCode.SubstituteToSubstituted: return "The system tried to substitute a drive to a directory on a substituted drive.";
                case Win32ErrorCode.OutOfMemory: return "Not enough storage is available to complete this operation.";
                case Win32ErrorCode.JoinToSubstitute: return "The system tried to join a drive to a directory on a substituted drive.";
                case Win32ErrorCode.SubstitutedToJoin: return "The system tried to SUBST a drive to a directory on a joined drive.";
                case Win32ErrorCode.BusyDrive: return "The system cannot perform a JOIN or SUBST at this time.";
                case Win32ErrorCode.SameDrive: return "The system cannot join or substitute a drive to or for a directory on the same drive.";
                case Win32ErrorCode.DirNotRoot: return "The directory is not a subdirectory of the root directory.";
                case Win32ErrorCode.DirNotEmpty: return "The directory is not empty.";
                case Win32ErrorCode.IsSubstitutePath: return "The path specified is being used in a substitute.";
                case Win32ErrorCode.IsJoinPath: return "Not enough resources are available to process this command.";
                case Win32ErrorCode.PathBusy: return "The path specified cannot be used at this time.";
                case Win32ErrorCode.IsSubstituteTarget: return "An attempt was made to join or substitute a drive for which a directory on the drive is the target of a previous substitute.";
                case Win32ErrorCode.InvalidDrive: return "The system cannot find the drive specified.";
                case Win32ErrorCode.SystemTrace: return "System trace information was not specified in your CONFIG.SYS file, or tracing is disallowed.";
                case Win32ErrorCode.InvalidEventCount: return "The number of specified semaphore events for DosMuxSemWait is not correct.";
                case Win32ErrorCode.TooManyMutexWaiters: return "DosMuxSemWait did not execute; too many semaphores are already set.";
                case Win32ErrorCode.InvalidListFormat: return "The DosMuxSemWait list is not correct.";
                case Win32ErrorCode.LabelTooLong: return "The volume label you entered exceeds the label character limit of the target file system.";
                case Win32ErrorCode.TooManyTaskControlBlocks: return "Cannot create another thread.";
                case Win32ErrorCode.SignalRefused: return "The recipient process has refused the signal.";
                case Win32ErrorCode.Discarded: return "The segment is already discarded and cannot be locked.";
                case Win32ErrorCode.NotLocked: return "The segment is already unlocked.";
                case Win32ErrorCode.BadThreadIdAddress: return "The address for the thread ID is not correct.";
                case Win32ErrorCode.CurrentDirectory: return "The directory cannot be removed.";
                case Win32ErrorCode.BadArguments: return "One or more arguments are not correct.";
                case Win32ErrorCode.BadPathName: return "The specified path is invalid.";
                case Win32ErrorCode.SignalPending: return "A signal is already pending.";
                case Win32ErrorCode.MaxThreadsReached: return "No more threads can be created in the system.";
                case Win32ErrorCode.LockFailed: return "Unable to lock a region of a file.";
                case Win32ErrorCode.NotSameDevice: return "The system cannot move the file to a different disk drive.";
                case Win32ErrorCode.Busy: return "The requested resource is in use.";
                case Win32ErrorCode.CancelViolation: return "A lock request was not outstanding for the supplied cancel region.";
                case Win32ErrorCode.AtomicLocksNotSupported: return "The file system does not support atomic changes to the lock type.";
                case Win32ErrorCode.NoMoreFiles: return "There are no more files.";
                case Win32ErrorCode.InvalidSegmentNumber: return "The system detected a segment number that was not correct.";
                case Win32ErrorCode.InvalidOrdinal: return "The operating system cannot run %1.";
                case Win32ErrorCode.AlreadyExists: return "Cannot create a file when that file already exists.";
                case Win32ErrorCode.InvalidFlagNumber: return "The flag passed is not correct.";
                case Win32ErrorCode.SemaphoreNotFound: return "The specified system semaphore name was not found.";
                case Win32ErrorCode.InvalidStartingCodeSegment: return "The operating system cannot run %1.";
                case Win32ErrorCode.InvalidStackSegment: return "The operating system cannot run %1.";
                case Win32ErrorCode.WriteProtect: return "The media is write protected.";
                case Win32ErrorCode.InvalidModuleType: return "The operating system cannot run %1.";
                case Win32ErrorCode.InvalidExeSignature: return "Cannot run %1 in Win32 mode.";
                case Win32ErrorCode.ExeMarkedInvalid: return "The operating system cannot run %1.";
                case Win32ErrorCode.BadExeFormat: return "is not a valid Win32 application.";
                case Win32ErrorCode.IteratedDataExceeds64K: return "The operating system cannot run %1.";
                case Win32ErrorCode.InvalidMinimumAllocationSize: return "The operating system cannot run %1.";
                case Win32ErrorCode.DynamicLinkFromInvalidRing: return "The operating system cannot run this application program.";
                case Win32ErrorCode.IoplNotEnabled: return "The operating system is not presently configured to run this application.";
                case Win32ErrorCode.InvalidSegmentDpl: return "The operating system cannot run %1.";
                case Win32ErrorCode.AutoDataSegmentExceeds64K: return "The operating system cannot run this application program.";
                case Win32ErrorCode.FileNotFound: return "The system cannot find the file specified.";
                case Win32ErrorCode.BadUnit: return "The system cannot find the device specified.";
                case Win32ErrorCode.Ring2SegmentMustBeMovable: return "The code segment cannot be greater than or equal to 64K.";
                case Win32ErrorCode.RelocationChainExceedsSegmentLimit: return "The operating system cannot run %1.";
                case Win32ErrorCode.InfiniteLoopInRelocationChain: return "The operating system cannot run %1.";
                case Win32ErrorCode.EnvironmentVariableNotFound: return "The system could not find the environment option that was entered.";
                case Win32ErrorCode.NoSignalSent: return "No process in the command subtree has a signal handler.";
                case Win32ErrorCode.FileNameExceedsRange: return "The filename or extension is too long.";
                case Win32ErrorCode.Ring2StackInUse: return "The ring 2 stack is in use.";
                case Win32ErrorCode.MetaExpansionTooLong: return "The global filename characters, * or ?, are entered incorrectly or too many global filename characters are specified.";
                case Win32ErrorCode.InvalidSignalNumber: return "The signal being posted is not correct.";
                case Win32ErrorCode.NotReady: return "The device is not ready.";
                case Win32ErrorCode.Thread1Inactive: return "The signal handler cannot be set.";
                case Win32ErrorCode.Locked: return "The segment is locked and cannot be reallocated.";
                case Win32ErrorCode.TooManyModules: return "Too many dynamic-link modules are attached to this program or dynamic-link module.";
                case Win32ErrorCode.NestingNotAllowed: return "Cannot nest calls to LoadModule.";
                case Win32ErrorCode.ExeMachineTypeMismatch: return "The version of %1 is not compatible with the version you're running. Check your computer's system information to see whether you need a x86 ; or x64 ; version of the program, and then contact the software publisher.";
                case Win32ErrorCode.ExeCannotModifySignedBinary: return "The image file %1 is signed, unable to modify.";
                case Win32ErrorCode.ExeCannotModifyStrongSignedBinary: return "The image file %1 is strong signed, unable to modify.";
                case Win32ErrorCode.BadCommand: return "The device does not recognize the command.";
                case Win32ErrorCode.FileCheckedOut: return "This file is checked out or locked for editing by another user.";
                case Win32ErrorCode.CheckoutRequired: return "The file must be checked out before saving changes.";
                case Win32ErrorCode.BadFileType: return "The file type being saved or retrieved has been blocked.";
                case Win32ErrorCode.FileTooLarge: return "The file size exceeds the limit allowed and cannot be saved.";
                case Win32ErrorCode.FormsAuthRequired: return "Access Denied. Before opening files in this location, you must first add the web site to your trusted sites list, browse to the web site, and select the option to login automatically.";
                case Win32ErrorCode.VirusInfected: return "Operation did not complete successfully because the file contains a virus.";
                case Win32ErrorCode.VirusDeleted: return "This file contains a virus and cannot be opened. Due to the nature of this virus, the file has been removed from this location.";
                case Win32ErrorCode.PipeLocal: return "The pipe is local.";
                case Win32ErrorCode.CyclicRedundancyCheck: return "Data error ;.";
                case Win32ErrorCode.BadPipe: return "The pipe state is invalid.";
                case Win32ErrorCode.PipeBusy: return "All pipe instances are busy.";
                case Win32ErrorCode.NoData: return "The pipe is being closed.";
                case Win32ErrorCode.PipeNotConnected: return "No process is on the other end of the pipe.";
                case Win32ErrorCode.MoreData: return "More data is available.";
                case Win32ErrorCode.BadLength: return "The program issued a command but the command length is incorrect.";
                case Win32ErrorCode.VCDisconnected: return "The session was canceled.";
                case Win32ErrorCode.Seek: return "The drive cannot locate a specific area or track on the disk.";
                case Win32ErrorCode.InvalidEAName: return "The specified extended attribute name was invalid.";
                case Win32ErrorCode.EAListInconsistent: return "The extended attributes are inconsistent.";
                case Win32ErrorCode.WaitTimeout: return "The wait operation timed out.";
                case Win32ErrorCode.NoMoreItems: return "No more data is available.";
                case Win32ErrorCode.NotDosDisk: return "The specified disk or diskette cannot be accessed.";
                case Win32ErrorCode.CannotCopy: return "The copy functions cannot be used.";
                case Win32ErrorCode.Directory: return "The directory name is invalid.";
                case Win32ErrorCode.SectorNotFound: return "The drive cannot find the sector requested.";
                case Win32ErrorCode.ExtendedAttributesDidNotFit: return "The extended attributes did not fit in the buffer.";
                case Win32ErrorCode.EAFileCorrupt: return "The extended attribute file on the mounted file system is corrupt.";
                case Win32ErrorCode.EATableFull: return "The extended attribute table file is full.";
                case Win32ErrorCode.InvalidEAHandle: return "The specified extended attribute handle is invalid.";
                case Win32ErrorCode.OutOfPaper: return "The printer is out of paper.";
                case Win32ErrorCode.ExtendedAttributesNotSupported: return "The mounted file system does not support extended attributes.";
                case Win32ErrorCode.NotOwner: return "Attempt to release mutex not owned by caller.";
                case Win32ErrorCode.WriteFault: return "The system cannot write to the specified device.";
                case Win32ErrorCode.TooManyPosts: return "Too many posts were made to a semaphore.";
                case Win32ErrorCode.PartialCopy: return "Only part of a ReadProcessMemory or WriteProcessMemory request was completed.";
                case Win32ErrorCode.PathNotFound: return "The system cannot find the path specified.";
                case Win32ErrorCode.ReadFault: return "The system cannot read from the specified device.";
                case Win32ErrorCode.OpLockNotGranted: return "The oplock request is denied.";
                case Win32ErrorCode.InvalidOpLockProtocol: return "An invalid oplock acknowledgment was received by the system.";
                case Win32ErrorCode.DiskTooFragmented: return "The volume is too fragmented to complete this operation.";
                case Win32ErrorCode.DeletePending: return "The file cannot be opened because it is in the process of being deleted.";
                case Win32ErrorCode.IncompatibleWithGlobalShortNameRegistrySetting: return "Short name settings may not be changed on this volume due to the global registry setting.";
                case Win32ErrorCode.ShortNamesNotEnabledOnVolume: return "Short names are not enabled on this volume.";
                case Win32ErrorCode.SecurityStreamIsInconsistent: return "The security stream for the given volume is in an inconsistent state. Please run CHKDSK on the volume.";
                case Win32ErrorCode.InvalidLockRange: return "A requested file lock operation cannot be processed due to an invalid byte range.";
                case Win32ErrorCode.ImageSubsystemNotPresent: return "The subsystem needed to support the image type is not present.";
                case Win32ErrorCode.NotificationGuidAlreadyDefined: return "The specified file already has a notification GUID associated with it.";
                case Win32ErrorCode.GeneralFailure: return "A device attached to the system is not functioning.";
                case Win32ErrorCode.MessageRecordForMessageIdNotFound: return "The system cannot find message text for message number 0x%1 in the message file for %2.";
                case Win32ErrorCode.ScopeNotFound: return "The scope specified was not found.";
                case Win32ErrorCode.SharingViolation: return "The process cannot access the file because it is being used by another process.";
                case Win32ErrorCode.LockViolation: return "The process cannot access the file because another process has locked a portion of the file.";
                case Win32ErrorCode.WrongDisk: return "The wrong diskette is in the drive. Insert %2 ; into drive %1.";
                case Win32ErrorCode.FailNoActionReboot: return "No action was taken as a system reboot is required.";
                case Win32ErrorCode.FailShutdown: return "The shutdown operation failed.";
                case Win32ErrorCode.FailRestart: return "The restart operation failed.";
                case Win32ErrorCode.MaxSessionsReached: return "The maximum number of sessions has been reached.";
                case Win32ErrorCode.SharingBufferExceeded: return "Too many files opened for sharing.";
                case Win32ErrorCode.HandleEndOfFile: return "Reached the end of the file.";
                case Win32ErrorCode.HandleDiskFull: return "The disk is full.";
                case Win32ErrorCode.TooManyOpenFiles: return "The system cannot open the file.";
                case Win32ErrorCode.ThreadModeAlreadyBackground: return "The thread is already in background processing mode.";
                case Win32ErrorCode.ThreadModeNotBackground: return "The thread is not in background processing mode.";
                case Win32ErrorCode.ProcessModeAlreadyBackground: return "The process is already in background processing mode.";
                case Win32ErrorCode.ProcessModeNotBackground: return "The process is not in background processing mode.";
                case Win32ErrorCode.InvalidAddress: return "Attempt to access invalid address.";
                case Win32ErrorCode.AccessDenied: return "Access is denied.";
                case Win32ErrorCode.NotSupported: return "The request is not supported.";
                case Win32ErrorCode.RemNotList: return "Windows cannot find the network path. Verify that the network path is correct and the destination computer is not busy or turned off. If Windows still cannot find the network path, contact your network administrator.";
                case Win32ErrorCode.DupName: return "You were not connected because a duplicate name exists on the network. If joining a domain, go to System in Control Panel to change the computer name and try again. If joining a workgroup, choose another workgroup name.";
                case Win32ErrorCode.BadNetworkPath: return "The network path was not found.";
                case Win32ErrorCode.NetworkBusy: return "The network is busy.";
                case Win32ErrorCode.DevNotExist: return "The specified network resource or device is no longer available.";
                case Win32ErrorCode.TooManyCommands: return "The network BIOS command limit has been reached.";
                case Win32ErrorCode.AdapterHardwareError: return "A network adapter hardware error occurred.";
                case Win32ErrorCode.BadNetworkResponse: return "The specified server cannot perform the requested operation.";
                case Win32ErrorCode.UnexpectedNetworkError: return "An unexpected network error occurred.";
                case Win32ErrorCode.InvalidHandle: return "The handle is invalid.";
                case Win32ErrorCode.BadEmAdapter: return "The remote adapter is not compatible.";
                case Win32ErrorCode.PrintQueueFull: return "The printer queue is full.";
                case Win32ErrorCode.NoSpoolSpace: return "Space to store the file waiting to be printed is not available on the server.";
                case Win32ErrorCode.PrintCanceled: return "Your file waiting to be printed was deleted.";
                case Win32ErrorCode.NetworkNameDeleted: return "The specified network name is no longer available.";
                case Win32ErrorCode.NetworkAccessDenied: return "Network access is denied.";
                case Win32ErrorCode.BadDevType: return "The network resource type is not correct.";
                case Win32ErrorCode.BadNetName: return "The network name cannot be found.";
                case Win32ErrorCode.TooManyNames: return "The name limit for the local computer network adapter card was exceeded.";
                case Win32ErrorCode.TooManySessions: return "The network BIOS session limit was exceeded.";
                case Win32ErrorCode.ArenaTrashed: return "The storage control blocks were destroyed.";
                case Win32ErrorCode.SharingPaused: return "The remote server has been paused or is in the process of being started.";
                case Win32ErrorCode.RequestNotAccepted: return "No more connections can be made to this remote computer at this time because there are already as many connections as the computer can accept.";
                case Win32ErrorCode.RedirectionPaused: return "The specified printer or disk device has been paused.";
                case Win32ErrorCode.NotEnoughMemory: return "Not enough storage is available to process this command.";
                case Win32ErrorCode.FileExists: return "The file exists.";
                case Win32ErrorCode.CannotMake: return "The directory or file cannot be created.";
                case Win32ErrorCode.FailI24: return "Fail on INT 24.";
                case Win32ErrorCode.OutOfStructures: return "Storage to process this request is not available.";
                case Win32ErrorCode.AlreadyAssigned: return "The local device name is already in use.";
                case Win32ErrorCode.InvalidPassword: return "The specified network password is not correct.";
                case Win32ErrorCode.InvalidParameter: return "The parameter is incorrect.";
                case Win32ErrorCode.NetWriteFault: return "A write fault occurred on the network.";
                case Win32ErrorCode.NoProcSlots: return "The system cannot start another process at this time.";
                case Win32ErrorCode.InvalidBlock: return "The storage control block address is invalid.";

                default: return "unknown.";
            }
        }
    }
}
