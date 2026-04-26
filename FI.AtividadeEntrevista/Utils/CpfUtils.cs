using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FI.AtividadeEntrevista.Utils
{
    public static class CpfUtils
    {
        public static bool isCpf(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "").Trim();
            //Verifica se o CPF tem 11 dígitos
            if (cpf.Length != 11)
                return false;

            //Verifica se todos os dígitos são iguais, o que não é um CPF válido
            if (new string(cpf[0], 11) == cpf)
                return false;

            //Calcula o primeiro dígito verificador
            int soma = 0; 
            for(int i = 0; i < 9; i++)
                soma += int.Parse(cpf[i].ToString()) * (10 - i);

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            // Calcula o segundo dígito verificador
            soma = 0; 
            for(int i = 0; i < 10; i++)
                soma += int.Parse(cpf[i].ToString()) * (11 - i);

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            //Comparador dos dígitos verificadores calculados com os dígitos finais do CPF
            string digitosFinais = cpf.Substring(9, 2);
            string digitosCalculados = digito1.ToString() + digito2.ToString();

            return digitosFinais == digitosCalculados;
        }
    }
}
