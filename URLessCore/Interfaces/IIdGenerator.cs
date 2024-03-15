namespace URLessCore.Interfaces
{
    public interface IIdGenerator
    {
        string Generate(string url);

        string Regenerate();
    }
}
