// -----------------------------------------------------------------------
// <copyright file="SymmetricAlgorithmProvider.cs" company="Project Contributors">
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
using System.Security.Cryptography;

namespace MicroLite.Infrastructure
{
    /// <summary>
    /// A base class for ISymmetricAlgorithmProvider implementations.
    /// </summary>
    public abstract class SymmetricAlgorithmProvider : ISymmetricAlgorithmProvider
    {
        private string _algorithm;
        private byte[] _keyBytes;

        /// <summary>
        /// Creates an instance of the symmetric algorithm to be used for encryption and decryption.
        /// </summary>
        /// <returns>
        /// An instance of the required symmetric algorithm.
        /// </returns>
        public SymmetricAlgorithm CreateAlgorithm()
        {
            var symmetricAlgorithm = SymmetricAlgorithm.Create(_algorithm);
            symmetricAlgorithm.Key = _keyBytes;

            return symmetricAlgorithm;
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="SymmetricAlgorithmProvider"/> class.
        /// </summary>
        /// <param name="algorithmName">The algorithm name.</param>
        /// <param name="algorithmKey">The key bytes.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if algorithmName or algorithmKey is null.
        /// </exception>
        protected void Configure(string algorithmName, byte[] algorithmKey)
        {
            if (string.IsNullOrEmpty(algorithmName))
            {
                throw new ArgumentNullException(nameof(algorithmName));
            }

            if (algorithmKey is null)
            {
                throw new ArgumentNullException(nameof(algorithmKey));
            }

            _algorithm = algorithmName;
            _keyBytes = algorithmKey;
        }
    }
}
