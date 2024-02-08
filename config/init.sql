CREATE OR REPLACE PROCEDURE criar_transacao_e_atualizar_saldo(
    id_cliente INTEGER,
    valor INTEGER,
    tipo VARCHAR(1),
    descricao VARCHAR(10),
    realizada_em TIMESTAMP WITH TIME ZONE,
    INOUT saldo_atual INTEGER DEFAULT NULL,
    INOUT limite_atual INTEGER DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
DECLARE
    valor_absoluto INTEGER;
BEGIN
    valor_absoluto := valor;

    IF tipo = 'd' THEN
        valor := -valor;
    END IF;

    UPDATE clientes
    SET saldo = saldo + valor
    WHERE id = id_cliente AND (saldo + valor) >= -limite
    RETURNING saldo, limite INTO saldo_atual, limite_atual;

    INSERT INTO transacoes (valor, id_cliente, tipo, descricao, realizada_em)
    VALUES (valor_absoluto, id_cliente, tipo, descricao, realizada_em);
END;
$$;