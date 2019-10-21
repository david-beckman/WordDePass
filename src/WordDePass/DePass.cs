//-----------------------------------------------------------------------
// <copyright file="DePass.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Collections.Generic;
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
        public async Task<string> FindPasswordAsync(PasswordHints hints = null, CancellationToken token = default)
        {
            hints = hints ?? new PasswordHints();
            var alphabet = hints.Alphabet;

            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var allFixes = hints.AllFixes;
                var tasks = Enumerable.Range(0, hints.AllFixes.Count).Select(_ => (Task<string>)null).ToList();
                for (int i = 0; i < allFixes.Count; i++)
                {
                    var fixes = allFixes[i];
                    tasks[i] = Task.Run(async () =>
                    {
                        for (var length = hints.MinLength.Value; length <= hints.MaxLength.Value; length++)
                        {
                            var password = await this.FindPasswordAsync(alphabet, length, fixes, tokenSource.Token).ConfigureAwait(false);
                            if (password != null)
                            {
                                return password;
                            }
                        }

                        return null;
                    });
                }

                while (tasks.Count > 0)
                {
                    var first = await Task.WhenAny(tasks).ConfigureAwait(false);
                    if (first.Result != null)
                    {
                        return first.Result;
                    }

                    tasks.Remove(first);
                }
            }

            return null;
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

        private async Task<string> FindPasswordAsync(Alphabet alphabet, int length, Fixes fixes, CancellationToken token)
        {
            var remainder = length - fixes.Length;

            for (int leftLength = 0; leftLength <= remainder; leftLength++)
            {
                var rightLength = remainder - leftLength;

                var leftCollection = new FixedLengthPasswordCollection(leftLength, alphabet);
                foreach (var left in leftCollection)
                {
                    var rightCollection = new FixedLengthPasswordCollection(rightLength, alphabet);
                    foreach (var right in rightCollection)
                    {
                        var password = fixes.ToString(left, right);
                        if (await this.checker.CheckPasswordAsync(password, token).ConfigureAwait(false))
                        {
                            return password;
                        }
                    }
                }

                if (!fixes.HasInfix)
                {
                    break;
                }
            }

            return null;
        }
    }
}
