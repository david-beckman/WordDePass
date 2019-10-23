//-----------------------------------------------------------------------
// <copyright file="CfbHeader.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Buffers.Binary;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>
    ///     A MS Binary Compound File header as per the MS-CFB standard:
    ///     <see href="https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-cfb/53989ce4-7b05-4f8d-829b-d08d6148375b" />.
    /// </summary>
    public class CfbHeader
    {
        /// <summary>The expected header length.</summary>
        public const int HeaderLength = 0x200;
        private const short ExpectedMinorVersion = 0x3E;
        private const ushort ExpectedBom = 0xFFFE;
        private const short ExpectedMiniShift = 0x06;
        private const int ExpectedMiniCutoff = 0x1000;
        private static readonly byte[] Signature = new byte[] { 0x0D, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 };
        private static readonly byte[] EmptyClsId = Guid.Empty.ToByteArray();
        private static readonly int ClsIdOffset = Signature.Length;
        private static readonly int MinorVersionOffset = ClsIdOffset + EmptyClsId.Length;
        private static readonly int MajorVersionOffset = MinorVersionOffset + 2;
        private static readonly int BomOffset = MajorVersionOffset + 2;
        private static readonly int SectorShiftOffset = BomOffset + 2;
        private static readonly int MiniShiftOffset = SectorShiftOffset + 2;
        private static readonly int TransactionSignatureOffset = MiniShiftOffset + 22;
        private static readonly int MiniCutoffOffset = TransactionSignatureOffset + 4;
        private static readonly IDictionary<short, short> ExpectedSectorShiftsByVersion = new Dictionary<short, short>
        {
            { 3, 0x09 },
            { 4, 0x0C },
        };

        private static readonly ICollection<short> ExpectedMajorVersions = ExpectedSectorShiftsByVersion.Keys;

        /// <summary>Initializes a new instance of the <see cref="CfbHeader" /> class.</summary>
        /// <param name="header">The file header.</param>
        /// <exception cref="ArgumentNullException"><paramref name="header" /> is <value>null</value>.</exception>
        /// <exception cref="ArgumentException"><paramref name="header" /> has a length other than <see cref="HeaderLength" />.</exception>
        /// <exception cref="NotSupportedException">
        ///     One of more values within the <paramref name="header" /> do not conform with the specification.
        /// </exception>
        public CfbHeader(ReadOnlySpan<byte> header)
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

            if (!header.StartsWith(Signature))
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_Signature,
                    nameof(header)));
            }

            if (!header.Slice(ClsIdOffset, EmptyClsId.Length).SequenceEqual(EmptyClsId))
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_Clsid,
                    nameof(header)));
            }

            this.MajorVersion = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(MajorVersionOffset, 2));
            this.MinorVerison = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(MinorVersionOffset, 2));
            if (this.MinorVerison != ExpectedMinorVersion || !ExpectedMajorVersions.Contains(this.MajorVersion))
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_Version2,
                    this.MajorVersion,
                    this.MinorVerison));
            }

            var bom = BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(BomOffset, 2));
            if (bom != ExpectedBom)
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_Bom,
                    bom));
            }

            this.SectorShift = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(SectorShiftOffset, 2));
            if (this.SectorShift != ExpectedSectorShiftsByVersion[this.MajorVersion])
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_SectorShift,
                    this.MajorVersion,
                    this.MinorVerison,
                    this.SectorShift));
            }

            var miniShift = BinaryPrimitives.ReadInt16LittleEndian(header.Slice(MiniShiftOffset, 2));
            if (miniShift != ExpectedMiniShift)
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_MiniSectorShift,
                    miniShift));
            }

            this.TransactionSignature = BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(TransactionSignatureOffset, 4));
            var miniCutoff = BinaryPrimitives.ReadInt32LittleEndian(header.Slice(MiniCutoffOffset, 4));
            if (miniCutoff != ExpectedMiniCutoff)
            {
                throw new NotSupportedException(string.Format(
                    Thread.CurrentThread.CurrentCulture,
                    Strings.NotSupported_MiniStreamCutoff,
                    miniShift));
            }
        }

        /// <summary>Gets the minor version of the specification that this file supports.</summary>
        /// <returns>
        ///     Version number for nonbreaking changes. This SHOULD be <value>0x003E</value> if the <see cref="MajorVersion" /> is either
        ///     <value>0x0003</value> or <value>0x0004</value>.
        /// </returns>
        public short MinorVerison { get; }

        /// <summary>Gets the major version of the specification that this file supports.</summary>
        /// <returns>
        ///     Version number for breaking changes. This MUST be set to either <value>0x0003</value> (version 3) or <value>0x0004</value>
        ///     (version 4).
        /// </returns>
        public short MajorVersion { get; }

        /// <summary>Gets the sector shift.</summary>
        /// <returns>
        ///     The sector shift. This MUST be set to <value>0x0009</value>, or <value>0x000c</value>, depending on the
        ///     <see cref="MajorVersion" />. This specifies the sector size of the compound file as a power of 2.
        ///     <list type="bullet">
        ///         <item>
        ///             If <see cref="MajorVersion" /> is 3, the Sector Shift MUST be <value>0x0009</value>, specifying a sector size of
        ///             512 bytes.
        ///         </item>
        ///         <item>
        ///             If <see cref="MajorVersion" /> is 4, the Sector Shift MUST be <value>0x000C</value>, specifying a sector size of
        ///             4096 bytes.
        ///         </item>
        ///     </list>
        /// </returns>
        public short SectorShift { get; }

        /// <summary>Gets the sector size.</summary>
        /// <returns>
        ///     The sector shift. This MUST be set to <value>512</value>, or <value>4096</value>, depending on the
        ///     <see cref="MajorVersion" />.
        ///     <list type="bullet">
        ///         <item>
        ///             If <see cref="MajorVersion" /> is 3, the Sector Size MUST be <value>512</value>.
        ///         </item>
        ///         <item>
        ///             If <see cref="MajorVersion" /> is 4, the Sector Size MUST be <value>4096</value>.
        ///         </item>
        ///     </list>
        /// </returns>
        public int SectorSize => 1 << this.SectorShift;

        /// <summary>Gets the transaction signature number.</summary>
        /// <returns>
        ///     An unigned integer that MAY contain a sequence number that is incremented every time the compound file is saved by an
        ///     implementation that supports file transactions. This MUST be set to <value>0</value> if file transactions are not
        ///     implemented.
        /// </returns>
        public uint TransactionSignature { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToString(Thread.CurrentThread.CurrentCulture);
        }

        /// <summary>
        ///     Converts the header value of this instance to its equivalent string representation using the specified culture-specific
        ///     format information.
        /// </summary>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>A string that represents the current instance.</returns>
        public string ToString(IFormatProvider provider)
        {
            return string.Format(
                provider,
                Strings.ToString_CfbHeader,
                this.MajorVersion,
                this.MinorVerison,
                this.SectorShift,
                this.TransactionSignature);
        }
    }
}
