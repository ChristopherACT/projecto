using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FG_Corporation.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        // Saca el Id del usuario autenticado desde el token JWT.
        // Solo funciona en endpoints marcados con [Authorize].
        protected int CurrentUserId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (claim == null)
                    throw new UnauthorizedAccessException("No hay un usuario autenticado.");
                return int.Parse(claim.Value);
            }
        }
    }
}