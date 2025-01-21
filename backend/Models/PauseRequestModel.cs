using System;
using System.Collections.Generic;

namespace backend.Models;

public class PauseRequestModel
{
    public string Ip { get; set; }  // IP address from the client (can be null for authenticated users)
}