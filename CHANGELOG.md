# Changelog

## 0.1.0 (2026-10-25)

First public version. Work in progress: APIs may change before 1.0.

- Gully ID: OIDC authorization code + PKCE (`GullyIdClient`), id_token claim decoding (unverified; verify server side),
  refresh, `/v1/me`, loopback redirect listener for desktop/editor, deep-link helper for mobile.
- Gully Chain: `JsonRpcClient` (eth_chainId, eth_blockNumber, eth_getBalance, eth_call, eth_gasPrice,
  eth_getBlockByNumber), `Abi` encode/decode for static types + dynamic bytes, `Keccak256`, EIP-55 checksum, `Erc20`
  reads and transfer calldata, `GullyChain` constants (testnet 485590).
- Gully Pay: `X402Client` (parse 402 requirements, pick the Gully Chain option, build the payment tx, retry with the
  `X-PAYMENT` header). Verification happens on your server through the Gully Pay API.
- Gully Arcade: `GullyArcadeClient` (run tickets, score attestation, `submitScore` calldata, leaderboard and season
  reads).
- `ITransactionSender` is the single wallet seam; no key custody in the SDK.
