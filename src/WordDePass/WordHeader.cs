//-----------------------------------------------------------------------
// <copyright file="WordHeader.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Buffers.Binary;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     The section header for an MS word document (MS-DOC) as specified by
    ///     <see href="https://docs.microsoft.com/en-us/openspecs/office_file_formats/ms-doc/ccd7b486-7881-484c-a137-51170af7cc22" />.
    ///     Note that a word document is a CFB document so the word header will be offset according to the MS-CFB specification.
    /// </summary>
    public class WordHeader
    {
        /// <summary>The expected header length.</summary>
        public const int HeaderLength = 32;
        private const ushort Signature = 0xA5EC;
        private const short Version = 0xC1;
        private const int SignatureOffset = 0;
        private const int VersionOffset = SignatureOffset + 2;
        private const int FlagsOffset = VersionOffset + 6;
        private const ushort MaskADot = 0b_1000_0000_0000_0000;
        private const ushort MaskBAutoText = 0b_0100_0000_0000_0000;
        private const ushort MaskCIncrementalSave = 0b_0010_0000_0000_0000;
        private const ushort MaskDHasPictures = 0b_0001_0000_0000_0000;
        private const ushort MaskESaves = 0b_0000_1111_0000_0000;
        private const ushort ShiftESaves = 8;
        private const ushort MaskFEncrypted = 0b_0000_0000_1000_0000;
        private const ushort MaskGTableStream = 0b_0000_0000_0100_0000;
        private const ushort ShiftGTableStream = 6;
        private const ushort MaskHReadOnly = 0b_0000_0000_0010_0000;
        private const ushort MaskIWriteReservation = 0b_0000_0000_0001_0000;
        private const ushort MaskJExtended = 0b_0000_0000_0000_1000;
        private const ushort MaskKLoadOverride = 0b_0000_0000_0000_0100;
        private const ushort MaskLFarEast = 0b_0000_0000_0000_0010;
        private const ushort MaskMObfuscated = 0b_0000_0000_0000_0001;
        private const int FibBackOffset = FlagsOffset + 2;
        private const int KeyOffset = FibBackOffset + 2;
        private static readonly short[] ExpectedFibBackValues = new short[] { 0xBF, 0xC1 };

        /// <summary>Initializes a new instance of the <see cref="WordHeader" /> class.</summary>
        /// <param name="header">The file header.</param>
        /// <exception cref="ArgumentNullException"><paramref name="header" /> is <value>null</value>.</exception>
        /// <exception cref="ArgumentException"><paramref name="header" /> has a length other than <see cref="HeaderLength" />.</exception>
        /// <exception cref="NotSupportedException">
        ///     One of more values within the <paramref name="header" /> do not conform with the specification.
        /// </exception>
        public WordHeader(ReadOnlySpan<byte> header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            if (header.Length != HeaderLength)
            {
                throw new ArgumentException(
                    string.Format(
                        Thread.CurrentThread.CurrentCulture,
                        Strings.Arg_TooShort,
                        nameof(header),
                        HeaderLength,
                        header.Length),
                    nameof(header));
            }

            if (BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(SignatureOffset, 2)) != Signature)
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_Signature,
                    nameof(header)));
            }

            var version = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(VersionOffset, 2));
            if (version != Version)
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_Version1,
                    version));
            }

            var flags = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(FlagsOffset, 2));
            this.IsDocumentTemplate = (flags & MaskADot) != 0;
            this.ContainsOnlyAutoText = (flags & MaskBAutoText) != 0;
            this.IncrementallySaved = (flags & MaskCIncrementalSave) != 0;
            this.HasPictures = (flags & MaskDHasPictures) != 0;
            this.Saves = (byte)((flags & MaskESaves) >> ShiftESaves);
            this.Encrypted = (flags & MaskFEncrypted) != 0;
            this.TableStream = (byte)((flags & MaskGTableStream) >> ShiftGTableStream);
            this.ReadOnly = (flags & MaskHReadOnly) != 0;
            this.WriteReservation = (flags & MaskIWriteReservation) != 0;
            this.LoadOverride = (flags & MaskKLoadOverride) != 0;
            this.FarEast = (flags & MaskLFarEast) != 0;
            this.Obfuscated = (flags & MaskMObfuscated) != 0;
            if ((flags & MaskJExtended) == 0)
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_FlagNotSet,
                    "extended"));
            }

            var fibBack = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(FibBackOffset, 2));
            if (!ExpectedFibBackValues.Contains(fibBack))
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_FlagNotSet,
                    "FIB Back",
                    fibBack));
            }

            this.Key = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(KeyOffset, 4));
        }

        /// <summary>Gets a value indicating whether this header represents a document template (DOT).</summary>
        /// <returns><value>true</value> if this header is for a document template (DOT); otherwise <value>false</value>.</returns>
        public bool IsDocumentTemplate { get; }

        /// <summary>Gets a value indicating whether the document this header represents contains only auto-text (Glsy).</summary>
        /// <returns>
        ///     <value>true</value> if this header is for a document that contains only auto-text; otherwise <value>false</value>.
        /// </returns>
        public bool ContainsOnlyAutoText { get; }

        /// <summary>
        ///     Gets a value indicating whether the document the last save operation that was performed on this document was an incremental
        ///     save operation.
        /// </summary>
        /// <returns><value>true</value> if the last save was an incremental save; otherwise <value>false</value>.</returns>
        public bool IncrementallySaved { get; }

        /// <summary>Gets a value indicating whether the document this header represents contains pictures.</summary>
        /// <returns><value>true</value> if this header is for a document that contains pictures; otherwise <value>false</value>.</returns>
        public bool HasPictures { get; }

        /// <summary>Gets the number of consecutive times this document was incrementally saved.</summary>
        /// <returns>A nibble (half byte) representing the number of incremental saves.</returns>
        public byte Saves { get; }

        /// <summary>Gets a value indicating whether the docuemnt is encrypted.</summary>
        /// <returns><value>true</value> if the docuemnt is encrypted; otherwise <value>false</value>.</returns>
        public bool Encrypted { get; }

        /// <summary>Gets the number of the table stream.</summary>
        /// <returns><value>0</value> if the 0Table Table Stream should be used or <value>1</value> for the 1Table table stream.</returns>
        public byte TableStream { get; }

        /// <summary>Gets a value indicating whether it is recommended that the document be read only.</summary>
        /// <returns><value>true</value> to recommend that the document be read only; otherwise <value>false</value>.</returns>
        public bool ReadOnly { get; }

        /// <summary>Gets a value indicating whether the document has a write-reservation password.</summary>
        /// <returns><value>true</value> if the document has a write-reservation password.; otherwise <value>false</value>.</returns>
        public bool WriteReservation { get; }

        /// <summary>
        ///     Gets a value indicating whether override the language information and font that are specified in the paragraph style (the
        ///     normal style) with the defaults that are appropriate for the installation language of the application.
        /// </summary>
        /// <returns><value>true</value> to override; otherwise <value>false</value>.</returns>
        public bool LoadOverride { get; }

        /// <summary>
        ///     Gets a value indicating whether the installation language of the application that created the document was an East Asian
        ///     language.
        /// </summary>
        /// <returns><value>true</value> for an east asian language; otherwise <value>false</value>.</returns>
        public bool FarEast { get; }

        /// <summary>
        ///     Gets a value indicating whether the document is obfuscated by using XOR obfuscation. If <see cref="Encrypted" /> is
        ///     <value>false</value>, this value MUST be ignored.
        /// </summary>
        /// <returns><value>true</value> for an XORed document; otherwise <value>false</value>.</returns>
        public bool Obfuscated { get; }

        /// <summary>Gets the XOR key or the encryption header size.</summary>
        /// <returns>
        ///     If <see cref="Encrypted" /> is <value>false</value>, this value MUST be <value>0</value>. If <see cref="Obfuscated" /> is
        ///     <value>true</value>, this specifies the XOR obfuscation password verifier. Otherwise, this specifies the size of the
        ///     Encryption Header that is stored at the beginning of the Table stream.
        /// </returns>
        public uint Key { get; }
    }
}
