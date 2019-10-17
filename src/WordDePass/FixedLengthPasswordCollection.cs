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

    /// <summary>A collection of passwords of a certain length.</summary>
    public class FixedLengthPasswordCollection : IEnumerable<string>
    {
        /// <summary>Initializes a new instance of the <see cref="FixedLengthPasswordCollection" /> class.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        public FixedLengthPasswordCollection(int passwordLength)
        {
            this.PasswordLength = FixedLengthPasswordGenerator.CheckLength(passwordLength);
        }

        /// <summary>Initializes a new instance of the <see cref="FixedLengthPasswordCollection" /> class.</summary>
        /// <param name="passwordLength">The length of the passwords in the collection.</param>
        /// <param name="alphabet">The alphabet available for the passwords.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="alphabet" /> is <value>null</value> or <see cref="string.Empty" />.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="passwordLength" /> is negative.</exception>
        public FixedLengthPasswordCollection(int passwordLength, string alphabet)
        {
            this.PasswordLength = FixedLengthPasswordGenerator.CheckLength(passwordLength);
            this.Alphabet = FixedLengthPasswordGenerator.CheckAlphabet(alphabet);
        }

        /// <summary>Gets the length of the passwords in the collection.</summary>
        /// <returns>The length of the passwords in the collection.</returns>
        public int PasswordLength { get; }

        /// <summary>Gets the alphabet available for the passwords.</summary>
        /// <returns>The alphabet available for the passwords.</returns>
        public string Alphabet { get; }

        /// <inheritdoc />
        public IEnumerator<string> GetEnumerator()
        {
            return this.Alphabet != null ?
                new FixedLengthPasswordGenerator(this.PasswordLength, this.Alphabet) :
                new FixedLengthPasswordGenerator(this.PasswordLength);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
