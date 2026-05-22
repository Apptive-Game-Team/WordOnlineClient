namespace Data.Versioning
{
    public interface IVersionedResponse
    {
        string Version { get; set; }
        string SourceUrl { get; set; }
    }
}
