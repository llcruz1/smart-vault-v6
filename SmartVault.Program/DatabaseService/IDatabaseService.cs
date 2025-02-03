using System.Collections.Generic;

namespace SmartVault.Program
{
    public interface IDatabaseService
    {
        public List<string> GetAllFilePaths();
        public List<string> GetEveryThirdFilePathContainingText(string accountId, string searchText);
    }
}
