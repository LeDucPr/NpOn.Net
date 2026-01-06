using Common.Extensions.NpOn.CommonGrpcContract;
using MicroServices.Account.Contracts.NpOn.AccountServiceContract.Events;
using ProtoBuf;

namespace MicroServices.Account.Contracts.NpOn.AccountServiceContract;

[ProtoContract]
[ProtoInclude(100, typeof(AccountSaveLoginEvent))]
public abstract class BaseAccountCommonEvent : CommonMessageContent
{
}