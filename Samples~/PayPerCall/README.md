# Pay per call (x402)

`X402Client.GetAsync` fetches a resource; on a 402 it reads the Gully Pay requirements, pays the Gully Chain option
with your `ITransactionSender` and retries with the `X-PAYMENT` header. Your resource server verifies the payment
through Gully Pay (`POST /x402/pay` with its API key) and serves the content. The sample uses a logging stub sender.
