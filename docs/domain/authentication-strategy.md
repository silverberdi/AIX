# Authentication Strategy

## Principle

Authentication is extensible, but the MVP remains simple.

Authentication and authorization are separate concerns.

## MVP

The MVP supports:
- local username/password authentication

## Future Iterations

Possible future capabilities:
- MFA
- authenticator apps
- Google authentication
- Microsoft/Entra ID
- OIDC
- passkeys
- enterprise SSO

## Authorization

Authorization remains policy-first and independent from authentication providers.

## Tenant Variability

Different tenants may require different authentication strategies.

AIX should evolve incrementally instead of trying to support every authentication model from the beginning.
