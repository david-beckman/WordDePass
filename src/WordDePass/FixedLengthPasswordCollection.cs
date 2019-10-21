//-----------------------------------------------------------------------
// <copyright file="FixedLengthPasswordCollection.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>A collection of passwords of a certain length.</summary>
    public class FixedLengthPasswordCollection : IReadOnlyCollection<string>
    {
        /// <summary>Initializes a new instance of the <see cref="FixedLengthPasswordCollection" /> class.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <param name="alphabet">The alphabet available for the passwords.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="alphabet" /> is <value>null</value>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        public FixedLengthPasswordCollection(int passwordLength, Alphabet alphabet)
        {
            this.PasswordLength = FixedLengthPasswordGenerator.CheckLength(passwordLength);
            this.Alphabet = alphabet ?? throw new ArgumentNullException(nameof(alphabet));
        }

        /// <summary>Gets the length of the passwords in the collection.</summary>
        /// <returns>The length of the passwords in the collection.</returns>
        public int PasswordLength { get; }

        /// <summary>Gets the alphabet available for the passwords.</summary>
        /// <returns>The alphabet available for the passwords.</returns>
        public Alphabet Alphabet { get; }

        /// <inheritdoc />
        public int Count => Enumerable.Range(0, this.PasswordLength).Aggregate((accum, value) => accum * this.Alphabet.Count);

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            return new FixedLengthPasswordGenerator(this.PasswordLength, this.Alphabet);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
