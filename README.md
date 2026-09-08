# BeeQ.FileStorage.AWSS3

> AWS S3 file storage provider for the BeeQ.FileStorage abstraction.

This library implements an AWS S3-backed file storage provider designed to be consumed through the BeeQ.FileStorage abstractions. It is packaged as a NuGet package named `BeeQ.FileStorage.AWSS3` and is suitable for publishing to nuget.org.

Key features
- S3 object listing and metadata retrieval
- Pre-signed (temporary) URL generation
- Copy and move operations across keys and buckets
- Object tagging (set and get)
- Pluggable identifier type (TId) with a default Guid specialization

Installation

Install the package from NuGet:

```powershell
dotnet add package BeeQ.FileStorage.AWSS3
```

Usage

The package exposes extension methods on IFileStorageBuilder to register an AWSS3-backed storage implementation. Configure the provider with an AWSS3Configuration (or AWSS3Configuration<TId> for custom identifier types).

Example — registering and using the default Guid-based service

```csharp
// Create and configure the file storage builder (pseudo-code, depends on your app)
var builder = new FileStorageBuilder();

// Register the AWSS3 provider
var s3Service = builder.UseAWSS3("my-schema", options =>
{
	options.BucketName = "my-bucket";
	options.Region = "us-east-1";
	options.AccessKey = "YOUR_ACCESS_KEY";     // Optional if using IAM role
	options.SecretKey = "YOUR_SECRET_KEY";     // Optional if using IAM role
	options.BasePath = "app-files";            // Optional base path inside the bucket
});

// Use the returned IAWSS3Service (Guid-based) to perform operations
var objects = await s3Service.GetFiles("folder/");
var tempUrl = await s3Service.GetTemporalyUrl(someGuidId, TimeSpan.FromMinutes(15));
var metadata = await s3Service.GetFileMetadata(someGuidId);

await s3Service.SetFileTags(someGuidId, new Dictionary<string,string>{{"env","prod"}});
var tags = await s3Service.GetFileTags(someGuidId);
```

Example — using a custom identifier type

```csharp
// Register a provider that uses string identifiers (for example)
var s3Service = builder.UseAWSS3<string>("my-schema", options =>
{
	options.BucketName = "my-bucket";
	options.Region = "eu-west-1";
	options.OnCreateId = async info =>
	{
		// Create and return an id for a newly uploaded file
		return Guid.NewGuid().ToString();
	};
});

// The strongly-typed API exposes the same operations using your TId
var url = await s3Service.GetTemporalyUrl("file-id-123", TimeSpan.FromHours(1));
```

API highlights
- IAWSS3Service and IAWSS3Service<TId>: primary interfaces exposing S3-backed file operations (listing, metadata, temporary URLs, copy/move, tags).
- AWSS3Configuration and AWSS3Configuration<TId>: configuration objects used to provide credentials, region, bucket name and path templates.

Configuration notes
- BasePath or PathTemplate must be provided to build object keys unless you only rely on a custom PathTemplate.
- For non-Guid identifier types, you must provide an OnCreateId delegate so the provider knows how to generate new identifiers.

Contributing

Contributions, issues and feature requests are welcome. Please follow the repository contribution guidelines and include unit tests for behavior changes when applicable.

License

This project is licensed under the Apache-2.0 License. See LICENSE for details.
