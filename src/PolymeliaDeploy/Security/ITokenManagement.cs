namespace PolymeliaDeploy.Security
{
    public interface ITokenManagement
    {
        string Generate(string value);

        bool IsEqual(string tooken1, string tooken2);
    }
}