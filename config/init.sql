CREATE TABLE
    "clientes" (
        "id" SERIAL NOT NULL,
        "saldo" INTEGER NOT NULL,
        "limite" INTEGER NOT NULL,
        CONSTRAINT "clientes_pkey" PRIMARY KEY ("id")
    );

CREATE TABLE
    "transacoes" (
        "id" SERIAL NOT NULL,
        "valor" INTEGER NOT NULL,
        "id_cliente" INTEGER NOT NULL,
        "tipo" VARCHAR(1) NOT NULL,
        "descricao" VARCHAR(10) NOT NULL,
        "realizada_em" TIMESTAMP WITH TIME ZONE NOT NULL,
        CONSTRAINT "transacoes_pkey" PRIMARY KEY ("id")
    );

ALTER TABLE "transacoes" ADD CONSTRAINT "transacoes_id_cliente_fkey" FOREIGN KEY ("id_cliente") REFERENCES "clientes" ("id") ON DELETE RESTRICT ON UPDATE CASCADE;

INSERT INTO
    clientes (saldo, limite)
VALUES
    (0, 100000),
    (0, 80000),
    (0, 1000000),
    (0, 10000000),
    (0, 500000);

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