using FI.AtividadeEntrevista.BLL;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FI.AtividadeEntrevista.DML;
using FI.AtividadeEntrevista.Utils;
using Newtonsoft.Json; 

namespace WebAtividadeEntrevista.Controllers
{
    public class ClienteController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Incluir()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Incluir(ClienteModel model, string BeneficiariosJson)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario(); 
            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                if (!CpfUtils.isCpf(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json("CPF inválido");
                }

                if (bo.VerificarExistencia(model.CPF, model.Id))
                {
                    Response.StatusCode = 400; 
                    return Json("CPF já cadastrado");
                }
                
                model.Id = bo.Incluir(new Cliente()
                {                    
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    CPF = model.CPF
                });

                if (!string.IsNullOrWhiteSpace(BeneficiariosJson))
                {
                    var beneficiariosModel = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(BeneficiariosJson);
                    if(beneficiariosModel != null)
                    {

                        var cpfs = beneficiariosModel
                            .Where(b => !string.IsNullOrWhiteSpace(b.CPF))
                            .Select(b => b.CPF.Replace(".", "").Replace("-", "").Trim())
                            .ToList();

                        var cpfsDuplicados = cpfs
                            .GroupBy(c => c)
                            .Where(g => g.Count() > 1)
                            .Select(g => g.Key)
                            .ToList();

                        if (cpfsDuplicados.Any())
                        {
                            Response.StatusCode = 400;
                            return Json("Não é permitido cadastrar beneficiários com o mesmo CPF para o mesmo cliente.");
                        }

                        foreach (var bm in beneficiariosModel)
                        {
                            if(!CpfUtils.isCpf(bm.CPF))
                            {
                                Response.StatusCode = 400;
                                return Json($"CPF do beneficiário {bm.Nome} é inválido");
                            }
                            if (boBeneficiario.VerificarExistencia(bm.CPF))
                            {
                                Response.StatusCode = 400;
                                return Json($"CPF do beneficiário {bm.Nome} já cadastrado");
                            }
                            boBeneficiario.Incluir(new Beneficiario()
                            {
                                Nome = bm.Nome,
                                CPF = bm.CPF != null ? bm.CPF.Replace(".", "").Replace("-", "").Trim() : null,
                                IdCliente = model.Id
                            });
                        }
                    }
                }

                return Json("Cadastro efetuado com sucesso");
            }
        }

        [HttpPost]
        public JsonResult Alterar(ClienteModel model, string BeneficiariosJson)
        {
            BoCliente bo = new BoCliente();
            BoBeneficiario boBeneficiario = new BoBeneficiario();

            if (!this.ModelState.IsValid)
            {
                List<string> erros = (from item in ModelState.Values
                                      from error in item.Errors
                                      select error.ErrorMessage).ToList();

                Response.StatusCode = 400;
                return Json(string.Join(Environment.NewLine, erros));
            }
            else
            {
                if (!CpfUtils.isCpf(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json("CPF inválido");
                }

                if (bo.VerificarExistencia(model.CPF, model.Id))
                {
                    Response.StatusCode = 400;
                    return Json("CPF já cadastrado");
                }
                bo.Alterar(new Cliente()
                {
                    Id = model.Id,
                    CEP = model.CEP,
                    Cidade = model.Cidade,
                    Email = model.Email,
                    Estado = model.Estado,
                    Logradouro = model.Logradouro,
                    Nacionalidade = model.Nacionalidade,
                    Nome = model.Nome,
                    Sobrenome = model.Sobrenome,
                    Telefone = model.Telefone,
                    CPF = model.CPF
                });

                if (!string.IsNullOrWhiteSpace(BeneficiariosJson))
                {
                    var enviados = JsonConvert.DeserializeObject<List<BeneficiarioModel>>(BeneficiariosJson)
                                   ?? new List<BeneficiarioModel>();

                    var cpfs = enviados
                        .Where(b => !string.IsNullOrWhiteSpace(b.CPF))
                        .Select(b => b.CPF.Replace(".", "").Replace("-", "").Trim())
                        .ToList();

                    var cpfsDuplicados = cpfs
                        .GroupBy(c => c)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (cpfsDuplicados.Any())
                    {
                        Response.StatusCode = 400;
                        return Json("Não é permitido cadastrar beneficiários com o mesmo CPF para o mesmo cliente.");
                    }

                    var atuais = boBeneficiario.ListarPorCliente(model.Id) ?? new List<Beneficiario>();

                    var enviadosIds = new HashSet<long>(enviados.Where(x => x.Id > 0).Select(x => x.Id));

                    foreach (var atual in atuais)
                    {
                        if (!enviadosIds.Contains(atual.Id))
                        {
                            boBeneficiario.Excluir(atual.Id);
                        }
                    }

                    foreach (var bm in enviados)
                    {
                        var bene = new Beneficiario
                        {
                            Id = bm.Id,
                            IdCliente = model.Id,
                            CPF = bm.CPF != null ? bm.CPF.Replace(".", "").Replace("-", "").Trim() : null,
                            Nome = bm.Nome
                        };

                        if (bene.Id == 0)
                        {
                            boBeneficiario.Incluir(bene);
                        }
                        else
                        {
                            boBeneficiario.Alterar(bene);
                        }
                    }
                }
                else
                {
                    var atuais = boBeneficiario.ListarPorCliente(model.Id) ?? new List<Beneficiario>();
                    foreach (var atual in atuais)
                    {
                        boBeneficiario.Excluir(atual.Id);
                    }
                }

                return Json("Cadastro alterado com sucesso");
            }
        }

        [HttpGet]
        public ActionResult Alterar(long id)
        {
            BoCliente bo = new BoCliente();
            Cliente cliente = bo.Consultar(id);
            Models.ClienteModel model = null;
                
            if (cliente != null)
            {
                model = new ClienteModel()
                {
                    Id = cliente.Id,
                    CEP = cliente.CEP,
                    Cidade = cliente.Cidade,
                    Email = cliente.Email,
                    Estado = cliente.Estado,
                    Logradouro = cliente.Logradouro,
                    Nacionalidade = cliente.Nacionalidade,
                    Nome = cliente.Nome,
                    Sobrenome = cliente.Sobrenome,
                    Telefone = cliente.Telefone,
                    CPF = cliente.CPF
                };

            
            }

            return View(model);
        }

        [HttpGet]
        public JsonResult ListarBeneficiarios(long idCliente)
        {
            var boBeneficiario = new BoBeneficiario();
            var beneficiarios = boBeneficiario.ListarPorCliente(idCliente)
                .Select(b => new { b.Id, b.Nome, b.CPF })
                .ToList();
            return Json(beneficiarios, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ClienteList(int jtStartIndex = 0, int jtPageSize = 0, string jtSorting = null)
        {
            try
            {
                int qtd = 0;
                string campo = string.Empty;
                string crescente = string.Empty;
                string[] array = jtSorting.Split(' ');

                if (array.Length > 0)
                    campo = array[0];

                if (array.Length > 1)
                    crescente = array[1];

                List<Cliente> clientes = new BoCliente().Pesquisa(jtStartIndex, jtPageSize, campo, crescente.Equals("ASC", StringComparison.InvariantCultureIgnoreCase), out qtd);

                //Return result to jTable
                return Json(new { Result = "OK", Records = clientes, TotalRecordCount = qtd });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }
    }
}