// Qdo vc clica no �cone do olho no campo de password, o olho alterna o �cone, e revela ou exibe o texto do campo
// !!! Reformatar: css background ao inv�s de trocar o src. Fazer igual ao verdinho do CPF. !!!
$(document).ready(function () {
    $('.eye').on('click', function () {
        $(this).toggleClass('eye-show');

        $(this).siblings('.sub').children('.textbox').attr('type',
            $(this).hasClass('eye-show') ? '' : 'password'
        );
    })
    /*$('.eye').on('click', function () {
        console.log('aa');
        if ($(this).hasClass('hide')) {
            console.log("rem");
            $(this).removeClass('hide');
            $(this).addClass('show');
            $(this).siblings('.sub').children('.textbox').attr('type', '');
            $(this).attr('src', 'https://raw.githubusercontent.com/WarmMateTea/ImageRepo/f85728639824166be9728567f69f6f2289494d2e/visibility_FILL0_wght400_GRAD0_opsz48.svg')
        }
        else {
            $(this).removeClass('show');
            $(this).addClass('hide');
            $(this).attr('src', 'https://raw.githubusercontent.com/WarmMateTea/ImageRepo/f85728639824166be9728567f69f6f2289494d2e/visibility_off_FILL0_wght400_GRAD0_opsz48.svg');
            $(this).siblings('.sub').children('.textbox').attr('type', 'password');
        }
    })*/
});