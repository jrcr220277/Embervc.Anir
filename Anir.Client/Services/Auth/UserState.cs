using Anir.Shared.Contracts.Auth;
using Anir.Shared.Enums;
using System;
using System.Collections.Generic;

namespace Anir.Client.Services.Auth;

public class UserState
{
    // ============================
    // PROPIEDADES DEL USUARIO
    // ============================
    public string? Id { get; private set; }
    public string? Email { get; private set; }
    public bool EmailConfirmed { get; private set; }

    public string? FullName { get; private set; }
      
    public string? ImagenId { get; private set; }

     public string? ImagenUrl { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public bool Active { get; private set; }
    public ThemeMode ThemeMode { get; private set; } = ThemeMode.Auto;


    // ============================
    // EVENTO DE CAMBIO
    // ============================
    public event Action? OnChange;
    private void Notify() => OnChange?.Invoke();

    // ============================
    // SET (cargar datos desde UserResponse)
    // ============================
    public void Set(UserResponse user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        Id = user.Id;
        Email = user.Email;
        EmailConfirmed = user.EmailConfirmed;

        FullName = user.FullName;

        ImagenId = user.ImagenId;      // <-- ID real del archivo
        ImagenUrl = user.ImagenUrl;    // <-- URL pública para mostrar

        Roles = user.Roles ?? new List<string>();
        Active = user.Active;
        ThemeMode = user.ThemeMode;

        Notify();
    }

    // ============================
    // CLEAR (logout)
    // ============================
    public void Clear()
    {
        Id = null;
        Email = null;
        EmailConfirmed = false;

        FullName = null;
        ImagenId = null;
        ImagenUrl = null;

        Roles.Clear();
        Active = false;
        ThemeMode = ThemeMode.Auto;


        Notify();
    }

    // ============================
    // PROPIEDADES DERIVADAS
    // ============================
    public string DisplayName =>
        string.IsNullOrWhiteSpace(FullName)
            ? (string.IsNullOrWhiteSpace(Email) ? "Usuario" : Email)
            : FullName;

    public string PrimaryRole =>
        (Roles == null || Roles.Count == 0) ? "Sin roles" : Roles[0];

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FullName)) return "U";
            var parts = FullName!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}
