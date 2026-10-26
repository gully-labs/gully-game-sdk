# Arcade Quickstart

Start a run, finish it with an input log, attest the score and build the `submitScore` transaction. The on-chain
submit needs an `ITransactionSender` (the player's Gully passkey account); the sample ships a `LoggingSender` stub that
prints the calldata instead of sending. Reads the live leaderboard from Gully Chain Testnet on start.
