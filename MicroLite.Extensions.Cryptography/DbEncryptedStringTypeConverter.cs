// -----------------------------------------------------------------------
// <copyright file="DbEncryptedStringTypeConverter.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using MicroLite.Extensions.Cryptography;
using MicroLite.Infrastructure;

namespace MicroLite.TypeConverters
{
    /// <summary>
    /// An ITypeConverter which can encrypt and decrypt a string which ensures that the cypher text is stored in the database but the
    /// clear text is used in the object.
    /// </summary>
    public sealed class DbEncryptedStringTypeConverter : ITypeConverter
    {
        private readonly ISymmetricAlgorithmProvider _algorithmProvider;
        private readonly Type _dbEncryptedStringType = typeof(DbEncryptedString);

        /// <summary>
        /// Initialises a new instance of the <see cref="DbEncryptedStringTypeConverter"/> class.
        /// </summary>
        /// <param name="algorithmProvider">The symmetric algorithm provider to be used.</param>
        public DbEncryptedStringTypeConverter(ISymmetricAlgorithmProvider algorithmProvider)
        {
            if (algorithmProvider is null)
            {
                throw new ArgumentNullException(nameof(algorithmProvider));
            }

            _algorithmProvider = algorithmProvider;

            TypeConverter.RegisterTypeMapping(_dbEncryptedStringType, DbType.String);
        }

        /// <summary>
        /// Determines whether this type converter can convert values for the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified type; otherwise, <c>false</c>.
        /// </returns>
        public bool CanConvert(Type type) => type == _dbEncryptedStringType;

        /// <summary>
        /// Converts the specified database value into an instance of the specified type.
        /// </summary>
        /// <param name="value">The database value to be converted.</param>
        /// <param name="type">The type to convert to.</param>
        /// <returns>An instance of the specified type containing the specified value.</returns>
        public object ConvertFromDbValue(object value, Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (value == DBNull.Value)
            {
                return null;
            }

            string stringValue = (string)value;

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return (DbEncryptedString)stringValue;
            }

            return (DbEncryptedString)Decrypt(stringValue);
        }

        /// <summary>
        /// Converts value at the specified index in the IDataReader into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The IDataReader containing the results.</param>
        /// <param name="index">The index of the record to read from the IDataReader.</param>
        /// <param name="type">The type to convert result value to.</param>
        /// <returns>An instance of the specified type containing the specified value.</returns>
        public object ConvertFromDbValue(IDataReader reader, int index, Type type)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (reader.IsDBNull(index))
            {
                return null;
            }

            string stringValue = reader.GetString(index);

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return (DbEncryptedString)stringValue;
            }

            return (DbEncryptedString)Decrypt(stringValue);
        }

        /// <summary>
        /// Converts the specified value into an instance of the database value.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="type">The type to convert from.</param>
        /// <returns>An instance of the corresponding database type containing the value.</returns>
        public object ConvertToDbValue(object value, Type type)
        {
            string stringValue = value?.ToString();

            if (string.IsNullOrEmpty(stringValue))
            {
                return value;
            }

            return Encrypt(stringValue);
        }

        private string Decrypt(string cipherText)
        {
            int index = cipherText.IndexOf('@');

            if (index == -1)
            {
                throw new MicroLiteException(ExceptionMessages.DbEncryptedStringTypeConverter_CipherTextInvalid);
            }

            byte[] cipherBytes = Convert.FromBase64String(cipherText.Substring(0, index));
            byte[] ivBytes = Convert.FromBase64String(cipherText.Substring(index + 1, cipherText.Length - (index + 1)));

            using (SymmetricAlgorithm algorithm = _algorithmProvider.CreateAlgorithm())
            {
                algorithm.IV = ivBytes;

                ICryptoTransform decryptor = algorithm.CreateDecryptor();

                MemoryStream memoryStream = null;
                CryptoStream cryptoStream = null;

                try
                {
                    memoryStream = new MemoryStream(cipherBytes);
                    cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    memoryStream = null;

                    using (var streamReader = new StreamReader(cryptoStream))
                    {
                        cryptoStream = null;

                        return streamReader.ReadToEnd();
                    }
                }
                finally
                {
                    if (memoryStream != null)
                    {
                        memoryStream.Dispose();
                    }

                    if (cryptoStream != null)
                    {
                        cryptoStream.Dispose();
                    }
                }
            }
        }

        private string Encrypt(string clearText)
        {
            byte[] cipherBytes = null;
            byte[] ivBytes;

            using (SymmetricAlgorithm algorithm = _algorithmProvider.CreateAlgorithm())
            {
                algorithm.GenerateIV();

                ivBytes = algorithm.IV; // should we use the IV bytes from the previous value if one exists?

                ICryptoTransform encryptor = algorithm.CreateEncryptor();

                using (var memoryStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(clearText);
                        }

                        cipherBytes = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(cipherBytes) + "@" + Convert.ToBase64String(ivBytes);
        }
    }
}
