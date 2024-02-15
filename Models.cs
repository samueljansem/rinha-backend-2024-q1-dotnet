using System;
using System.Collections.Generic;

public record struct Cliente(int Id, int Limite, int Saldo);

public record struct CriarTransacaoRequest(int Valor, char Tipo, string Descricao);

public record struct CriarTransacaoResponse(int Limite, int Saldo);

public record struct Extrato(Saldo Saldo, IEnumerable<Transacao> UltimasTransacoes);

public record struct Saldo(int Total, int Limite, DateTime DataExtrato);

public record struct Transacao(int Valor, string Descricao, char Tipo, DateTime RealizadaEm);