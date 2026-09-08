using Amazon.S3.Model;
using BeeQ.FileStorage.Service;

namespace BeeQ.FileStorage.AWSS3;

#pragma warning disable S101

/// <summary>
/// AWS S3 file storage service using <see cref="Guid"/> as the identifier type.
/// </summary>
public interface IAWSS3Service : IAWSS3Service<Guid> { }

/// <summary>
/// Provides AWS S3 file storage operations for entities identified by <typeparamref name="TId"/>.
/// Implementations of this interface expose common file operations such as listing, copying,
/// moving, tagging and retrieving metadata or temporary URLs for stored objects.
/// </summary>
/// <typeparam name="TId">Type used to identify stored files.</typeparam>
public interface IAWSS3Service<TId> : IFileStorage<TId>
{
    /// <summary>
    /// Retrieves objects from the configured S3 bucket that match the specified key prefix.
    /// </summary>
    /// <param name="prefix">The key prefix to filter objects by (can be a folder-like prefix).</param>
    /// <param name="bucketName">Optional bucket name. If null, the default configured bucket is used.</param>
    /// <returns>An array of <see cref="S3Object"/> instances matching the prefix. Empty array if none found.</returns>
    Task<S3Object[]> GetFiles(string prefix, string? bucketName = null);

    /// <summary>
    /// Generates a temporary (pre-signed) URL for the file identified by <paramref name="id"/>.
    /// The returned URL is valid for the specified duration.
    /// </summary>
    /// <param name="id">Identifier of the file for which to generate the temporary URL.</param>
    /// <param name="duracion">The time span that the temporary URL should remain valid.</param>
    /// <param name="bucketName">Optional bucket name. If null, the default configured bucket is used.</param>
    /// <returns>A pre-signed URL as a <see cref="string"/>, or <c>null</c> if the file does not exist or a URL could not be created.</returns>
    Task<string?> GetTemporalyUrl(TId id, TimeSpan duracion, string? bucketName = null);

    /// <summary>
    /// Retrieves the metadata for the file identified by <paramref name="id"/> from S3.
    /// </summary>
    /// <param name="id">Identifier of the file whose metadata will be retrieved.</param>
    /// <param name="bucketName">Optional bucket name. If null, the default configured bucket is used.</param>
    /// <returns>An AWS <see cref="GetObjectMetadataResponse"/> containing metadata information, or <c>null</c> if the file is not found.</returns>
    Task<GetObjectMetadataResponse?> GetFileMetadata(TId id, string? bucketName = null);

    /// <summary>
    /// Copies a file from <paramref name="sourceKey"/> to <paramref name="destinationKey"/> within the default bucket.
    /// </summary>
    /// <param name="sourceKey">The key of the source object to copy.</param>
    /// <param name="destinationKey">The key for the destination object.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    Task CopyFile(string sourceKey, string destinationKey);

    /// <summary>
    /// Copies a file from the specified source bucket/key to the specified destination bucket/key.
    /// </summary>
    /// <param name="sourceBucket">Source bucket name. If null, the default configured bucket is used.</param>
    /// <param name="sourceKey">The key of the source object to copy.</param>
    /// <param name="destinationBucket">Destination bucket name. If null, the default configured bucket is used.</param>
    /// <param name="destinationKey">The key for the destination object.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    Task CopyFile(string? sourceBucket, string sourceKey, string? destinationBucket, string destinationKey);

    /// <summary>
    /// Moves (copies then deletes) a file from <paramref name="sourceKey"/> to <paramref name="destinationKey"/> within the default bucket.
    /// </summary>
    /// <param name="sourceKey">The key of the source object to move.</param>
    /// <param name="destinationKey">The key for the destination object.</param>
    /// <returns>A task that represents the asynchronous move operation.</returns>
    Task MoveFile(string sourceKey, string destinationKey);

    /// <summary>
    /// Moves (copies then deletes) a file from the specified source bucket/key to the specified destination bucket/key.
    /// </summary>
    /// <param name="sourceBucket">Source bucket name. If null, the default configured bucket is used.</param>
    /// <param name="sourceKey">The key of the source object to move.</param>
    /// <param name="destinationBucket">Destination bucket name. If null, the default configured bucket is used.</param>
    /// <param name="destinationKey">The key for the destination object.</param>
    /// <returns>A task that represents the asynchronous move operation.</returns>
    Task MoveFile(string? sourceBucket, string sourceKey, string? destinationBucket, string destinationKey);

    /// <summary>
    /// Replaces the tags on the file identified by <paramref name="id"/> with the provided tags.
    /// </summary>
    /// <param name="id">Identifier of the file to tag.</param>
    /// <param name="tags">A read-only dictionary of tag key/value pairs to apply to the file.</param>
    /// <returns>A task that represents the asynchronous tag update operation.</returns>
    Task SetFileTags(TId id, IReadOnlyDictionary<string, string> tags);

    /// <summary>
    /// Retrieves the tags associated with the file identified by <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Identifier of the file whose tags will be retrieved.</param>
    /// <returns>A read-only dictionary containing tag key/value pairs. Empty if no tags are present.</returns>
    Task<IReadOnlyDictionary<string, string>> GetFileTags(TId id);
}

#pragma warning restore S101
