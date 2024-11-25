using backend.Models;

namespace backend.ViewModels;

public class AccountsViewModel {
    public IEnumerable<AccountRegularModel> Accounts { get; set; } = new List<AccountRegularModel>();

    public PagingInfo PagingInfo { get; set; } = new PagingInfo();
}