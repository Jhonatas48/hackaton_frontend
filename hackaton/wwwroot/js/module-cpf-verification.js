// Essa loucura aqui verifica se o bendito do campo que envolve a maluquice da verifica��o de CPF tem filho, pq isso indica que teve um erro. A� ele troca o verdinho de acordo.
// Note que essa bagun�a envolve QUATRO classes: .cpf, .ver-cpf, .checkmark e .checked .
$(document).ready(function () {
    $('.cpf').on('blur', function () {
        if ($('.ver-cpf').children().length === 0) {
            // Alternar a classe "checked" nos campos de imagem com a classe "checkmark"
            console.log("entrou :)");
            $('.checkmark').addClass('checked');
        }
        else {
            console.log("n entrou :(");
            $('.checkmark').removeClass('checked');
        }
    });
});