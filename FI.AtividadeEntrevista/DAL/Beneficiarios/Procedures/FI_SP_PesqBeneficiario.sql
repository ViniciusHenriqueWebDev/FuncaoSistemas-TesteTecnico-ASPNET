CREATE PROC FI_SP_PesqBeneficiario
    @iniciarEm INT,
    @quantidade INT,
    @campoOrdenacao VARCHAR(200),
    @crescente BIT
AS
BEGIN
    DECLARE @SCRIPT NVARCHAR(MAX);
    DECLARE @ORDER VARCHAR(50);

    IF(@campoOrdenacao = 'CPF')
        SET @ORDER = '[CPF]';
    ELSE IF(@campoOrdenacao = 'IDCLIENTE')
        SET @ORDER = '[IDCLIENTE]';
    ELSE
        SET @ORDER = '[NOME]';

    IF(@crescente = 0)
        SET @ORDER = @ORDER + ' DESC';
    ELSE
        SET @ORDER = @ORDER + ' ASC';

    SET @SCRIPT = N'
        SELECT ID, NOME, CPF, IDCLIENTE
        FROM (
            SELECT 
                ROW_NUMBER() OVER (ORDER BY ' + @ORDER + N') AS Row,
                ID, NOME, CPF, IDCLIENTE
            FROM BENEFICIARIOS WITH(NOLOCK)
        ) AS BeneficiariosWithRowNumbers
        WHERE Row > @iniciarEm AND Row <= (@iniciarEm + @quantidade)
        ORDER BY ' + @ORDER;

    EXECUTE SP_EXECUTESQL @SCRIPT,
        N'@iniciarEm INT, @quantidade INT',
        @iniciarEm = @iniciarEm,
        @quantidade = @quantidade;

    SELECT COUNT(1) FROM BENEFICIARIOS WITH(NOLOCK);
END