using backend.Models;

namespace backend.ViewModels;

public class AccountsViewModel {
    public IEnumerable<AccountViewModel> Accounts { get; set; } = new List<AccountViewModel>();

    public PagingInfo PagingInfo { get; set; } = new PagingInfo();
}