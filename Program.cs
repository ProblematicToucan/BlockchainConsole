using Microsoft.Extensions.Configuration;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BlockchainConsole;
class Program
{
    private static readonly IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
    private static readonly Account _account = new(config["PrivateKey"]);
    private static readonly Web3 _web3 = new(_account, "wss://ethereum-sepolia-rpc.publicnode.com"); // Sepolia testnet
    private const string Contract = "0x4446BcEb26a36FCe0dEDf6E54928f994A235A1af"; // Contract address
    private const string CONTRACT_ABI = @"[{
                'inputs': [],
                'name': 'getMood',
                'outputs': [{
                    'internalType': 'string',
                    'name': '',
                    'type': 'string'
                }],
                'stateMutability': 'view',
                'type': 'function'
            },
            {
                'inputs': [{
                    'internalType': 'string',
                    'name': '_mood',
                    'type': 'string'
                }],
                'name': 'setMood',
                'outputs': [],
                'stateMutability': 'nonpayable',
                'type': 'function'
            }
        ]";
    static async Task Main(string[] args)
    {
        Console.WriteLine("An Adress: " + _account.Address);
        try
        {
            var balance = await GetAccountBalanceAsync();
            Console.WriteLine($"ETH Balance: {balance} ETH");
            // await SetMoodAsync();
            var mood = await GetMoodAsync();
            Console.WriteLine("Mood: " + mood);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An exception occurred: " + ex.Message);
        }
    }

    static async Task<decimal> GetAccountBalanceAsync()
    {
        var balance = await _web3.Eth.GetBalance.SendRequestAsync(_account.Address);
        var etherAmount = Web3.Convert.FromWei(balance.Value);
        return etherAmount;
    }

    static async Task<string> GetMoodAsync()
    {
        var contract = _web3.Eth.GetContract(CONTRACT_ABI, Contract);
        var function = contract.GetFunction("getMood");
        var result = await function.CallAsync<string>();
        return result;
    }

    static async Task SetMoodAsync()
    {
        // Get the contract and function
        var contract = _web3.Eth.GetContract(CONTRACT_ABI, Contract);
        var function = contract.GetFunction("setMood");

        // Get the current gas price
        var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();
        Console.WriteLine($"Gas Price: {Web3.Convert.FromWei(gasPrice)} ETH");

        // Get a more accurate gas estimate with a buffer
        var gasLimit = await function.EstimateGasAsync(_account.Address, gasPrice, null, "Hee hee...");
        Console.WriteLine($"Gas Limit: {gasLimit.Value} units");

        // Send the transaction and wait for the receipt
        var transactionReceipt = await function.SendTransactionAndWaitForReceiptAsync(_account.Address, gasLimit, null, null, "Hee hee...");
        Console.WriteLine($"Transaction status : {transactionReceipt.Status.Value}");
        Console.WriteLine($"Transaction hash: {transactionReceipt.TransactionHash}");
    }

    static async Task SendEthAsync(decimal amountInEther)
    {
        _web3.TransactionManager = new AccountSignerTransactionManager(_web3.Client, _account);
        var recipientAddress = "0x47E0bBBD519F3117560E2b0Ff72e248BF8915008";
        var transaction = await _web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(recipientAddress, amountInEther);
        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TransactionHash);
        Console.WriteLine($"Transaction status: {receipt.Status.Value == 1}");
    }
}
