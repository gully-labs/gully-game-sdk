using System;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;

namespace GullyLabs.GameSdk
{
    public class GullyRpcException : Exception
    {
        public int Code { get; }
        public GullyRpcException(int code, string message) : base($"rpc error {code}: {message}") { Code = code; }
    }

    [Serializable]
    public class RpcBlock
    {
        public string number;
        public string hash;
        public string parentHash;
        public string timestamp;
        public string gasUsed;
        public string gasLimit;
        public string baseFeePerGas;
        public string[] transactions;

        public long Number => (long)Hex.ToBigInteger(number);
        public long Timestamp => (long)Hex.ToBigInteger(timestamp);
        public int TxCount => transactions == null ? 0 : transactions.Length;
        public DateTimeOffset Time => DateTimeOffset.FromUnixTimeSeconds(Timestamp);
    }

    /// <summary>
    /// Minimal Ethereum JSON-RPC client over <see cref="GullyHttp"/>. Defaults to the public Gully Chain Testnet RPC;
    /// pass your Gully RPC gateway URL (<see cref="GullyChain.GullyRpcUrl"/>) for production traffic.
    /// </summary>
    public sealed class JsonRpcClient
    {
        [Serializable] private class RpcError { public int code; public string message; }
        [Serializable] private class StringResult { public string result; public RpcError error; }
        [Serializable] private class BlockResult { public RpcBlock result; public RpcError error; }

        public string Url { get; }
        private int _id;

        public JsonRpcClient(string url = null) { Url = string.IsNullOrEmpty(url) ? GullyChain.TestnetRpc : url; }

        public async Task<string> CallRawAsync(string method, string paramsJson)
        {
            var body = "{\"jsonrpc\":\"2.0\",\"id\":" + (++_id) + ",\"method\":" + GullyHttp.JsonString(method) + ",\"params\":" + (paramsJson ?? "[]") + "}";
            var res = await GullyHttp.PostJsonAsync(Url, body);
            if (!res.IsSuccess) throw new GullyHttpException($"rpc {method}: http {res.Status}", res.Status, res.Body);
            return res.Body;
        }

        private async Task<string> StringResultAsync(string method, string paramsJson)
        {
            var body = await CallRawAsync(method, paramsJson);
            var r = JsonUtility.FromJson<StringResult>(body);
            if (r.error != null && !string.IsNullOrEmpty(r.error.message)) throw new GullyRpcException(r.error.code, r.error.message);
            return r.result;
        }

        public async Task<long> ChainIdAsync() => (long)Hex.ToBigInteger(await StringResultAsync("eth_chainId", "[]"));

        public async Task<long> BlockNumberAsync() => (long)Hex.ToBigInteger(await StringResultAsync("eth_blockNumber", "[]"));

        public async Task<BigInteger> GasPriceAsync() => Hex.ToBigInteger(await StringResultAsync("eth_gasPrice", "[]"));

        public async Task<BigInteger> GetBalanceAsync(string address, string block = "latest")
            => Hex.ToBigInteger(await StringResultAsync("eth_getBalance", "[" + GullyHttp.JsonString(address) + "," + GullyHttp.JsonString(block) + "]"));

        public async Task<long> GetTransactionCountAsync(string address, string block = "latest")
            => (long)Hex.ToBigInteger(await StringResultAsync("eth_getTransactionCount", "[" + GullyHttp.JsonString(address) + "," + GullyHttp.JsonString(block) + "]"));

        /// <summary>eth_call; returns the raw 0x return data. Reverts surface as <see cref="GullyRpcException"/>.</summary>
        public Task<string> EthCallAsync(string to, string data, string block = "latest")
            => StringResultAsync("eth_call", "[{\"to\":" + GullyHttp.JsonString(to) + ",\"data\":" + GullyHttp.JsonString(data) + "}," + GullyHttp.JsonString(block) + "]");

        public async Task<RpcBlock> GetBlockByNumberAsync(long number)
        {
            var body = await CallRawAsync("eth_getBlockByNumber", "[" + GullyHttp.JsonString(Hex.FromBigInteger(number)) + ",false]");
            var r = JsonUtility.FromJson<BlockResult>(body);
            if (r.error != null && !string.IsNullOrEmpty(r.error.message)) throw new GullyRpcException(r.error.code, r.error.message);
            return r.result != null && !string.IsNullOrEmpty(r.result.number) ? r.result : null;
        }

        public async Task<RpcBlock> GetLatestBlockAsync()
        {
            var body = await CallRawAsync("eth_getBlockByNumber", "[\"latest\",false]");
            var r = JsonUtility.FromJson<BlockResult>(body);
            if (r.error != null && !string.IsNullOrEmpty(r.error.message)) throw new GullyRpcException(r.error.code, r.error.message);
            return r.result;
        }

        /// <summary>Broadcast a signed raw transaction (produced by your wallet layer). Returns the tx hash.</summary>
        public Task<string> SendRawTransactionAsync(string signedTxHex)
            => StringResultAsync("eth_sendRawTransaction", "[" + GullyHttp.JsonString(signedTxHex) + "]");
    }
}
