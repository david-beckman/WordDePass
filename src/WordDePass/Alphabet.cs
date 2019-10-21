//-----------------------------------------------------------------------
// <copyright file="Alphabet.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;

    /// <summary>An alphabet; a set of letters.</summary>
    [SuppressMessage(
        "Microsoft.Naming",
        "CA1710:IdentifiersShouldHaveCorrectSuffix",
        Justification = "Alphabet is more specific than LetterCollection.")]
    public class Alphabet : IReadOnlyList<char>, IEquatable<Alphabet>
    {
        /// <summary>The set (alphabet) of lowercase letters.</summary>
        public static readonly Alphabet LowerCase = new Alphabet("abcdefghijklmnopqrstuvwxyz");

        /// <summary>The set (alphabet) of uppercase letters.</summary>
        public static readonly Alphabet UpperCase = new Alphabet("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

        /// <summary>The set (alphabet) of numeric letters.</summary>
        public static readonly Alphabet Numeric = new Alphabet("0123456789");

        /// <summary>The set (alphabet) of Whitespace symbol letters.</summary>
        public static readonly Alphabet WhitespaceSymbol = new Alphabet("\t\n\v\f\r ");

        /// <summary>The set (alphabet) of Non-Whitespace symbol letters.</summary>
        public static readonly Alphabet NonWhitespaceSymbol = new Alphabet(new string(
            Enumerable.Range(0x21, 0x30 - 0x21)
                .Union(Enumerable.Range(0x3A, 0x41 - 0x3A))
                .Union(Enumerable.Range(0x5B, 0x61 - 0x5B))
                .Union(Enumerable.Range(0x7B, 0x7F - 0x7B))
                .Select(id => (char)id)
                .ToArray()));

        /// <summary>Initializes a new instance of the <see cref="Alphabet" /> class.</summary>
        /// <param name="letters">The letters.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="letters" /> is <value>null</value> or <see cref="string.Empty" />.
        /// </exception>
        public Alphabet(string letters)
        {
            this.Letters = CheckLetters(letters);
        }

        /// <summary>Gets the letters that make up this alphabet.</summary>
        /// <returns>The distinct, ordered letters that make up this alphabet.</returns>
        public string Letters { get; }

        /// <inheritdoc />
        public int Count => this.Letters.Length;

        /// <inheritdoc />
        public char this[int i] => this.Letters[i];

        /// <summary>Joins the two alphabets together to form a new, larger <see cref="Alphabet" />.</summary>
        /// <param name="left">The left <see cref="Alphabet" /> to join.</param>
        /// <param name="right">The right <see cref="Alphabet" /> to join.</param>
        /// <returns>An <see cref="Alphabet" /> that is the combination of the two.</returns>
        public static Alphabet operator +(Alphabet left, Alphabet right)
        {
            return Add(left, right);
        }

        /// <summary>Returns the <see cref="Alphabet" /> by the specified name.</summary>
        /// <param name="name">The name of the <see cref="Alphabet" />.</param>
        /// <returns>The matching <see cref="Alphabet" />.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="name" /> is <value>null</value> or <see cref="string.Empty" />.
        /// </exception>
        /// <exception cref="NotSupportedException">The named alphabet is not supported.</exception>
        public static Alphabet GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var field = typeof(Alphabet).GetFields().FirstOrDefault(field => field.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (field == null)
            {
                throw new NotSupportedException(string.Format(Thread.CurrentThread.CurrentCulture, "Unknown alphabet: {0}", name));
            }

            return (Alphabet)field.GetValue(null);
        }

        /// <summary>Joins the two alphabets together to form a new, larger <see cref="Alphabet" />.</summary>
        /// <param name="left">The left <see cref="Alphabet" /> to join.</param>
        /// <param name="right">The right <see cref="Alphabet" /> to join.</param>
        /// <returns>An <see cref="Alphabet" /> that is the combination of the two.</returns>
        public static Alphabet Add(Alphabet left, Alphabet right)
        {
            return left == null ? right : left.Add(right);
        }

        /// <inheritdoc />
        public IEnumerator<char> GetEnumerator()
        {
            return this.Letters.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Letters.GetEnumerator();
        }

        /// <summary>Reports the zero-based index of the first occurrence of the specified Unicode character in this alphabet.</summary>
        /// <param name="value">A Unicode character to seek.</param>
        /// <returns>
        ///     The zero-based index position of <paramref name="value" /> if that character is found, or <value>-1</value> if it is not.
        /// </returns>
        public int IndexOf(char value)
        {
            return this.Letters.IndexOf(value, StringComparison.Ordinal);
        }

        /// <summary>Returns a value indicating whether a specified character occurs within this alphabet.</summary>
        /// <param name="value">A character to seek.</param>
        /// <returns>
        ///     <value>true</value> if the <paramref name="value" /> parameter occurs within this alphabet; otherwise, <value>false</value>.
        /// </returns>
        public bool Contains(char value)
        {
            return this.Letters.Contains(value, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Copies the elements of the <see cref="Alphabet" /> to an <see cref="Array" />, starting at a particular
        ///     <see cref="Array" /> index.
        /// </summary>
        /// <param name="array">
        ///     The one-dimensional <see cref="Array" /> that is the destination of the elements copied from <see cref="Alphabet" />. The
        ///     <see cref="Array" /> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array" /> is <value>null</value>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than <value>0</value>.</exception>
        /// <exception cref="ArgumentException">
        ///     The number of elements in the source <see cref="Alphabet" /> is greater than the available space from
        ///     <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.
        /// </exception>
        public void CopyTo(char[] array, int arrayIndex)
        {
            this.Letters.CopyTo(0, array, arrayIndex, this.Letters.Length);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return this.Letters;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Alphabet);
        }

        /// <inheritdoc />
        public bool Equals(Alphabet other)
        {
            return other != null && this.Letters.Equals(other.Letters, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Letters.GetHashCode(StringComparison.Ordinal);
        }

        /// <summary>Joins the two alphabets together to form a new, larger <see cref="Alphabet" />.</summary>
        /// <param name="other">The other <see cref="Alphabet" /> to join.</param>
        /// <returns>An <see cref="Alphabet" /> that is the combination of the two.</returns>
        public Alphabet Add(Alphabet other)
        {
            if (other == null || this.Equals(other))
            {
                return this;
            }

            return new Alphabet(this.Letters + other.Letters);
        }

        /// <summary>Checks the letters.</summary>
        /// <param name="letters">The letters.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="letters" /> is <value>null</value> or <see cref="string.Empty" />.
        /// </exception>
        /// <returns>The distinct set of letters.</returns>
        internal static string CheckLetters(string letters)
        {
            if (string.IsNullOrEmpty(letters))
            {
                throw new ArgumentNullException(nameof(letters));
            }

            return new string(letters.Distinct().OrderBy(c => c).ToArray());
        }
    }
}
