# checkSession.html

Allows checking the active session. To perform the check, you need to obtain:
- `session_state`, which is taken from the query parameters after the `/connect/authorize` redirect;
- `clientId` used for the `/connect/authorize` request;
- the full address to `.well-known/openid-configuration`.