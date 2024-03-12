﻿namespace CanFlyPipeline.JwtAuthentication
{
    public record class JwtOptions(
        string Issuer,
        string Audience,
        string SigningKey,
        int ExpirationSeconds
    );
}
