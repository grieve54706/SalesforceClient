using sf_demo.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using sf_demo.Salesforce;
using System.Text.Json;
using System;
using System.Linq;

public class App
{
    private SalesforceClient salesforceClient;

    public App(SalesforceClient salesforceClient)
    {
        this.salesforceClient = salesforceClient;
    }

    internal async Task Run()
    {
        salesforceClient.connect();

        var accountName = "TestCreateAccount";

        Console.WriteLine("");
        Console.WriteLine($"Create {accountName}");

        var account = CreateAccount(accountName);

        var insertResults = await salesforceClient.InsertAsync<Account>(new List<Account> { account });

        var result = insertResults[0];

        var accountId = result.Success ? result.Id : null;

        if (accountId == null) {
            throw new Exception("Insert fail");
        }

        List<Account> accounts = await GetAccounts();

        Console.WriteLine("");
        Console.WriteLine("Query all account}");

        Print(accounts);

        account = accounts.Where(a => accountId.Equals(a.Id) && accountName.Equals(a.Name))
            .Select(a => a).First();

        account.SLASerialNumber__c = new Random(9999).Next().ToString();

        await salesforceClient.UpdateAsync<Account>(new List<Account> { account });

        Console.WriteLine("");
        Console.WriteLine("Update.");

        accounts = await GetAccountById(accountId);

        account = accounts[0];

        Print(accounts);

        Console.WriteLine("");
        Console.WriteLine("Delete");

        await salesforceClient.DeleteAsync<Account>(new List<Account> { account });

        accounts = await GetAccounts();

        Print(accounts);
    }

    private Account CreateAccount(string accountName)
    {
        var account = new Account();
        account.Name = accountName;
        return account;
    }

    private async Task<List<Account>> GetAccounts()
    {
        string queryMessage = "SELECT Id, Name, SLASerialNumber__c FROM Account";

        return await salesforceClient.QueryAsync<Account>(queryMessage);
    }

    private async Task<List<Account>> GetAccountById(string id)
    {
        string queryMessage = $"SELECT Id, Name, SLASerialNumber__c FROM Account WHERE Id = '{id}'";

        return await salesforceClient.QueryAsync<Account>(queryMessage);
    }

    private void Print<T>(T obj)
    {
        Console.WriteLine("");
        Console.WriteLine(JsonSerializer.Serialize(obj));
        Console.WriteLine("");
    }

    private void Print<T>(List<T> objs)
    {
        Console.WriteLine("");
        objs.Select(o => JsonSerializer.Serialize(o)).ToList().ForEach(Console.WriteLine);
        Console.WriteLine("");
    }

}