﻿using LifeOne.Models.API;
using LifeOne.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LifeOne.Models.AssociateManagement.AssociateEntity
{
    public class AssociateTeam
    {
        public int Fk_UpId { get; set; }
        public string SearchLoginId { get; set; }
        public List<DirectAPI> Directs { get; set; }
        public List<DownlineAPI> Downlines { get; set; }
        public Pager Pager { get; set; }
    }
}