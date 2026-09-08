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

internal class AWSS3Service : AWSS3Service<Guid>, IAWSS3Service
{
    public AWSS3Service(AWSS3Configuration<Guid> options) : base(options)
    {
        base.Interceptors.OnCreateId = options.OnCreateId ?? OnCreateId;
        base.FullPathTemplate = options.PathTemplate ?? (info => AWSS3Service.UseCustomFullPath(info, options.BasePath!));
    }

    private static async Task<Guid> OnCreateId((string? SchemaName, string Filename) cfg)
    {
        var key = $"{cfg.SchemaName}.{cfg.Filename}";
        var bytes = Encoding.UTF8.GetBytes(key);
#pragma warning disable S4790
        var hash = MD5.HashData(bytes);
#pragma warning restore S4790

        return new Guid(hash);
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<Guid> info, string basePath)
    {
        var id = $"{info.Id}";
        return Path.Combine(basePath, id[..2], id[2..4], info.Id.ToString());
    }
}

internal partial class AWSS3Service<TId> : FileStorageService<TId>, IAWSS3Service<TId>, IDisposable
{
    public AWSS3Configuration<TId> Options { get; set; }

    private AmazonS3Client? _Client = null;

    public AWSS3Service(AWSS3Configuration<TId> options)
    {
        options.Validate();
        this.Options = options;
        base.Interceptors.OnCreateId = options.OnCreateId;
        base.Interceptors.OnGetFilename = options.GetFilename ?? GetFilename;
        base.FullPathTemplate = options.PathTemplate ?? (info => AWSS3Service<TId>.UseCustomFullPath(info, options.BasePath!));

        base.Interceptors.OnUploadStream = OnUploadStream;
        base.Interceptors.OnGetStream = OnGetStream;
        base.Interceptors.OnDelete = OnDelete;
    }

    private static string GetFilename(TId id)
    {
        return $"{id}";
    }

    private static string UseCustomFullPath(IFileStorageFullIdentifier<TId> info, string basePath)
    {
        return Path.Combine(basePath, info.Filename);
    }

    private AmazonS3Client GetClient()
    {
        if (_Client is not null)
            return _Client;

        BasicAWSCredentials? credentials = null;
        if (this.Options.AccessKey != null && this.Options.SecretKey != null)
            credentials = new BasicAWSCredentials(this.Options.AccessKey, this.Options.SecretKey);

        RegionEndpoint? region = null;
        if (!string.IsNullOrEmpty(this.Options.Region))
            region = RegionEndpoint.GetBySystemName(this.Options.Region);


        if (this.Options.CustomConfig != null)
        {
            if (credentials != null)
                return _Client = new AmazonS3Client(credentials, this.Options.CustomConfig);
            else
                return _Client = new AmazonS3Client(this.Options.CustomConfig);
        }

        if (credentials != null)
        {
            if (region != null)
                return _Client = new AmazonS3Client(credentials, region);
            else
                return _Client = new AmazonS3Client(credentials);
        }

        if (region != null)
            return _Client = new AmazonS3Client(region);

        return _Client = new AmazonS3Client();
    }

    #region Dispose

    protected virtual void Dispose(bool disposing)
    {
        _Client?.Dispose();
        _Client = null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}


#pragma warning restore S101