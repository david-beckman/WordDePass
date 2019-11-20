namespace WordDePass.Conosle
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

        internal static string Args_FilenameRequired => ResourceManager.GetString(nameof(Args_FilenameRequired), Culture);

        internal static string Args_InvalidLength => ResourceManager.GetString(nameof(Args_InvalidLength), Culture);

        internal static string Args_UnknownOption => ResourceManager.GetString(nameof(Args_UnknownOption), Culture);

        internal static string Maximum => ResourceManager.GetString(nameof(Maximum), Culture);

        internal static string Minimum => ResourceManager.GetString(nameof(Minimum), Culture);

        internal static string OutputFormat => ResourceManager.GetString(nameof(OutputFormat), Culture);

        internal static string Usage => ResourceManager.GetString(nameof(Usage), Culture);
    }
}
