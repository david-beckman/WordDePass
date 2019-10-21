//-----------------------------------------------------------------------
// <copyright file="FixedLengthPasswordGenerator.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    /// <summary>Generates a set of passwords of a given length and alphabet.</summary>
    public class FixedLengthPasswordGenerator : IEnumerator<string>
    {
        private int[] current;
        private bool started;
        private bool finished;

        /// <summary>Initializes a new instance of the <see cref="FixedLengthPasswordGenerator" /> class.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <param name="alphabet">The alphabet available for the passwords.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="alphabet" /> is <value>null</value>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        public FixedLengthPasswordGenerator(int passwordLength, Alphabet alphabet)
        {
            this.PasswordLength = CheckLength(passwordLength);
            this.Alphabet = alphabet ?? throw new ArgumentNullException(nameof(alphabet));
        }

        /// <summary>Finalizes an instance of the <see cref="FixedLengthPasswordGenerator" /> class.</summary>
        ~FixedLengthPasswordGenerator()
        {
            this.Dispose(false);
        }

        /// <summary>Gets the length of the passwords in the collection.</summary>
        /// <returns>The length of the passwords in the collection.</returns>
        public int PasswordLength { get; }

        /// <summary>Gets the alphabet available for the passwords.</summary>
        /// <returns>The alphabet available for the passwords.</returns>
        public Alphabet Alphabet { get; }

        /// <inheritdoc />
        public string Current => this.started && !this.finished ?
            new string(this.current.Select(index => this.Alphabet[index]).ToArray()) :
            throw new InvalidOperationException(this.started ?
                Strings.InvalidOperation_EnumNotStarted :
                Strings.InvalidOperation_EnumEnded);

        /// <inheritdoc />
        object IEnumerator.Current => this.Current;

        /// <inheritdoc />
        public bool MoveNext()
        {
            int i;

            if (this.current == null)
            {
                this.started = true;
                this.current = new int[this.PasswordLength];
                return true;
            }

            for (i = this.PasswordLength - 1; i >= 0; i--)
            {
                this.current[i]++;
                if (this.current[i] < this.Alphabet.Count)
                {
                    break;
                }

                this.current[i] = 0;
            }

            this.finished = i < 0;
            return !this.finished;
        }

        /// <inheritdoc />
        public void Reset()
        {
            this.current = null;
            this.started = this.finished = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Checks the length.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        /// <returns>The length.</returns>
        internal static int CheckLength(int passwordLength)
        {
            if (passwordLength < 0)
            {
                // 'passwordLength' must be non-negative.
                throw new ArgumentOutOfRangeException(
                    nameof(passwordLength),
                    passwordLength,
                    string.Format(Thread.CurrentThread.CurrentCulture, Strings.Arg_NonNegative, nameof(passwordLength)));
            }

            return passwordLength;
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
                this.current = null;
            }
        }
    }
}
