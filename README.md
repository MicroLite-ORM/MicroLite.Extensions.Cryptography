# MicroLite.Extensions.Cryptography

##Status

|Service|Status|
|-------|------|
||[![NuGet version](https://badge.fury.io/nu/MicroLite.Extensions.Cryptography.svg)](http://badge.fury.io/nu/MicroLite.Extensions.Cryptography)|
|/develop|[![Build Status](https://dev.azure.com/trevorpilley/MicroLite-ORM/_apis/build/status/MicroLite-ORM.MicroLite.Extensions.Cryptography?branchName=develop)](https://dev.azure.com/trevorpilley/MicroLite-ORM/_build/latest?definitionId=32&branchName=develop)|
|/master|[![Build Status](https://dev.azure.com/trevorpilley/MicroLite-ORM/_apis/build/status/MicroLite-ORM.MicroLite.Extensions.Cryptography?branchName=master)](https://dev.azure.com/trevorpilley/MicroLite-ORM/_build/latest?definitionId=32&branchName=master)|

## Installation

Install the nuget package `Install-Package MicroLite.Extensions.Cryptography`

## Summary

MicroLite.Extensions.Cryptography is a .NET 4.5 library which adds an extension for the MicroLite ORM Framework to encrypt and decrypt values as they are written to or read from a database.

The extension contains the `DbEncryptedString` class which allows you to have a property value automatically encrypted upon write to and decrypted upon read from the database.

You can use it as the property type for any mapped class by simply declaring the property as a DbEncryptedString:

    public class Thing
    {
        public DbEncryptedString SecureData { get; set; }
    }

The DbEncryptedString class provides automatic conversion from a .NET string class so you can easily use it as follows:

    var thing = new Thing { SecureData = "My Secret data..." };

Once the object is saved to the database (via `ISession.Insert()` or `ISession.Update()`) the column will contain the Base64 encoded cipher text (encrypted value). When the object is read from the database it will be decrypted and the clear text will be visible in the property.

The encrypted value will look something along the lines of `MlZPjm49IKNGtOQoHAAgEa2+ycQHzXk8FIRbJ/SQ/BM=@ngWykCGsVyD/aD8ZWIhXWw==`

If you want to use `DbEncryptedString`, you need to manually register the type converter for it. The `DbEncryptedStringTypeConverter` is designed to work with any implementation of the `System.Security.Cryptography.SymmetricAlgorithm` class. In order to achieve this, the creation of the `SymmetricAlgorithm` has been abstracted from the type converter.

Notice the constructor of the type converter requires an `ISymmetricAlgorithmProvider`:

    public DbEncryptedStringTypeConverter(ISymmetricAlgorithmProvider algorithmProvider) { ... }

MicroLite.Extensions.Cryptography ships with an implementation of this interface which reads the encryption key and algorithm type from the app.config. In order to use it, add 2 values to the appsettings section:

    <appSettings>
        <add key="MicroLite.DbEncryptedString.EncryptionKey" value="bru$3atheM-pey+=!a5ebr7d6Tru@E?4" />
        <add key="MicroLite.DbEncryptedString.SymmetricAlgorithm" value="AesManaged" />
    </appSettings>

The SymmetricAlgorithm can be any which can be created by [SymmetricAlgorithm.Create](http://msdn.microsoft.com/en-us/library/k74a682y.aspx).

It is then possible to instantiate the `DbEncryptedStringTypeConverter` with the `AppSettingSymmetricAlgorithmProvider` as follows:

    using MicroLite.Infrastructure;
    using MicroLite.TypeConverter;

    TypeConverter.Converters.Add(
        new DbEncryptedStringTypeConverter(
            new AppSettingSymmetricAlgorithmProvider()));

    // Then the usual configuration for MicroLite
    Configure.Fluently()...

## Supported .NET Versions

The NuGet Package contains binaries compiled against (dependencies indented):

* .NET Framework 4.5
  * MicroLite 6.3.1
