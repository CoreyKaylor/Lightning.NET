using System.Text;

namespace LightningDB
{
    public interface IDatabaseAttributesProvider
    {
        DatabaseOpenFlags OpenFlags { get; }

        string Name { get; }

        Encoding Encoding { get; }
    }
}
