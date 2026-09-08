namespace BeeQ.FileStorage.AWSS3;

#pragma warning disable S101

public class BasePathAWSS3ConfigratedException : System.Exception
{
    public BasePathAWSS3ConfigratedException() : base("Basepath is not configurated") { }
}
public class CreateIdAWSS3ConfigratedException : System.Exception
{
    public CreateIdAWSS3ConfigratedException() : base("CreateId is not configurated") { }
}

#pragma warning restore S101
