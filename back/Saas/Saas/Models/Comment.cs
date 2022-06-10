using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Saas.Models
{
    public class Comment
    {
        public int Id { get; set; }
    
        public string Img { get; set; }
        [Required]
        public string Desc { get; set; }
        [Required]
        public string Name { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }
    }
}
