var beneficiarios = [];
var idxEditando = null;

function formatarCPF(cpf) {
    cpf = cpf.replace(/\D/g, '');
    return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
}

function atualizarGridBeneficiarios() {
    var tbody = $('#gridBeneficiarios tbody');
    tbody.empty();
    $.each(beneficiarios, function (idx, b) {
        var row = '<tr>' +
            '<td>' + formatarCPF(b.cpf) + '</td>' +
            '<td>' + b.nome + '</td>' +
            '<td>' +
            '<button type="button" class="btn btn-primary btn-sm mr-6" style="margin-right:10px;" onclick="alterarBeneficiario(' + idx + ')">Alterar</button>' +
            '<button type="button" class="btn btn-primary btn-sm" onclick="removerBeneficiario(' + idx + ')">Excluir</button>' +
            '</td>' +
            '</tr>';
        tbody.append(row);
    });
}

function removerBeneficiario(idx) {
    beneficiarios.splice(idx, 1);
    atualizarGridBeneficiarios();
    limparFormulario();
}

function alterarBeneficiario(idx) {
    var b = beneficiarios[idx];
    $('#BeneficiarioCPF').val(b.cpf);
    $('#BeneficiarioNome').val(b.nome);
    idxEditando = idx;
    $('#btnAdicionarBeneficiario').hide();
    $('#btnSalvarBeneficiario').show();
    $('#msgBeneficiario').text('');
}

function limparFormulario() {
    $('#BeneficiarioCPF').val('');
    $('#BeneficiarioNome').val('');
    idxEditando = null;
    $('#btnAdicionarBeneficiario').show();
    $('#btnSalvarBeneficiario').hide();
    $('#msgBeneficiario').text('');
}

function isCpfValido(cpf) {
    cpf = cpf.replace(/[^\d]+/g, '');
    if (cpf.length !== 11) return false;
    if (/^(\d)\1+$/.test(cpf)) return false;

    var soma = 0;
    for (var i = 0; i < 9; i++)
        soma += parseInt(cpf.charAt(i)) * (10 - i);
    var resto = soma % 11;
    var digito1 = resto < 2 ? 0 : 11 - resto;

    soma = 0;
    for (var i = 0; i < 10; i++)
        soma += parseInt(cpf.charAt(i)) * (11 - i);
    resto = soma % 11;
    var digito2 = resto < 2 ? 0 : 11 - resto;

    return cpf.substr(9, 2) === ('' + digito1 + digito2);
}

function cpfDuplicado(cpf, idxIgnorar) {
    cpf = cpf.replace(/[^\d]+/g, '');
    for (var i = 0; i < beneficiarios.length; i++) {
        if (i !== idxIgnorar && beneficiarios[i].cpf.replace(/[^\d]+/g, '') === cpf) {
            return true;
        }
    }
    return false;
}

$(document).ready(function () {

    $('#BeneficiarioCPF').on('input', function () {
        let valor = $(this).val();
        $(this).val(formatarCPF(valor));
    });
    $('#BeneficiarioCPF').on('blur', function () {
        $(this).val(formatarCPF($(this).val()));
    });
    $('#btnAdicionarBeneficiario').off('click').click(function () {
        var cpf = $('#BeneficiarioCPF').val().trim();
        var nome = $('#BeneficiarioNome').val().trim();
        $('#msgBeneficiario').text('');

        if (!isCpfValido(cpf)) {
            $('#msgBeneficiario').text('CPF do beneficiario invalido!');
            return;
        }
        if (cpfDuplicado(cpf, null)) {
            $('#msgBeneficiario').text('Ja existe um beneficiário com este CPF.');
            return;
        }
        beneficiarios.push({ cpf: cpf, nome: nome });
        atualizarGridBeneficiarios();
        limparFormulario();
    });

    if ($('#btnSalvarBeneficiario').length === 0) {
        $('#btnAdicionarBeneficiario').after('<button type="button" id="btnSalvarBeneficiario" class="btn btn-success" style="display:none;margin-left:5px;">Salvar</button>');
    }

    $('#btnSalvarBeneficiario').click(function () {
        var cpf = $('#BeneficiarioCPF').val().trim();
        var nome = $('#BeneficiarioNome').val().trim();
        $('#msgBeneficiario').text('');

        if (!isCpfValido(cpf)) {
            $('#msgBeneficiario').text('CPF do beneficiario invalido!');
            return;
        }
        if (cpfDuplicado(cpf, idxEditando)) {
            $('#msgBeneficiario').text('Ja existe um beneficiario com este CPF.');
            return;
        }
        if (idxEditando !== null) {
            beneficiarios[idxEditando] = { cpf: cpf, nome: nome };
            atualizarGridBeneficiarios();
            limparFormulario();
        }
    });
});