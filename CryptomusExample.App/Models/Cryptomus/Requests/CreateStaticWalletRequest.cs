namespace CryptomusExample.App.Models.Cryptomus.Requests;

public class CreateStaticWalletRequest
{
    public string Currency { get; set; }
    public string Network { get; set; }
    public string OrderId { get; set; }
}