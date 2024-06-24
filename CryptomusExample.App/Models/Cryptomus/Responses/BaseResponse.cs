namespace CryptomusExample.App.Models.Cryptomus.Responses;

public class BaseResponse<T>
{
    public int State { get; set; }
    public T Result { get; set; }
}