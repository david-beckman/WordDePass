//-----------------------------------------------------------------------
// <copyright file="PasswordChecker.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.IO;
    using System.Security;
    using System.Threading;
    using System.Threading.Tasks;

    using Aspose.Words;

    /// <summary>Checks passwords against the a word document.</summary>
    public class PasswordChecker : IDisposable
    {
        private readonly FileInfo file;

        private byte[] bytes;

        private object mutex = new object();

        /// <summary>Initializes a new instance of the <see cref="PasswordChecker" /> class.</summary>
        /// <param name="filename">The name of the file to de-pass.</param>
        /// <exception cref="ArgumentNullException"><paramref name="filename" /> is <value>null</value>.</exception>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="filename" /> is empty, contains only white spaces, or contains invalid characters.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">Access to <paramref name="filename" /> is denied.</exception>
        /// <exception cref="PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined maximum length.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     <paramref name="filename" /> contains a colon (:) in the middle of the string.
        /// </exception>
        /// <exception cref="FileNotFoundException">The file referenced by <paramref name="filename" /> does not exist.</exception>
        public PasswordChecker(string filename)
            : this(new FileInfo(filename))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PasswordChecker" /> class.</summary>
        /// <param name="file">The file to de-pass.</param>
        /// <exception cref="ArgumentNullException"><paramref name="file" /> is <value>null</value>.</exception>
        /// <exception cref="FileNotFoundException"><paramref name="file" /> does not exist.</exception>
        public PasswordChecker(FileInfo file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Exists)
            {
                throw new FileNotFoundException(null, file.FullName);
            }

            this.file = file;
        }

        /// <summary>Finalizes an instance of the <see cref="PasswordChecker" /> class.</summary>
        ~PasswordChecker()
        {
            this.Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Checks to see if the password works.</summary>
        /// <param name="password">The password to check.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns><value>true</value> if the password works; otherwise <value>false</value>.</returns>
        public async Task<bool> CheckPasswordAsync(string password, CancellationToken token)
        {
            var bytes = await this.GetBytesAsync(token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();

            return TryPassword(bytes, password);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="disposing">
        ///     <value>true</value> if called from the <see cref="Dispose()" /> method; <value>false</value> when called from the
        ///     finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.mutex)
                {
                    this.bytes = null;
                }
            }
        }

        private static bool TryPassword(byte[] bytes, string password)
        {
            using (var stream = new MemoryStream(bytes))
            {
                try
                {
                    var doc = new Document(stream, new LoadOptions(password));

                    return true;
                }
                catch (IncorrectPasswordException)
                {
                    return false;
                }
            }
        }

        private async Task<byte[]> GetBytesAsync(CancellationToken token)
        {
            lock (this.mutex)
            {
                if (this.bytes != null)
                {
                    return this.bytes;
                }
            }

            var bytes = await File.ReadAllBytesAsync(this.file.FullName, token).ConfigureAwait(false);

            lock (this.mutex)
            {
                if (this.bytes == null)
                {
                    this.bytes = bytes;
                }
            }

            return bytes;
        }
    }
}
