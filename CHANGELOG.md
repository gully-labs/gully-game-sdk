# Changelog

## Unreleased

- Networks: `GullyNetwork` (chain id, RPC, explorer, WETH, x402 network, arcade address and attestation URL) and
  `GullyChain.Use(network)` / `GullyChain.Network`. Testnet stays the default. `GullyNetwork.Mainnet` (48559) has no
  built-in RPC, explorer or contract values; SDK calls that need one throw `GullyNetworkNotConfiguredException`
  instead of falling back to testnet values. `JsonRpcClient`, `GullyArcadeClient`, `X402Client`,
  `TransactionRequest`, `GullyChain.TxUrl/AddressUrl/GullyRpcUrl` and the editor menu read the selected network.
- `X402Client.Choose` refuses to pick a payment option when no x402 network is configured, instead of paying the
  first option offered.
- Plain .NET tests for the network config (`Tools~/dotnet-tests`, no Unity licence needed).

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
- `GullyStudioTheme`: GullyStudio brand kit tokens and sample-scene defaults; brand files under `Documentation~/brand/`.
