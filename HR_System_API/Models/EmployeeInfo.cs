using System;
using System.Collections.Generic;

namespace HR_System_API.Models;

public partial class EmployeeInfo
{
    public string EmployeeId { get; set; } = null!;

    public string? Name { get; set; }

    public string? EnglishName { get; set; }

    public string? Account { get; set; }

    public string? Password { get; set; }

    public DateOnly? JoinDate { get; set; }

    public bool? Status { get; set; }

    public string? ContactInfo { get; set; }

    public string? Role { get; set; }
}
