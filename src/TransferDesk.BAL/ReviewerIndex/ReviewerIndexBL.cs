﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using TransferDesk.Contracts.ReviewerIndex.ComplexTypes;
using TransferDesk.DAL.ReviewerIndex.DataContext;
using TransferDesk.Contracts.ReviewerIndex.Entities;
using TransferDesk.DAL.ReviewerIndex.Repositories;
using TransferDesk.Contracts.Logging;

namespace TransferDesk.BAL.ReviewerIndex
{
    public class ReviewerIndexBL:IDisposable
    {
       
        private ReviewerIndexRepository _reviewerIndexRepository;
        private ReviewerIndexDBRepositoriesReadSite _reviewerIndexDBContext;
        private ILogger _logger;
        private bool disposed = false;      
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {

                }
            }
            this.disposed = true;
        }

        public ReviewerIndexBL(string conString, ILogger Logger)
        {   
            _reviewerIndexRepository = new ReviewerIndexRepository(conString);
            _reviewerIndexDBContext = new ReviewerIndexDBRepositoriesReadSite(conString, Logger);
        }

        public void EditManuscriptTitle(int titleId, string mScriptID, string name)
        {
            _reviewerIndexDBContext.UpdateManuscriptTitle(titleId, mScriptID, name);
        }

        public int SaveReviewerProfile(SaveReviewerProfile_Result profileDetails)
        {
            profileDetails = AddAffiliationIntoMaster(profileDetails);
            if (profileDetails.StreetName == null)
            {
                profileDetails.StreetName = "";
            }
            if (profileDetails.MiddleName == null)
            {
                profileDetails.MiddleName = "";
            }
            int reviewerID = _reviewerIndexDBContext.spSaveUpdateReviewerDetails(profileDetails.Initials, profileDetails.LastName, profileDetails.FirstName, profileDetails.MiddleName, profileDetails.FirstName, profileDetails.ReviewerID, profileDetails.InstituteID, profileDetails.DeptID, profileDetails.StreetName, profileDetails.CityId, profileDetails.NoOfPublication, profileDetails.CreatedBy);

            if (profileDetails.ReviewerEmails.Count > 0)
            {
                _reviewerIndexDBContext.DeleteReviewerMailByReviewerId(reviewerID);
            }
            foreach (var item in profileDetails.ReviewerEmails)
            {
                if (item.IsActive == true) { AddToReviewerMailLink(item.Email, reviewerID, profileDetails.CreatedBy); }               
            }
            if (profileDetails.ReferenceLink.Count > 0)
            {
                _reviewerIndexDBContext.DeleteReferenceLinkByReviewerId(reviewerID);
            }
            foreach (var item in profileDetails.ReferenceLink)
            {
                if (item.IsActive == true) { AddReferenceReviewerlink(item.ReferenceLink, reviewerID, profileDetails.CreatedBy); }               
            }
            if (profileDetails.Journal.Count > 0)
            {
                _reviewerIndexDBContext.DeleteJournalByReviewerId(reviewerID);
            }
            foreach (var item in profileDetails.Journal)
            {
                if (item.IsActive == true) { AddJournalReviewerLink(item.JournalID, reviewerID, profileDetails.CreatedBy); }                
            }
            if ( profileDetails.AreaOfExpReviewerlink.Count > 0)
            {
                _reviewerIndexDBContext.UpdateAreaOfExpReviewerlink(reviewerID);
            }
          
            foreach (var item in profileDetails.AreaOfExpReviewerlink)
            {
                item.ReviewerMasterID = reviewerID;                
                if (item.IsActive == true) { AddAreaOfExpReviewerlink(item, profileDetails.CreatedBy); }
            }
            if (profileDetails.TitleMaster.Count > 0)
            {
                _reviewerIndexDBContext.DeleteTitleReviewerByReviewerId(reviewerID);
            }
            foreach (var item in profileDetails.TitleMaster)
            {
                if (item.IsActive == true && item.ID <= 0)
                {
                    item.CreatedBy = profileDetails.CreatedBy;
                    var titleId = AddTitleMaster(item);
                    TitleReviewerlink objTitleReviewerlink = new TitleReviewerlink();
                    objTitleReviewerlink.TitleMasterID = titleId;
                    objTitleReviewerlink.ReviewerMasterID = reviewerID;
                    objTitleReviewerlink.CreatedBy = profileDetails.CreatedBy;
                    _reviewerIndexRepository.AddTitleReviewerlink(objTitleReviewerlink);              
                 
                }
                else
                {
                    if (item.IsActive == true && item.ID > 0 && reviewerID > 0)
                    {
                        TitleReviewerlink objTitleReviewerlink = new TitleReviewerlink();
                        objTitleReviewerlink.TitleMasterID = item.ID;
                        objTitleReviewerlink.ReviewerMasterID = reviewerID;
                        _reviewerIndexRepository.AddTitleReviewerlink(objTitleReviewerlink);
                    }                    
                }
              
            }
            return reviewerID;
        }

        
        private SaveReviewerProfile_Result AddAffiliationIntoMaster(SaveReviewerProfile_Result profileDetails)
        {   
            if (profileDetails.InstituteID == null)
            {
                profileDetails.InstituteID = AddInstituteMaster(profileDetails.InstituteName, profileDetails.CreatedBy);
            }
            if (profileDetails.DeptID == null)
            {
                profileDetails.DeptID = AddDepartmentMaster(profileDetails.DepartmentName, profileDetails.CreatedBy);
            }
            if (profileDetails.CountryID == null)
            {
                profileDetails.CountryID = AddCountryMaster(profileDetails.Country);
            }
            if (profileDetails.StateId == null && profileDetails.CountryID > 0)
            {
                profileDetails.StateId = AddStateMaster(profileDetails.State, Convert.ToInt32(profileDetails.CountryID));
            }
            if (profileDetails.CityId == null && profileDetails.StateId > 0)
            {
                profileDetails.CityId = AddCityMaster(profileDetails.City, Convert.ToInt32(profileDetails.StateId));
            }
            return profileDetails;
        }

        private int AddInstituteMaster(string instituteName, string createdBy)
        {
            InstituteMaster objInstituteMaster = new InstituteMaster();
            objInstituteMaster.Name = instituteName;
            objInstituteMaster.CreatedBy = createdBy;            
            return Convert.ToInt32(_reviewerIndexRepository.AddInstituteMaster(objInstituteMaster));             
        }
        private int AddCountryMaster(string countryName)
        {
            Location objLocation = new Location();
            objLocation.Name = countryName;
            objLocation.Locationtype = 0;
            objLocation.Parentid = 0;          
            return Convert.ToInt32(_reviewerIndexRepository.AddLocation(objLocation));             
        }
        private int AddStateMaster(string stateName, int parentid)
        {
            Location objLocation = new Location();
            objLocation.Name = stateName;
            objLocation.Locationtype = 1;
            objLocation.Parentid = parentid;
            return Convert.ToInt32(_reviewerIndexRepository.AddLocation(objLocation));
        }
        private int AddCityMaster(string cityName, int parentid)
        {
            Location objLocation = new Location();
            objLocation.Name = cityName;
            objLocation.Locationtype = 2;
            objLocation.Parentid = parentid;
            return Convert.ToInt32(_reviewerIndexRepository.AddLocation(objLocation));
        }
        private int AddDepartmentMaster(string departmentName, string createdBy)
        {
            DepartmentMaster objDepartmentMaster = new DepartmentMaster();
            objDepartmentMaster.Name = departmentName;
            objDepartmentMaster.CreatedBy = createdBy;
            return Convert.ToInt32(_reviewerIndexRepository.AddDepartmentMaster(objDepartmentMaster));             
        }
        private void AddToReviewerMailLink(string emailAddress, int reviewerID, string createdBy)
        {
            ReviewerMailLink objReviewerMailLink = new ReviewerMailLink();
            objReviewerMailLink.Email = emailAddress;
            objReviewerMailLink.ReviewerMasterID = reviewerID;
            objReviewerMailLink.CreatedBy = createdBy;
            _reviewerIndexRepository.AddReviewerMailLink(objReviewerMailLink);
        }
        private int AddTitleMaster(TitleReviewerlinkMaster_Result result)
        {
            TitleMaster objTitleMaster = new TitleMaster();
            objTitleMaster.MScriptID = result.MScriptID;
            objTitleMaster.TitleName = result.Name;
            objTitleMaster.Name = result.Name;
            objTitleMaster.TitleID = result.ID;
            objTitleMaster.CreatedBy = result.CreatedBy;
            return Convert.ToInt32(_reviewerIndexRepository.AddTitleMaster(objTitleMaster));
        }      
        private void AddReferenceReviewerlink(string refLink, int reviewerID, string createdBy)
        {
            ReferenceReviewerlink objReferenceReviewerlink = new ReferenceReviewerlink();
            objReferenceReviewerlink.ReferenceLink = refLink;
            objReferenceReviewerlink.ReviewerMasterID = reviewerID;
            objReferenceReviewerlink.CreatedBy = createdBy;
            _reviewerIndexRepository.AddReferenceReviewerlink(objReferenceReviewerlink);
        }
        private void AddJournalReviewerLink(int journalId, int reviewerId, string createdBy)
        {
            JournalReviewerLink objJournalReviewerLink = new JournalReviewerLink ();
            objJournalReviewerLink.JournalID = journalId;
            objJournalReviewerLink.ReviewerMasterID = reviewerId;
            objJournalReviewerLink.CreatedBy = createdBy;
            _reviewerIndexRepository.AddJournalReviewerLink(objJournalReviewerLink);           
        }       

        private void AddAreaOfExpReviewerlink(AreaOfExpReviewerlink_Result result, string createdBy)
        {
            AreaOfExpReviewerlink objPID = new AreaOfExpReviewerlink ();
            objPID.AreaOfExpertiseMasterID = result.PID;
            objPID.ReviewerMasterID = result.ReviewerMasterID;
            objPID.CreatedBy = createdBy;
            _reviewerIndexRepository.AddAreaOfExpReviewerlink(objPID);

            AreaOfExpReviewerlink objSID = new AreaOfExpReviewerlink();
            objSID.AreaOfExpertiseMasterID = result.SID;
            objSID.ReviewerMasterID = result.ReviewerMasterID;
            objSID.CreatedBy = createdBy;
            _reviewerIndexRepository.AddAreaOfExpReviewerlink(objSID);

            AreaOfExpReviewerlink objTID = new AreaOfExpReviewerlink();
            objTID.AreaOfExpertiseMasterID = result.TID;
            objTID.ReviewerMasterID = result.ReviewerMasterID;
            objTID.CreatedBy = createdBy;
            _reviewerIndexRepository.AddAreaOfExpReviewerlink(objTID);
        }
      
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
      
    }




}
