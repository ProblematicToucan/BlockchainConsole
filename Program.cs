using Microsoft.Extensions.Configuration;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;

namespace BlockchainConsole;
class Program
{
    private static IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
    private static Account _account = new Account(config["PrivateKey"]);
    private static Web3 _web3 = new Web3("https://rpc.sepolia.org/"); // Sepolia testnet
    private const string Contract = "0x4446BcEb26a36FCe0dEDf6E54928f994A235A1af"; // Contract address
    static async Task Main(string[] args)
    {
        Console.WriteLine("An Adress: " + _account.Address);
        try
        {
            var balance = await GetAccountBalanceAsync();
            var mood = await GetMoodAsync();
            Console.WriteLine("ETH Balance: " + balance);
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
        var contractAbi = @"[{
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
        var contract = _web3.Eth.GetContract(contractAbi, Contract);
        var function = contract.GetFunction("getMood");
        var result = await function.CallAsync<string>();
        return result;
    }

    static async Task SendEthAsync(decimal amountInEther)
    {
        var recipientAddress = "0x47E0bBBD519F3117560E2b0Ff72e248BF8915008";
        _web3.TransactionManager = new AccountSignerTransactionManager(_web3.Client, _account);
        var transaction = await _web3.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(recipientAddress, amountInEther);
        var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction.TransactionHash);
        Console.WriteLine($"Transaction status: {receipt.Status.Value == 1}");
    }
}
