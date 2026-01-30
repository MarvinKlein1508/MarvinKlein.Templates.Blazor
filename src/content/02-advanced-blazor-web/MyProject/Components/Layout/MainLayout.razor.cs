using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MyProject.Components.Layout;

public partial class MainLayout
{
    private FluentLayoutHamburger _hamburger = default!;

    [Inject]
    public required IJSRuntime JSRuntime { get; set; }

    private async Task SwitchThemeAsync()
    {
        await JSRuntime.InvokeVoidAsync("Blazor.theme.switchTheme");
    }
}
