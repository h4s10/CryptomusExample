using CryptomusExample.App.Models.Cryptomus.Requests;
using CryptomusExample.App.Models.Cryptomus.Responses;
using Refit;

namespace CryptomusExample.App.ThirdPartyApis;

public interface ICryptomusClient
{
    [Post("/wallet")]
    Task<BaseResponse<CreateStaticWalletResponse>> CreateStaticWalletAsync([Body(BodySerializationMethod.Serialized)] CreateStaticWalletRequest request,
        [Header("sign")] string sign, [Header("merchant")] string merchantId);
}