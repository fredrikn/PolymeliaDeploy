namespace PolymeliaDeploy.Security
{
    public class TokenManagement : ITokenManagement
    {
        public string Generate(string value)
        {
            return value;
        }

        public bool IsEqual(string tooken1, string tooken2)
        {
            return tooken1.Equals(tooken2);
        }
    }
}
