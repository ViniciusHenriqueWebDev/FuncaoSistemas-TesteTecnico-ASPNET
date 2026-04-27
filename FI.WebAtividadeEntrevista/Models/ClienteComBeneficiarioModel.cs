using System.Collections.Generic;
using WebAtividadeEntrevista.Models;

public class ClienteComBeneficiariosModel
{
    public ClienteModel Cliente { get; set; }
    public List<BeneficiarioModel> Beneficiarios { get; set; }
}