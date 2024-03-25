namespace compenza.authentication.api.Payloads.Request
{
    public record LoginCredentialsRequest( string LoginName, string Password, int Language = 1);
}
