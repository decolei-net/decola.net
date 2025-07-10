using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Decolei.net.Models
{
    // [Table("Viajante")]
    // Explicação: Este atributo liga a classe "Viajante" à tabela "Viajante" no banco de dados.
    // É essencial quando o nome da classe C# é diferente do nome da tabela no SQL.
    [Table("Viajante")]
    public class Viajante
    {
        // [Key]
        // Explicação: Marca a propriedade "Id" como a chave primária (Primary Key) da tabela.
        [Key]
        // [Column("Viajante_Id")]
        // Explicação: Mapeia esta propriedade "Id" para a coluna "Viajante_Id" no banco de dados.
        [Column("Viajante_Id")]
        public int Id { get; set; }

        // [Column("Reserva_Id")]
        // Explicação: Mapeia para a coluna que armazena a chave estrangeira da Reserva.
        [Column("Reserva_Id")]
        public int Reserva_Id { get; set; }

        // [Required]
        // Explicação: Corresponde ao "NOT NULL" no SQL. Garante que esta propriedade
        // não pode ser nula, tanto na validação do .NET quanto na geração de queries.
        [Required]
        // [StringLength(100)]
        // Explicação: Corresponde ao "VARCHAR(100)". Define o tamanho máximo da string,
        // útil para validações automáticas no frontend e backend.
        [StringLength(100)]
        [Column("Viajante_Nome")]
        public string? Nome { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Viajante_Documento")]
        public string? Documento { get; set; }

        // --- PROPRIEDADE DE NAVEGAÇÃO ---
        // [ForeignKey("Reserva_Id")]
        // Explicação: Informa ao Entity Framework que a propriedade "Reserva_Id" (acima)
        // é a chave estrangeira para a propriedade de navegação "Reserva" (abaixo).
        [ForeignKey("Reserva_Id")]
        // 'virtual'
        // Explicação: A palavra-chave 'virtual' permite que o EF Core use um recurso chamado
        // "Lazy Loading" (Carregamento Preguiçoso), onde os dados da reserva só são
        // carregados do banco quando você acessa esta propriedade pela primeira vez.
        public virtual Reserva Reserva { get; set; } = null!;
    }
}
