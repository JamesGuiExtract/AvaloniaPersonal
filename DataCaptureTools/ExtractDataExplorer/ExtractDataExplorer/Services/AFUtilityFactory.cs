using UCLID_AFUTILSLib;

namespace ExtractDataExplorer.Services
{
    public interface IAFUtilityFactory
    {
        IAFUtility Create();
    }

    public class AFUtilityFactory : IAFUtilityFactory
    {
        public IAFUtility Create()
        {
            return new AFUtilityClass();
        }
    }
}
