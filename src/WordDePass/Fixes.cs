//-----------------------------------------------------------------------
// <copyright file="Fixes.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    /// <summary>A collection of a password <see cref="Prefix" />, and <see cref="Infix" />, and a <see cref="Suffix" />.</summary>
    public class Fixes
    {
        /// <summary>Initializes a new instance of the <see cref="Fixes" /> class.</summary>
        /// <param name="prefix">The prefix. <value>null</value> is handled the same as <see cref="string.Empty" />.</param>
        /// <param name="infix">The infix. <value>null</value> is handled the same as <see cref="string.Empty" />.</param>
        /// <param name="suffix">The suffix. <value>null</value> is handled the same as <see cref="string.Empty" />.</param>
        public Fixes(string prefix, string infix, string suffix)
        {
            this.Prefix = prefix ?? string.Empty;
            this.Infix = infix ?? string.Empty;
            this.Suffix = suffix ?? string.Empty;
        }

        /// <summary>Gets a likely prefix for the password.</summary>
        /// <returns>The likely password prefix.</returns>
        public string Prefix { get; }

        /// <summary>Gets a likely infix for the password.</summary>
        /// <returns>The likely password infix.</returns>
        public string Infix { get; }

        /// <summary>Gets a value indicating whether <see cref="Infix" /> is non-empty.</summary>
        /// <returns><value>true</value> if <see cref="Infix" /> is non-empty; other <value>false</value>.</returns>
        public bool HasInfix => this.Infix.Length != 0;

        /// <summary>Gets a likely suffix for the password.</summary>
        /// <returns>The likely password suffix.</returns>
        public string Suffix { get; }

        /// <summary>Gets the number of characters in the current <see cref="Fixes" /> object.</summary>
        /// <returns>The number of characters in the current fixes.</returns>
        public int Length => this.Prefix.Length + this.Infix.Length + this.Suffix.Length;

        /// <inheritdoc />
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        /// <summary>
        ///     Returns a string that represents the current object adding the passed <paramref name="left" /> and
        ///     <paramref name="right" /> strings.
        /// </summary>
        /// <param name="left">The <see cref="string" /> to include between <see cref="Prefix" /> and <see cref="Infix" />.</param>
        /// <param name="right">The <see cref="string" /> to include between <see cref="Infix" /> and <see cref="Suffix" />.</param>
        /// <returns>
        ///     A concatenation of <see cref="Prefix" />, <paramref name="left" />, <see cref="Infix" />, <paramref name="right" />, and
        ///     <see cref="Suffix" />.
        /// </returns>
        public string ToString(string left, string right)
        {
            return ((object)this.Prefix).ToString() + left + this.Infix + right + this.Suffix;
        }
    }
}
