using System;
using backend.Enums;

namespace backend.DTO;
public class SavePauseDto
{
	public Guid UserId { get; set; }
	public string IpAddress { get; set; } = null!;
}