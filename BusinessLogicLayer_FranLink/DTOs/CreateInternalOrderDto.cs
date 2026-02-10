using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessLogicLayer_FranLink.DTOs
{
    public class CreateInternalOrderDto
    {
        [Required]
        public int FranchiseStoreId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public List<CreateInternalOrderItemDto> Items { get; set; } = new();
    }
}
