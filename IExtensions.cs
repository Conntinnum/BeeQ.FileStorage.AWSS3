using BeeQ.FileStorage.AWSS3;
using BeeQ.FileStorage.Builder;

namespace BeeQ.FileStorage;

public static class IExtensions
{
    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="options">Configuration options for the Local Disk storage</param>
    /// <returns>The configured file storage</returns>
    public static IAWSS3Service UseAWSS3(this IFileStorageBuilder builder, string schemaName, Action<AWSS3Configuration> options)
    {
        var opt = new AWSS3Configuration();
        options.Invoke(opt);
        return builder.Use<Guid>(schemaName)
            .CustomBuild<AWSS3Service, IAWSS3Service>(() => new AWSS3Service(opt));
    }

    /// <summary>
    /// Extension to use the Local Disk to storage the Files
    /// </summary>
    /// <typeparam name="TId">The type of the Id used to identify the files</typeparam>
    /// <param name="builder">Empty File Storage Builder</param>
    /// <param name="schemaName">Optional schema name used to distinguish storage namespaces</param>
    /// <param name="options">Configuration options for the Local Disk storage</param>
    /// <returns>The configured file storage</returns>
    public static IAWSS3Service<TId> UseAWSS3<TId>(this IFileStorageBuilder builder, string schemaName, Action<AWSS3Configuration<TId>> options)
    {
        var opt = new AWSS3Configuration<TId>();
        options.Invoke(opt);
        return builder.Use<TId>(schemaName)
            .CustomBuild<AWSS3Service<TId>, IAWSS3Service<TId>>(() => new AWSS3Service<TId>(opt));
    }
}
