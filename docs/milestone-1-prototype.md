# Milestone 1 Prototype

## Scope

This milestone implements only a one-PC local WPF prototype.

Included:

- fullscreen always-on-top check-in screen
- student ID field
- student name field
- login button
- fake successful login flow
- simple unlocked state after login
- logout button
- return to lock/check-in screen after logout

Not included in this milestone:

- Supabase
- web dashboard
- Windows service
- real session persistence
- advanced blocking or kiosk hardening

## File Structure

```text
apps/client-wpf/
  PCLab.Client.sln
  src/
    PCLab.Client/
      App.xaml
      App.xaml.cs
      PCLab.Client.csproj
      Models/
        MockSession.cs
      Services/
        MockCheckInService.cs
      ViewModels/
        AsyncCommand.cs
        ObservableObject.cs
        ShellViewModel.cs
      Views/
        ShellWindow.xaml
        ShellWindow.xaml.cs
```

## File Explanations

### `PCLab.Client.sln`

Solution file for opening the prototype in Visual Studio.

### `App.xaml`

Application-level XAML resources. This milestone only uses a built-in boolean-to-visibility converter to switch between locked and unlocked UI states.

### `App.xaml.cs`

Application startup entry point. It creates the mock service, the main view model, and the fullscreen shell window.

### `PCLab.Client.csproj`

Minimal WPF project definition for the prototype.

### `Models/MockSession.cs`

Simple in-memory model returned by the mock login service after a fake successful login.

### `Services/MockCheckInService.cs`

Local fake login service. It simulates a short delay and returns a successful mock session without calling any backend.

### `ViewModels/ObservableObject.cs`

Base MVVM helper for property change notifications.

### `ViewModels/AsyncCommand.cs`

Simple async command helper for login and logout button actions.

### `ViewModels/ShellViewModel.cs`

Main view model for milestone 1. It owns:

- locked vs unlocked screen state
- student ID input
- student name input
- fake login flow
- logout flow
- simple status and error text

### `Views/ShellWindow.xaml`

Single fullscreen always-on-top window. It contains both:

- the locked check-in UI
- the unlocked state UI

The visible section changes based on the current view model state.

### `Views/ShellWindow.xaml.cs`

Small window behavior class that keeps the prototype fullscreen and topmost.

## Minimal Code Boundary For Milestone 1

The implementation is intentionally limited to:

- one fullscreen window
- one main view model
- one mock check-in service
- one in-memory session model

There is no networking, no persistence, no admin UI, and no background process in this milestone.

## Where Supabase Integration Will Be Added Later

Future integration should be added by replacing or extending `Services/MockCheckInService.cs`.

Planned future direction:

- replace `MockCheckInService` with a real API client service
- add session start and end persistence in that service layer
- keep `ShellViewModel` focused on UI state while delegating real login/logout to the service layer
- optionally split the shell into more view models once real session lifecycle logic is introduced

The main UI files that should stay mostly stable later are:

- `ViewModels/ShellViewModel.cs`
- `Views/ShellWindow.xaml`

## What Is Intentionally Mocked Now

These are fake or local-only in milestone 1:

- login success always works when both fields are filled
- no student validation against a real roster
- no session ID from a backend
- no idle tracking
- no local storage
- no startup recovery
- no dashboard visibility
- no teacher/admin override
- no keyboard blocking beyond the fullscreen topmost window behavior

## Demo Flow

1. Launch the WPF app.
2. The fullscreen lock/check-in screen is shown.
3. Enter student ID and student name.
4. Click `Login`.
5. The app shows the unlocked state with the entered student information.
6. Click `Logout`.
7. The app returns to the lock/check-in screen.
