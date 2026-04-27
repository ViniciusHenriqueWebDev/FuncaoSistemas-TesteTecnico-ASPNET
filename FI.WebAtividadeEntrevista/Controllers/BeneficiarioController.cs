using FI.AtividadeEntrevista.BLL;
using FI.AtividadeEntrevista.DML;
using FI.AtividadeEntrevista.Utils;
using WebAtividadeEntrevista.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WebAtividadeEntrevista.Controllers
{
    public class BeneficiarioController : Controller
    {
        [HttpGet]
        public ActionResult Consultar(long id)
        {
            try
            {
                BoBeneficiario bo = new BoBeneficiario();
                Beneficiario ben = bo.Consultar(id);

                if (ben == null)
                    return Json(new { Result = "ERROR", Message = "Beneficiário não encontrado" }, JsonRequestBehavior.AllowGet);

                var model = ToModel(ben);
                return Json(new { Result = "OK", Record = model }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult ListarPorCliente(long idCliente)
        {
            try
            {
                BoBeneficiario bo = new BoBeneficiario();
                List<Beneficiario> list = bo.ListarPorCliente(idCliente);
                var models = list.Select(ToModel).ToList();
                return Json(new { Result = "OK", Records = models }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Incluir(BeneficiarioModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    List<string> erros = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();

                    Response.StatusCode = 400;
                    return Json(string.Join(Environment.NewLine, erros));
                }

                if (!CpfUtils.isCpf(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json("CPF inválido");
                }

                BoBeneficiario bo = new BoBeneficiario();

                if (bo.VerificarExistencia(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json("CPF já cadastrado");
                }

                var dml = ToDml(model);
                long newId = bo.Incluir(dml);
                model.Id = newId;

                return Json(new { Result = "OK", Record = model });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult IncluirMultiplos(long idCliente, string beneficiarios)
        {
            try
            {
                if (string.IsNullOrEmpty(beneficiarios))
                    return Json(new { Result = "ERROR", Message = "Nenhum beneficiário informado" });

                var lista = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BeneficiarioModel>>(beneficiarios);
                BoBeneficiario bo = new BoBeneficiario();

                foreach (var b in lista)
                {
                    b.IdCliente = idCliente;
                    if (!CpfUtils.isCpf(b.CPF))
                        continue;

                    if (!bo.VerificarExistencia(b.CPF))
                        bo.Incluir(ToDml(b));
                }

                return Json(new { Result = "OK", Message = "Beneficiários salvos com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Alterar(BeneficiarioModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    List<string> erros = (from item in ModelState.Values
                                          from error in item.Errors
                                          select error.ErrorMessage).ToList();

                    Response.StatusCode = 400;
                    return Json(string.Join(Environment.NewLine, erros));
                }

                if (!CpfUtils.isCpf(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json("CPF inválido");
                }

                BoBeneficiario bo = new BoBeneficiario();

                if (bo.VerificarExistencia(model.CPF))
                {
                    Response.StatusCode = 400;
                    return Json("CPF já cadastrado");
                }

                var dml = ToDml(model);
                bo.Alterar(dml);

                return Json(new { Result = "OK", Message = "Beneficiário alterado com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        // POST: /Beneficiario/Excluir
        [HttpPost]
        public JsonResult Excluir(long id)
        {
            try
            {
                BoBeneficiario bo = new BoBeneficiario();
                bo.Excluir(id);
                return Json(new { Result = "OK", Message = "Beneficiário excluído com sucesso" });
            }
            catch (Exception ex)
            {
                return Json(new { Result = "ERROR", Message = ex.Message });
            }
        }

        #region Helpers - mapeamento entre Model (Web) e DML (BLL/DAL)

        private BeneficiarioModel ToModel(Beneficiario d)
        {
            if (d == null) return null;
            return new BeneficiarioModel
            {
                Id = d.Id,
                Nome = d.Nome,
                CPF = d.CPF,
                IdCliente = d.IdCliente
            };
        }

        private Beneficiario ToDml(BeneficiarioModel m)
        {
            if (m == null) return null;
            return new Beneficiario
            {
                Id = m.Id,
                Nome = m.Nome,
                CPF = m.CPF,
                IdCliente = m.IdCliente
            };
        }

        #endregion
    }
}