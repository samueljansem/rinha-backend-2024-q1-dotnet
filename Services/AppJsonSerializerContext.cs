using System.Text.Json.Serialization;

[JsonSerializable(typeof(CriarTransacaoRequest))]
[JsonSerializable(typeof(CriarTransacaoResponse))]
[JsonSerializable(typeof(ExtratoDTO))]
[JsonSerializable(typeof(TransacaoDTO))]
[JsonSerializable(typeof(SaldoDTO))]
internal partial class AppJsonSerializerContext : JsonSerializerContext {}