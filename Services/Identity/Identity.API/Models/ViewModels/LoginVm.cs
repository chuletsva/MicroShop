﻿namespace Identity.API.Models.ViewModels;

public record LoginVm
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public bool RememberMe { get; set; }
}
