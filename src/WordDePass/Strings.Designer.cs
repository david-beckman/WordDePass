namespace WordDePass
{
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [DebuggerNonUserCode]
    [CompilerGenerated]
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    internal sealed class Strings
    {
        private static ResourceManager resourceMan;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager =>
            resourceMan ?? (resourceMan = new ResourceManager(typeof(Strings).FullName, typeof(Strings).Assembly));

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture { get; set; }

        internal static string Arg_GreaterThanOrEqualTo => ResourceManager.GetString(nameof(Arg_GreaterThanOrEqualTo), Culture);

        internal static string Arg_NonNegative => ResourceManager.GetString(nameof(Arg_NonNegative), Culture);

        internal static string Arg_TooShort => ResourceManager.GetString(nameof(Arg_TooShort), Culture);

        internal static string InvalidOperation_EnumEnded => ResourceManager.GetString(nameof(InvalidOperation_EnumEnded), Culture);

        internal static string InvalidOperation_EnumNotStarted => ResourceManager.GetString(nameof(InvalidOperation_EnumNotStarted), Culture);

        internal static string NotSupported_Bom => ResourceManager.GetString(nameof(NotSupported_Bom), Culture);

        internal static string NotSupported_Clsid => ResourceManager.GetString(nameof(NotSupported_Clsid), Culture);

        internal static string NotSupported_FlagNotSet => ResourceManager.GetString(nameof(NotSupported_FlagNotSet), Culture);

        internal static string NotSupported_MiniSectorShift => ResourceManager.GetString(nameof(NotSupported_MiniSectorShift), Culture);

        internal static string NotSupported_MiniStreamCutoff => ResourceManager.GetString(nameof(NotSupported_MiniStreamCutoff), Culture);

        internal static string NotSupported_SectorShift => ResourceManager.GetString(nameof(NotSupported_SectorShift), Culture);

        internal static string NotSupported_Signature => ResourceManager.GetString(nameof(NotSupported_Signature), Culture);

        internal static string NotSupported_Version1 => ResourceManager.GetString(nameof(NotSupported_Version1), Culture);

        internal static string NotSupported_Version2 => ResourceManager.GetString(nameof(NotSupported_Version2), Culture);

        internal static string ToString_CfbHeader => ResourceManager.GetString(nameof(ToString_CfbHeader), Culture);
    }
}