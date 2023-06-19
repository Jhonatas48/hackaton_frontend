using hackaton.Models;
using System.ComponentModel.DataAnnotations;

namespace frontend_hackaton.Models
{
    public class Schedule
    {
        public int ScheduleID { get; set; }

        [Required(ErrorMessage = "O campo é obrigatório.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "O campo é obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime DataInicial { get; set; }

        [Required(ErrorMessage = "O campo é obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime DataFinal { get; set; }

        //Campos de navegação
        public User User { get; set; }
        public int UserId { get; set; }
    }
}
