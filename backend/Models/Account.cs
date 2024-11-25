using System;
using System.Collections.Generic;
using backend.Enums;

namespace backend.Models;

public partial class Account
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string OuterUid { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsEmailVerified { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastSignIn { get; set; }

    public string PasswordHash { get; set; } = null!;
    
    public AccountType Type { get; set; }
    
    public ProviderType Provider { get; set; }

    public Guid? CreatedBy { get; set; }

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<CongestionRating> CongestionRatings { get; set; } = new List<CongestionRating>();

    public virtual Account? CreatedByNavigation { get; set; }

    public virtual ICollection<Gym> Gyms { get; set; } = new List<Gym>();

    public virtual ICollection<Account> InverseCreatedByNavigation { get; set; } = new List<Account>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Ownership> OwnershipRequestedByNavigations { get; set; } = new List<Ownership>();

    public virtual ICollection<Ownership> OwnershipRespondedByNavigations { get; set; } = new List<Ownership>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual RequestPause? RequestPause { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual UserToken? UserToken { get; set; }
}
