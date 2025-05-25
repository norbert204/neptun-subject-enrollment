namespace NeptunKiller.SubjectService.Options;

public class ServiceOptions
{
    public required Uri CachingServiceUri { get; init; }

    public required Uri DatabaseServiceUri { get; init; }
}