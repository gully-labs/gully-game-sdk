# Sign in with Gully

Desktop/editor flow: open the system browser at the authorization URL, catch the redirect on the loopback listener,
exchange the code with PKCE, read the id_token claims. Register a **public** OIDC client at Gully ID with redirect
`http://127.0.0.1:53682/callback` (see gully-id/docs/integration.md) and put its client id in the inspector.
Verify tokens on your server with the JWKS before trusting `sub`.
