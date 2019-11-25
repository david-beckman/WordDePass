//-----------------------------------------------------------------------
// <copyright file="DePass.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>Finds the password for a word document.</summary>
    public class DePass : IDisposable
    {
        private readonly bool shouldDisposeChecker;

        private PasswordChecker checker;

        /// <summary>Initializes a new instance of the <see cref="DePass" /> class.</summary>
        /// <param name="checker">The checker to see if a password is valid.</param>
        /// <param name="shouldDisposeChecker">
        ///     <value>true</value> if the inner checker should be disposed of by <see cref="Dispose()" />; <value>false</value> if you
        ///     intend to reuse the inner checker.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="checker" /> is <value>null</value>.</exception>
        public DePass(PasswordChecker checker, bool shouldDisposeChecker = true)
        {
            this.checker = checker ?? throw new ArgumentNullException(nameof(checker));
            this.shouldDisposeChecker = shouldDisposeChecker;
        }

        /// <summary>Finalizes an instance of the <see cref="DePass" /> class.</summary>
        ~DePass()
        {
            this.Dispose(false);
        }

        /// <summary>Finds the password.</summary>
        /// <param name="hints">The various password hints.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The password.</returns>
        public string FindPassword(PasswordHints hints = null, CancellationToken token = default)
        {
            hints ??= new PasswordHints();

            string result = null;
            Parallel.ForEach(hints.GeneratePasswords(), new ParallelOptions { CancellationToken = token }, (password, loopState) =>
            {
                if (this.checker.CheckPassword(password))
                {
                    result = password;
                    loopState.Stop();
                }
            });

            return result;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="disposing">
        ///     <value>true</value> if called from the <see cref="Dispose()" /> method; <value>false</value> when called from the
        ///     finalizer.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && this.checker != null && this.shouldDisposeChecker)
            {
                this.checker.Dispose();
            }

            this.checker = null;
        }
    }
}
