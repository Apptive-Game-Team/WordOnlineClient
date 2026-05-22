namespace Data.Versioning
{
    // Shared cache metadata used by VersionedDataSource across versioned payload types.
    public interface IVersionedResponse
    {
        string Version { get; set; }
        string SourceUrl { get; set; }
    }
}
