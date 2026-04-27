$(document).ready(function () {
    if (obj) {
        $('#formCadastro #Nome').val(obj.Nome);
        $('#formCadastro #CEP').val(obj.CEP);
        $('#formCadastro #Email').val(obj.Email);
        $('#formCadastro #Sobrenome').val(obj.Sobrenome);
        $('#formCadastro #Nacionalidade').val(obj.Nacionalidade);
        $('#formCadastro #Estado').val(obj.Estado);
        $('#formCadastro #Cidade').val(obj.Cidade);
        $('#formCadastro #Logradouro').val(obj.Logradouro);
        $('#formCadastro #Telefone').val(obj.Telefone);
        $('#formCadastro #CPF').val(formatarCPF(obj.CPF));
    }

    $('#formCadastro #CPF').on('input', function () {
        let valor = $(this).val();
        $(this).val(formatarCPF(valor));
    });

    $('#formCadastro #CPF').on('blur', function () {
        $(this).val(formatarCPF($(this).val()));
    });

    $('#BeneficiarioCPF').on('input', function () {
        let valor = $(this).val();
        $(this).val(formatarCPF(valor));
    });

    $('#BeneficiarioCPF').on('blur', function () {
        $(this).val(formatarCPF($(this).val()));
    });

    $('#formCadastro #CPF').val(formatarCPF($('#formCadastro #CPF').val()));
    $('#formCadastro').submit(function (e) {
        e.preventDefault();
        $.ajax({
            url: urlPost,
            method: "POST",
            data: {
                "Id": $(this).find("#Id").val(),
                "Nome": $(this).find("#Nome").val(),
                "CEP": $(this).find("#CEP").val(),
                "Email": $(this).find("#Email").val(),
                "Sobrenome": $(this).find("#Sobrenome").val(),
                "Nacionalidade": $(this).find("#Nacionalidade").val(),
                "Estado": $(this).find("#Estado").val(),
                "Cidade": $(this).find("#Cidade").val(),
                "Logradouro": $(this).find("#Logradouro").val(),
                "Telefone": $(this).find("#Telefone").val(),
                "CPF": $(this).find("#CPF").val().replace(/\D/g, ""),
                "BeneficiariosJson": JSON.stringify(beneficiarios)
            },
            success: function (r) {
                ModalDialog("Sucesso!", r);
                $('#' + $('.modal').last().attr('id')).on('hidden.bs.modal', function () {
                    window.location.href = '/Cliente';
                });
            },
            error: function (r) {
                if (r.status == 400)
                    ModalDialog("Ocorreu um erro", r.responseJSON);
                else if (r.status == 500)
                    ModalDialog("Ocorreu um erro", "Ocorreu um erro interno no servidor.");
            }
        });
    });

    $('#btnBeneficiarios').click(function () {
        $('#modalBeneficiarios').modal('show');
    });

    // Adiciona botão Salvar se não existir
    if ($('#btnSalvarBeneficiario').length === 0) {
        $('#btnAdicionarBeneficiario').after('<button type="button" id="btnSalvarBeneficiario" class="btn btn-success" style="display:none;margin-left:5px;">Salvar</button>');
    }

    $('#btnAdicionarBeneficiario').click(function () {
        var cpf = $('#BeneficiarioCPF').val().trim();
        var nome = $('#BeneficiarioNome').val().trim();
        $('#msgBeneficiario').text('');

        if (!isCpfValido(cpf)) {
            $('#msgBeneficiario').text('CPF do beneficiario invalido!');
            return;
        }
        if (cpfDuplicado(cpf, null)) {
            $('#msgBeneficiario').text('Já existe um beneficiario com este CPF.');
            return;
        }
        beneficiarios.push({ cpf: cpf, nome: nome });
        atualizarGridBeneficiarios();
        limparFormularioBeneficiario();
    });

    $('#btnSalvarBeneficiario').off('click').click(function () {
        var cpf = $('#BeneficiarioCPF').val().trim();
        var nome = $('#BeneficiarioNome').val().trim();
        
        if (!isCpfValido(cpf)) {
            $('#msgBeneficiario').text('CPF do beneficiario incorreto!');
            return;
        }
        if (cpfDuplicado(cpf, idxEditando)) {
            $('#msgBeneficiario').text('Ja existe um beneficiario com este CPF.');
            return;
        }
        if (idxEditando !== null) {
            beneficiarios[idxEditando] = { cpf: cpf, nome: nome };
            atualizarGridBeneficiarios();
            limparFormularioBeneficiario();
            $('#BeneficiarioCPF').blur();
            $('#BeneficiarioNome').blur();
        }
    });
});

var beneficiarios = [];
var idxEditando = null;

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

function atualizarGridBeneficiarios() {
    var tbody = $('#gridBeneficiarios tbody');
    tbody.empty();
    $.each(beneficiarios, function (idx, b) {
        var row = '<tr>' +
            '<td>' + formatarCPF(b.cpf) + '</td>' +
            '<td>' + b.nome + '</td>' +
            '<td>' +
            '<button type="button" class="btn btn-primary btn-sm mr-6" onclick="alterarBeneficiario(' + idx + ')">Alterar</button>' +
            '<button type="button" class="btn btn-primary btn-sm" onclick="removerBeneficiario(' + idx + ')">Excluir</button>' +
            '</td>' +
            '</tr>';
        tbody.append(row);
    });
}

$('#modalBeneficiarios').on('show.bs.modal', function () {
    $('#msgBeneficiario').text('');
    if (obj && obj.Id && beneficiarios.length === 0) {
        $.get('/Cliente/ListarBeneficiarios', { idCliente: obj.Id }, function (data) {
            beneficiarios = data.map(function (b) {
                return { id: b.Id, cpf: b.CPF, nome: b.Nome };
            });
            atualizarGridBeneficiarios();
        });
    } else {
        atualizarGridBeneficiarios();
    }
    limparFormularioBeneficiario();
});

function removerBeneficiario(idx) {
    if (confirm('Deseja realmente excluir este beneficiário?')) {
        beneficiarios.splice(idx, 1);
        atualizarGridBeneficiarios();
        limparFormularioBeneficiario();
    }
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

function limparFormularioBeneficiario() {
    $('#BeneficiarioCPF').val('');
    $('#BeneficiarioNome').val('');
    idxEditando = null;
    $('#btnAdicionarBeneficiario').show();
    $('#btnSalvarBeneficiario').hide();
    $('#msgBeneficiario').text('');
}

function formatarCPF(cpf) {
    cpf = cpf.replace(/\D/g, "").slice(0, 11);
    cpf = cpf.replace(/(\d{3})(\d)/, "$1.$2");
    cpf = cpf.replace(/(\d{3})(\d)/, "$1.$2");
    cpf = cpf.replace(/(\d{3})(\d{1,2})$/, "$1-$2");
    return cpf;
};

function ModalDialog(titulo, texto) {
    var random = Math.random().toString().replace('.', '');
    var texto = '<div id="' + random + '" class="modal fade">                                                               ' +
        '        <div class="modal-dialog">                                                                                 ' +
        '            <div class="modal-content">                                                                            ' +
        '                <div class="modal-header">                                                                         ' +
        '                    <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>         ' +
        '                    <h4 class="modal-title">' + titulo + '</h4>                                                    ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-body">                                                                           ' +
        '                    <p>' + texto + '</p>                                                                           ' +
        '                </div>                                                                                             ' +
        '                <div class="modal-footer">                                                                         ' +
        '                    <button type="button" class="btn btn-default" data-dismiss="modal">Fechar</button>             ' +
        '                                                                                                                   ' +
        '                </div>                                                                                             ' +
        '            </div><!-- /.modal-content -->                                                                         ' +
        '  </div><!-- /.modal-dialog -->                                                                                    ' +
        '</div> <!-- /.modal -->                                                                                        ';

    $('body').append(texto);
    $('#' + random).modal('show');
};