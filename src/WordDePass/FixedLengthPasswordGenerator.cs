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
        /// <summary>The set (alphabet) of lowercase letters.</summary>
        public const string LowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>The set (alphabet) of uppercase letters.</summary>
        public const string UpperCaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>The set (alphabet) of numeric letters.</summary>
        public const string NumericLetters = "0123456789";

        private char[] current;

        /// <summary>Initializes a new instance of the <see cref="FixedLengthPasswordGenerator" /> class.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        public FixedLengthPasswordGenerator(int passwordLength)
            : this(passwordLength, LowerCaseLetters + UpperCaseLetters + NumericLetters)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FixedLengthPasswordGenerator" /> class.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <param name="alphabet">The alphabet available for the passwords.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="alphabet" /> is <value>null</value> or <see cref="string.Empty" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        public FixedLengthPasswordGenerator(int passwordLength, string alphabet)
        {
            this.PasswordLength = CheckLength(passwordLength);
            this.Alphabet = CheckAlphabet(alphabet);
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
        public string Alphabet { get; }

        /// <inheritdoc />
        public string Current => this.current != null ? new string(this.current) : throw new InvalidOperationException();

        /// <inheritdoc />
        object IEnumerator.Current => this.Current;

        /// <inheritdoc />
        public bool MoveNext()
        {
            int i;

            if (this.current == null)
            {
                this.current = new char[this.PasswordLength];
                for (i = 0; i < this.PasswordLength; i++)
                {
                    this.current[i] = this.Alphabet[0];
                }

                return true;
            }

            for (i = this.PasswordLength - 1; i >= 0; i--)
            {
                var index = this.Alphabet.IndexOf(this.current[i], StringComparison.Ordinal);
                index++;
                if (index < this.Alphabet.Length)
                {
                    this.current[i] = this.Alphabet[index];
                    break;
                }

                this.current[i] = this.Alphabet[0];
            }

            return i >= 0;
        }

        /// <inheritdoc />
        public void Reset()
        {
            this.current = null;
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

        /// <summary>Checks the alphabet.</summary>
        /// <param name="alphabet">The alphabet available for the passwords.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="alphabet" /> is <value>null</value> or <see cref="string.Empty" />.
        /// </exception>
        /// <returns>The alphabet.</returns>
        internal static string CheckAlphabet(string alphabet)
        {
            if (string.IsNullOrEmpty(alphabet))
            {
                throw new ArgumentNullException(nameof(alphabet));
            }

            return new string(alphabet.Distinct().ToArray());
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
