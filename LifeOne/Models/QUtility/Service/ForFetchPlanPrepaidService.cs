﻿using LifeOne.Models.QUtility.API;
using LifeOne.Models.QUtility.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LifeOne.Models.QUtility.Service
{
    public class ForFetchPlanPrepaidService
    {
        public ResponseModel GetAllForFetchPlanPrepaidService(RequestModel _obj)
        {
            try
            {
                string _url = "ForFetchPlanPrepaid";   //BaseUrl+API URL                       
                ResponseModel _rsponsedetails = GenericAPI.sendRequestToUtilityAPIPost(_obj, _url);
                return _rsponsedetails;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}