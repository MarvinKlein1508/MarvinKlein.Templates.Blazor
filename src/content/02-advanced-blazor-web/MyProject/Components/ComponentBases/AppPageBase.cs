using MyProject.Domain.Entities;
using MyProject.Infrastructure.Auth;
using MyProject.Infrastructure.Database;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MyProject.Components.ComponentBases;

public abstract class AppPageBase : ComponentBase
{
    [Inject] protected AuthService AuthService { get; set; } = default!;
    [Inject] protected IDbConnectionFactory DbFactory { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] protected ProtectedLocalStorage LocalStorage { get; set; } = default!;
    //[Inject] protected IToastService ToastService { get; set; } = default!;
    //[Inject] protected IMessageService MessageService { get; set; } = default!;
    [Inject] protected IDialogService DialogService { get; set; } = default!;

    protected User? CurrentUser { get; set; }

    protected override async Task OnInitializedAsync()
    {
        CurrentUser = await AuthService.GetUserAsync();
        //Log.Information("Visit: {url}; UserId: {user}", NavigationManager.Uri, CurrentUser?.DisplayName.ToString() ?? "<UNBEKANNT>");
    }
}
