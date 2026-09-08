using Amazon.S3;
using BeeQ.FileStorage.LocalDisk;
using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.AWSS3;

#pragma warning disable S101

public class AWSS3Configuration : AWSS3Configuration<Guid> { }

public class AWSS3Configuration<TId>
{
    public AmazonS3Config? CustomConfig { get; set; }
    public string? AccessKey { get; set; }
    public string? SecretKey { get; set; }
    public string? Region { get; set; }
    public string? BucketName { get; set; }
    public string? BasePath { get; set; }

    public Func<IFileStorageFullIdentifier<TId>, string>? PathTemplate { get; set; }
    public Func<TId, string>? GetFilename { get; set; }
    public Func<(string? SchemaName, string Filename), Task<TId>>? OnCreateId { get; set; }

    internal virtual void Validate()
    {
        if (PathTemplate == null && string.IsNullOrEmpty(BasePath))
            throw new BasePathAWSS3ConfigratedException();

        if (typeof(TId) != typeof(Guid) && OnCreateId == null)
            throw new CreateIdAWSS3ConfigratedException();
    }
}

#pragma warning restore S101
