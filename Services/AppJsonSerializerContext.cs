using System.Text.Json.Serialization;

[JsonSerializable(typeof(CriarTransacaoRequest))]
[JsonSerializable(typeof(CriarTransacaoResponse))]
[JsonSerializable(typeof(Extrato))]
[JsonSerializable(typeof(Transacao))]
[JsonSerializable(typeof(Saldo))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }