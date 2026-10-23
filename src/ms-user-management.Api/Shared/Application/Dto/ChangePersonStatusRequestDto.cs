namespace ms_user_management.Api.Shared.Application.Dto;

/// <summary>
/// Estado nuevo de la cuenta. Se recibe como cadena (<c>Active</c> /
/// <c>Inactive</c>) para alinear el cuerpo HTTP con el evento publicado y no
/// obligar al frontend a conocer el ordinal del enum.
/// </summary>
public class ChangePersonStatusRequestDto
{
    public string Status { get; set; }
}
