using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using BeeQ.FileStorage.Service;
using System.Security.Cryptography;
using System.Text;

namespace BeeQ.FileStorage.AWSS3;

#pragma warning disable S101

internal partial class AWSS3Service<TId> : FileStorageService<TId>, IAWSS3Service<TId>, IDisposable
{
    private async Task OnDelete(IFileInfo<TId> info)
    {
        await GetClient().DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = this.Options.BucketName,
            Key = info.FullPath
        });
    }

    private async Task<Stream?> OnGetStream(IFileInfo<TId> info)
    {
        var response = await GetClient().GetObjectAsync(new GetObjectRequest
        {
            BucketName = this.Options.BucketName,
            Key = info.FullPath
        });

        return response.ResponseStream;
    }

    private async Task OnUploadStream(IFileUploadInfo<TId, Stream> info)
    {
        await GetClient().PutObjectAsync(new PutObjectRequest
        {
            BucketName = this.Options.BucketName,
            Key = info.FullPath,
            ContentType = "application/octet-stream",
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
            InputStream = info.Content
        });
    }

    public async Task<S3Object[]> GetFiles(string prefix, string? bucketName = null)
    {
        var list = new List<S3Object>();

        var request = new ListObjectsV2Request()
        {
            BucketName = bucketName ?? this.Options.BucketName,
            Prefix = prefix
        };

        do
        {
            var response = await GetClient().ListObjectsV2Async(request);

            list.AddRange(response.S3Objects);

            request.ContinuationToken = response.NextContinuationToken;

        } while (request.ContinuationToken is not null);

        return [.. list];
    }

    public async Task<string?> GetTemporalyUrl(TId id, TimeSpan duracion, string? bucketName = null)
    {
        var info = GetFileInfo(id);
        if (info is null) return null;

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName ?? this.Options.BucketName,
            Key = info.FullPath,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(duracion)
        };

        return await GetClient().GetPreSignedURLAsync(request);
    }


    public async Task<GetObjectMetadataResponse?> GetFileMetadata(TId id, string? bucketName = null)
    {
        var info = GetFileInfo(id);
        if (info is null) return null;
        
        var request = new GetObjectMetadataRequest
        {
            BucketName = bucketName ?? this.Options.BucketName,
            Key = info.FullPath
        };

        return await GetClient().GetObjectMetadataAsync(request);
    }

    public async Task CopyFile(string sourceKey, string destinationKey)
    {
        await CopyFile(null, sourceKey, null, destinationKey);
    }

    public async Task CopyFile(string? sourceBucket, string sourceKey, string? destinationBucket, string destinationKey)
    {
        await GetClient().CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = sourceBucket ?? this.Options.BucketName,
            SourceKey = sourceKey,

            DestinationBucket = destinationBucket ?? this.Options.BucketName,
            DestinationKey = destinationKey
        });
    }

    public async Task MoveFile(string sourceKey, string destinationKey)
    {
        await MoveFile(null, sourceKey, null, destinationKey);
    }

    public async Task MoveFile(string? sourceBucket, string sourceKey, string? destinationBucket, string destinationKey)
    {
        await CopyFile(sourceBucket, sourceKey, destinationBucket, destinationKey);

        await GetClient().DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = sourceBucket ?? this.Options.BucketName,
            Key = sourceKey
        });
    }

    public async Task SetFileTags(TId id, IReadOnlyDictionary<string, string> tags)
    {
        var info = GetFileInfo(id);
        if (info is null) return;

        var tagSet = tags.Select(x => new Tag { Key = x.Key, Value = x.Value }).ToList();
        await GetClient().PutObjectTaggingAsync(new PutObjectTaggingRequest
        {
            BucketName = this.Options.BucketName,
            Key = info.FullPath,
            Tagging = new Tagging
            {
                TagSet = tagSet
            }
        });
    }

    public async Task<IReadOnlyDictionary<string, string>> GetFileTags(TId id)
    {
        var info = GetFileInfo(id);
        if (info is null) return new Dictionary<string, string>();

        var response = await GetClient().GetObjectTaggingAsync(new GetObjectTaggingRequest
        {
            BucketName = this.Options.BucketName,
            Key = info.FullPath
        });

        return response.Tagging.ToDictionary(t => t.Key, t => t.Value);
    }
}


#pragma warning restore S101