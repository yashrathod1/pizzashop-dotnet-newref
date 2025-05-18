namespace pizzashop_service.Interface;

public interface IJwtService
{
      Task<string> GenerateJwtTokenAsync(string email, int roleId);
}
