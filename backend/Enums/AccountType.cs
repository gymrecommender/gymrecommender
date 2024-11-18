using System.ComponentModel;
using System.Runtime.Serialization;
using Google.Apis.Util;

namespace backend.Enums;

public enum AccountType
{
    [Description("user")]
    user,
    [Description("gym")]
    gym,
    [Description("admin")]
    admin
}