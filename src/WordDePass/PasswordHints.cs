//-----------------------------------------------------------------------
// <copyright file="PasswordHints.cs" company="N/A">
//     Copyright © 2019 David Beckman. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace WordDePass
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    /// <summary>A set of password hints to limit the password space.</summary>
    public class PasswordHints
    {
        private const int DefaultMin = 0;
        private const int DefaultMax = 16;

        private static readonly Alphabet DefaultAlphabet = Alphabet.LowerCase +
                Alphabet.UpperCase +
                Alphabet.Numeric +
                Alphabet.NonWhitespaceSymbol;

        private Alphabet alphabet;
        private List<string> prefixes = new List<string>();
        private List<string> infixes = new List<string>();
        private List<string> suffixes = new List<string>();
        private int? min;
        private int? max;

        /// <summary>Gets or sets the likely <see cref="Alphabet" /> for the password.</summary>
        /// <returns>The likely password <see cref="Alphabet" />.</returns>
        public Alphabet Alphabet
        {
            get
            {
                return this.alphabet ?? DefaultAlphabet;
            }

            set
            {
                this.alphabet = value;
            }
        }

        /// <summary>Gets the unique likely prefixes for the password.</summary>
        /// <returns>The unique likely password prefixes.</returns>
        public IReadOnlyCollection<string> Prefixes =>
            this.HasPrefixes ? this.prefixes.AsReadOnly() : (IReadOnlyCollection<string>)new[] { string.Empty };

        /// <summary>Gets a value indicating whether <see cref="Prefixes" /> is non-empty.</summary>
        /// <returns><value>true</value> if <see cref="Prefixes" /> is non-empty; otherwise <value>false</value>.</returns>
        public bool HasPrefixes => this.prefixes.Count != 0 && (this.prefixes.Count != 1 || !string.IsNullOrEmpty(this.prefixes[0]));

        /// <summary>Gets the unique likely infixes (i.e. something in the middle) for the password.</summary>
        /// <returns>The unique likely password infixes.</returns>
        public IReadOnlyCollection<string> Infixes =>
            this.HasInfixes ? this.infixes.AsReadOnly() : (IReadOnlyCollection<string>)new[] { string.Empty };

        /// <summary>Gets a value indicating whether <see cref="Infixes" /> is non-empty.</summary>
        /// <returns><value>true</value> if <see cref="Infixes" /> is non-empty; otherwise <value>false</value>.</returns>
        public bool HasInfixes => this.infixes.Count != 0 && (this.infixes.Count != 1 || !string.IsNullOrEmpty(this.infixes[0]));

        /// <summary>Gets the lunique ikely suffixes for the password.</summary>
        /// <returns>The unique likely password suffixes.</returns>
        public IReadOnlyCollection<string> Suffixes =>
            this.HasSuffixes ? this.suffixes.AsReadOnly() : (IReadOnlyCollection<string>)new[] { string.Empty };

        /// <summary>Gets a value indicating whether <see cref="Suffixes" /> is non-empty.</summary>
        /// <returns><value>true</value> if <see cref="Suffixes" /> is non-empty; otherwise <value>false</value>.</returns>
        public bool HasSuffixes => this.suffixes.Count != 0 && (this.suffixes.Count != 1 || !string.IsNullOrEmpty(this.suffixes[0]));

        /// <summary>
        ///     Gets the cross-join of all fixes: <see cref="Prefixes" />, <see cref="Infixes" />, and <see cref="Suffixes" />.
        /// </summary>
        /// <returns>The cross join of all fixes.</returns>
        public IReadOnlyList<Fixes> AllFixes => this.Prefixes
            .SelectMany(prefix => this.Infixes, (prefix, infix) => new { prefix, infix })
            .SelectMany(pi => this.Suffixes, (pi, suffix) => new Fixes(pi.prefix, pi.infix, suffix)).ToArray();

        /// <summary>Gets a value indicating whether <see cref="AllFixes" /> is non-empty.</summary>
        /// <returns><value>true</value> if <see cref="AllFixes" /> is non-empty; otherwise <value>false</value>.</returns>
        public bool HasFixes => this.HasPrefixes || this.HasInfixes || this.HasSuffixes;

        /// <summary>
        ///     Gets or sets the likely minimum (inclusive) length for the password. Note that if the min and <see cref="MaxLength" /> are
        ///     the same, that is saying that the password is explicitly that long.
        /// </summary>
        /// <returns>The likely lower bound (inclusive) for the password length.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value" /> is negative or greater than <see cref="MaxLength" />.</exception>
        public int? MinLength
        {
            get
            {
                return this.min;
            }

            set
            {
                if (value != null)
                {
                    if (value.Value < 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            value,
                            string.Format(Thread.CurrentThread.CurrentCulture, Strings.Arg_NonNegative, nameof(value)));
                    }
                    else if (value.Value > this.MaxLength)
                    {
                        var message = string.Format(
                            Thread.CurrentThread.CurrentCulture,
                            Strings.Arg_GreaterThanOrEqualTo,
                            nameof(this.MaxLength),
                            nameof(this.MinLength));
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            value,
                            message);
                    }
                }

                this.min = value;
            }
        }

        /// <summary>
        ///     Gets or sets the likely maximum (inclusive) length for the password. Note that if the max and <see cref="MinLength" /> are
        ///     the same, that is saying that the password is explicitly that long.
        /// </summary>
        /// <returns>The likely upper bound (inclusive) for the password length.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value" /> is negative or less than <see cref="MinLength" />.</exception>
        public int? MaxLength
        {
            get
            {
                return this.max;
            }

            set
            {
                if (value != null)
                {
                    if (value.Value < 0)
                    {
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            value,
                            string.Format(Thread.CurrentThread.CurrentCulture, Strings.Arg_NonNegative, nameof(value)));
                    }
                    else if (value.Value < this.MinLength)
                    {
                        var message = string.Format(
                            Thread.CurrentThread.CurrentCulture,
                            Strings.Arg_GreaterThanOrEqualTo,
                            nameof(this.MaxLength),
                            nameof(this.MinLength));
                        throw new ArgumentOutOfRangeException(
                            nameof(value),
                            value,
                            message);
                    }
                }

                this.max = value;
            }
        }

        /// <summary>
        ///     Generates the set of all passwords this set of hints results in. If <see cref="MinLength" /> is not defined, then a minimum
        ///     length of 0 is used. If a <see cref="MaxLength" /> is not defined, then a maximim length of 16 is used.
        /// </summary>
        /// <returns>The set of passwords possible given the hints specified.</returns>
        public IEnumerable<string> GeneratePasswords()
        {
            var min = this.min ?? DefaultMin;
            var max = this.max ?? DefaultMax;
            var count = max - min + 1;
            return Enumerable.Range(min, count)
                .SelectMany(length => this.AllFixes, (length, fix) => new { length, fix })
                .SelectMany(lf =>
                {
                    var remainder = lf.length - lf.fix.Length;
                    if (remainder < 0)
                    {
                        return Array.Empty<string>();
                    }

                    return new FixedLengthPasswordCollection(remainder, this.Alphabet)
                        .SelectMany(intermediate => lf.fix.ToStrings(intermediate));
                });
        }

        /// <summary>
        ///     Adds the <paramref name="prefix" /> to the set of <see cref="Prefixes" /> if it is not already there. Note that
        ///     <value>null</value> is interpreted as <see cref="string.Empty" />.
        /// </summary>
        /// <param name="prefix">The prefix to add.</param>
        public void AddPrefix(string prefix)
        {
            AddUniqueToCollection(this.prefixes, prefix);
        }

        /// <summary>
        ///     Adds the <paramref name="infix" /> to the set of <see cref="Infixes" /> if it is not already there. Note that
        ///     <value>null</value> is interpreted as <see cref="string.Empty" />.
        /// </summary>
        /// <param name="infix">The infix to add.</param>
        public void AddInfix(string infix)
        {
            AddUniqueToCollection(this.infixes, infix);
        }

        /// <summary>
        ///     Adds the <paramref name="suffix" /> to the set of <see cref="Suffixes" /> if it is not already there. Note that
        ///     <value>null</value> is interpreted as <see cref="string.Empty" />.
        /// </summary>
        /// <param name="suffix">The suffix to add.</param>
        public void AddSuffix(string suffix)
        {
            AddUniqueToCollection(this.suffixes, suffix);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var builder = new StringBuilder("(");

            AppendList(builder, this.prefixes);
            this.AppendAlphabet(builder);

            if (this.HasInfixes)
            {
                AppendList(builder, this.infixes);
                this.AppendAlphabet(builder);
            }

            AppendList(builder, this.suffixes);

            builder.Append("){");
            builder.Append(this.MinLength);
            builder.Append(",");
            builder.Append(this.MaxLength);
            builder.Append("}");

            return builder.ToString();
        }

        private static void AppendList(StringBuilder builder, IReadOnlyCollection<string> list)
        {
            if (list.Count == 0)
            {
                return;
            }

            builder.Append("(");
            foreach (var item in list)
            {
                builder.Append(item);
                builder.Append("|");
            }

            if (builder[builder.Length - 1] == '|')
            {
                builder[builder.Length - 1] = ')';
            }
            else
            {
                builder.Append(")");
            }
        }

        private static void AddUniqueToCollection(IList<string> collection, string value)
        {
            value = value ?? string.Empty;

            if (!collection.Contains(value))
            {
                collection.Add(value);
            }
        }

        private void AppendAlphabet(StringBuilder builder)
        {
            builder.Append("[");
            builder.Append(this.Alphabet);
            builder.Append("]*");
        }
    }
}
