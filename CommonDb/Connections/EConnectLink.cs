using System.ComponentModel.DataAnnotations;

namespace CommonDb.Connections;

public enum EConnectLink
{
    [Display(Name = "SelfValidateConnection")]
    SelfValidateConnection, // pass all
}