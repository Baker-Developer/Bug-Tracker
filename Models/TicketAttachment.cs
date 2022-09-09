﻿using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public string TicketId { get; set; }

        [DisplayName("File Data")]
        public DateTimeOffset Created { get; set; }

        [DisplayName("Team Member")]
        public string UserId { get; set; }

        [DisplayName("File Description")]
        public string Description { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile FormFile { get; set; }

        [DisplayName("File Name")]
        public string FileName { get; set; }
       
        public byte[] FileData { get; set; }

        [DisplayName("File Extention")]
        public string FileContentType { get; set; }

        //Navigation Properties
        public virtual Ticket Ticket { get; set; }
        public virtual BugTrackerUser User { get; set; }
    }
}