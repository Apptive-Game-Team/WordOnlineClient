namespace Data.Versioning
{
    // Shared cache metadata used by VersionedDataSource across versioned payload types.
    public interface IVersionedResponse
    {
        bool Changed { get; set; }
        string Version { get; set; }
        string SourceUrl { get; set; }
    }
}
