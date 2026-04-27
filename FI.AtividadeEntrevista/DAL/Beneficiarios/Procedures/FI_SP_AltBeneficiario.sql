CREATE PROC FI_SP_AltBeneficiario
    @NOME      VARCHAR(50),
    @CPF       VARCHAR(11),
    @IDCLIENTE INT,
    @Id        INT
AS
BEGIN
    UPDATE BENEFICIARIOS
    SET 
        NOME = @NOME, 
        CPF = @CPF, 
        IDCLIENTE = @IDCLIENTE
    WHERE Id = @Id;
END