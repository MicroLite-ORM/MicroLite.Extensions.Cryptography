// -----------------------------------------------------------------------
// <copyright file="DbEncryptedString.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
using System;

namespace MicroLite
{
    /// <summary>
    /// A class which represents a string which is encrypted before being written to the database
    /// and decrypted after being read from the database.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{value}")]
    public sealed class DbEncryptedString : IEquatable<DbEncryptedString>, IEquatable<string>
    {
        private readonly string _value;

        private DbEncryptedString(string value) => _value = value;

        /// <summary>
        /// Returns a DbEncryptedString containing the value of the specified string.
        /// </summary>
        /// <param name="value">The string to convert value.</param>
        /// <returns>A DbEncryptedString containing the value of the specified string.</returns>
        public static implicit operator DbEncryptedString(string value) => value == null ? null : new DbEncryptedString(value);

        /// <summary>
        /// Returns a string containing the value of the DbEncryptedString.
        /// </summary>
        /// <param name="dbEncryptedString">The db encrypted string.</param>
        /// <returns>A string containing the value of the DbEncryptedString.</returns>
        public static implicit operator string(DbEncryptedString dbEncryptedString) => dbEncryptedString?._value;

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is DbEncryptedString dbEncryptedString)
            {
                return Equals(dbEncryptedString);
            }

            return Equals(obj as string);
        }

        /// <inheritdoc/>
        public bool Equals(DbEncryptedString other)
        {
            if (other is null)
            {
                return false;
            }

            return Equals(other._value);
        }

        /// <inheritdoc/>
        public bool Equals(string other)
        {
            if (other is null)
            {
                return false;
            }

            return _value == other;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => _value.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => _value;
    }
}
