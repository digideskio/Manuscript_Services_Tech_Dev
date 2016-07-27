﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TransferDesk.BAL.ReviewerIndex;
using TransferDesk.Contracts.Logging;
using TransferDesk.Contracts.ReviewerIndex.ComplexTypes;
using TransferDesk.DAL.Manuscript.Repositories;
using TransferDesk.DAL.ReviewerIndex.DataContext;
using TransferDesk.DAL.ReviewerIndex.Repositories;
using TransferDesk.Services.Manuscript;

namespace TransferDesk.MS.Web.Controllers
{
    public class ReviewerIndexController : Controller
    {
        private ReviewerIndexDBRepositoriesReadSite _reviewerIndexDBRepositoriesReadSite;//= new ReviewerIndexDBRepositoriesReadSite();
        private ReviewerIndexDBContext _reviewerIndexDBContext;
        private readonly ManuscriptDBRepositoryReadSide _manuscriptDBRepositoryReadSide;
        private ILogger _logger;

        public ReviewerIndexController(ILogger Logger)
        {
            try
            {
                string conString = string.Empty;
                conString = Convert.ToString(ConfigurationManager.AppSettings["dbTransferDeskService"]);
                _reviewerIndexDBRepositoriesReadSite = new ReviewerIndexDBRepositoriesReadSite(conString, Logger);
                _reviewerIndexDBContext = new ReviewerIndexDBContext(conString);
                _manuscriptDBRepositoryReadSide = new ManuscriptDBRepositoryReadSide(conString);
                _logger = Logger;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                throw;
            }

        }

        //
        // GET: /ReviewerIndex/
        public ActionResult Index()
        {
            return View();
        }
        /// <summary>
        /// This method will open Reviewer Index Search page
        /// </summary>
        /// <returns></returns>
        public ActionResult ReviewerIndexSearch()
        {

            try
            {

                if (Session["UserName"] == null)
                {
                    var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                    string userName = _manuscriptDBRepositoryReadSide.EmployeeName(userId);
                    Session["UserName"] = userName;
                    _logger.Log(userId, "INFO: UserId-" + userId + "open reviewer index Search.");
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex, null);
                throw;
            }

        }

        /// <summary>
        /// This action will open profile page
        /// if reviewerId == 0 its means new profile page
        /// </summary>
        /// <param name="reviewerId">reviewerId</param>
        /// <returns></returns>
        public ActionResult ReviewerProfile(string reviewerId)
        {
            try
            {
                if (Session["UserName"] == null)
                {
                    var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                    string userName = _manuscriptDBRepositoryReadSide.EmployeeName(userId);
                    Session["UserName"] = userName;
                    _logger.Log(userId, "INFO: UserId-" + userId + " open reviewer profile.");
                }
                ViewBag.ReviewerId = reviewerId.Replace("N", "");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }

        /// <summary>
        /// This method will fetch data/result as per input parameters  
        /// </summary>
        /// <param name="fromrow">first record row number</param>
        /// <param name="torow">last record row numbe</param>
        /// <param name="pagesize">page size</param>
        /// <param name="searchOne">searchOne</param>
        /// <param name="searchTwo">searchTwo</param>
        /// <param name="minValue">minValue</param>
        /// <param name="maxValue">maxvalue</param>
        /// <param name="SearchOneVal"></param>
        /// <param name="ConditionVal">& ||</param>
        /// <param name="SearchTwoVal"></param>
        /// <param name="NewSearch">true/false</param>
        /// <returns>List of filterd records</returns>
        public ActionResult GetReviewerIndexData(int fromrow, int torow, int pagesize, string searchOne, string searchTwo, string minValue, string maxValue, string SearchOneVal, string ConditionVal, string SearchTwoVal, bool NewSearch)
        {
            try
            {
                long firstRow = 1;
                long lastRow = 20;
                int totalCount = 0;
                List<pr_GetReviewersList> searchResult = new List<pr_GetReviewersList>();
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                if (minValue == string.Empty)
                {
                    minValue = null;
                }
                if (maxValue == string.Empty)
                {
                    maxValue = null;
                }

                if (NewSearch || Session["SearchResult"] == null)
                {
                    var strReviewersIDs = _reviewerIndexDBRepositoriesReadSite.GetReviewerIds(searchOne, searchTwo, ConditionVal, SearchOneVal, SearchTwoVal);

                    var profileList = _reviewerIndexDBRepositoriesReadSite.GetReviewersLists(strReviewersIDs, Convert.ToInt32(minValue), Convert.ToInt32(maxValue)).ToList();
                    Session["SearchResult"] = profileList;
                }
                searchResult = Session["SearchResult"] as List<pr_GetReviewersList>;

                var result = searchResult.Skip(fromrow - 1).Take(pagesize).ToList();
                if (result.Count > 0)
                {
                    firstRow = result.FirstOrDefault().RowNo;
                    lastRow = result.LastOrDefault().RowNo;
                    totalCount = searchResult.Count();
                }

                var jsonData = new
                {
                    totalcount = totalCount,
                    firstrownumber = firstRow,
                    lastrownumber = lastRow,
                    records = result
                };
                _logger.Log(userId, "Info: Method Name - GetReviewerIndexData , Parameters > searchOne: " + searchOne + ",searchTwo: " + searchTwo + ", ConditionVal: " + ConditionVal + ", SearchOneVal: " + SearchOneVal + ", SearchTwoVal: " + SearchTwoVal + "Min Value: " + minValue + "Max Value: " + maxValue + "Total Result Count: " + result.Count);
                _logger.Log(userId, "Info: Method Name - GetReviewerIndexData , Result Count : " + result.Count + "totalcount: " + totalCount + "firstRow: " + firstRow + "lastRow: " + lastRow);
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        /// <summary>
        /// Its will load data on View/Edit 
        /// </summary>
        /// <param name="reviewerId">reviewerId</param>
        /// <returns></returns>
        public ActionResult LoadProfileData(int reviewerId)
        {
            try
            {
                var profileData = _reviewerIndexDBRepositoriesReadSite.GetReviewerDetails(reviewerId);
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var jsonData = new
                {
                    records = profileData
                };
                _logger.Log(userId, "Info: Method Name - LoadProfileData : ReviewerId-" + reviewerId);
                return Json(jsonData, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }



        }
        /// <summary>
        /// AutoComplete functionality Country 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult AutoCompleteCountry(string term)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var countryList = _reviewerIndexDBContext.Location.Where(o => o.Parentid == 0 && o.Name.ToLower().Contains(term.ToLower())).ToList();
                _logger.Log(userId, "Info: Method Name - AutoCompleteCountry: term-" + term + "country count" + countryList.Count);
                return Json(countryList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }
        /// <summary>
        /// AutoComplete functionality State 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="parentPrefix"></param>
        /// <returns></returns>
        public JsonResult AutoCompleteState(string term, int parentPrefix)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                if (parentPrefix > 0)
                {
                    var stateList = _reviewerIndexDBContext.Location.Where(o => o.Parentid == parentPrefix && o.Name.ToLower().Contains(term.ToLower())).ToList();
                    _logger.Log(userId, "Info : Method Name - AutoCompleteState : term-" + term + "parentPrefix : " + parentPrefix + "& state count" + stateList.Count);
                    return Json(stateList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Log(userId, "Info : Method Name - AutoCompleteState : term-" + term + "parentPrefix : " + parentPrefix + "& state count" + null);
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }


        }

        /// <summary>
        ///  AutoComplete functionality City 
        /// </summary>
        /// <param name="term"></param>
        /// <param name="parentPrefix"></param>
        /// <returns></returns>
        public JsonResult AutoCompleteCity(string term, int parentPrefix)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                if (parentPrefix > 0)
                {
                    var cityList = _reviewerIndexDBContext.Location.Where(o => o.Parentid == parentPrefix && o.Name.ToLower().Contains(term.ToLower())).ToList();
                    _logger.Log(userId, "Info : Method Name - AutoCompleteCity : term-" + term + "parentPrefix : " + parentPrefix + "& city count" + cityList.Count);
                    return Json(cityList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logger.Log(userId, "Info : Method Name - AutoCompleteCity : term-" + term + "parentPrefix : " + parentPrefix + "& city count" + null);
                    return Json(false, JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }
        /// <summary>
        ///  AutoComplete functionality Institute 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult AutoCompleteInstitute(string term)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var instituteList = _reviewerIndexDBContext.InstituteMaster.Where(o => o.Name.ToLower().Contains(term.ToLower())).ToList();
                _logger.Log(userId, "Info : Method Name - AutoCompleteInstitute : term-" + term + "& institute count" + instituteList.Count);
                return Json(instituteList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }
        /// <summary>
        ///  AutoComplete functionality Departmnet 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult AutoCompleteDepartment(string term)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var departmentList = _reviewerIndexDBContext.DepartmentMaster.Where(o => o.Name.ToLower().Contains(term.ToLower())).ToList();
                _logger.Log(userId, "Info : Method Name - AutoCompleteDepartment : term-" + term + "& Department count" + departmentList.Count);
                return Json(departmentList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        /// <summary>
        ///  AutoComplete functionality ManuScript Title 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult AutoCompleteManuscriptTitle(string term)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var titleList = _reviewerIndexDBContext.TitleMaster.Where(o => o.Name.ToLower().Contains(term.ToLower())).ToList();
                _logger.Log(userId, "Info : Method Name - AutoCompleteManuscriptTitle : term-" + term + "& titel count" + titleList.Count);
                return Json(titleList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        /// <summary>
        ///  AutoComplete functionality Journal 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public JsonResult GetJournal(string term)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                if (!string.IsNullOrEmpty(term))
                {
                    var JournalList = _reviewerIndexDBContext.Journal.Where(o => o.JournalTitle.ToLower().Contains(term.ToLower())).ToList();
                    _logger.Log(userId, "Info : Method Name - GetJournal : term-" + term + "& Journal count" + JournalList.Count);
                    return Json(JournalList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var JournalList = _reviewerIndexDBContext.Journal.ToList();
                    _logger.Log(userId, "Info : Method Name - GetJournal : term-" + term + "& Journal count" + JournalList.Count);
                    return Json(JournalList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }

        public JsonResult GetTask()
        {
            var taskList = _reviewerIndexDBContext.StatusMaster.Where(o => o.StatusCode == "TaskStatus").ToList();
            return Json(taskList, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// It will fetch all PrimaryExpertise
        /// </summary>
        /// <returns></returns>
        public JsonResult GetPrimaryExpertise()
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var primaryExpertiesList = _reviewerIndexDBContext.AreaOfExpertiseMaster.Where(o => o.ParentID == 0).ToList();
                _logger.Log(userId, "Info : Method Name - GetPrimaryExpertise : PrimaryExpertise count" + primaryExpertiesList.Count);
                return Json(primaryExpertiesList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        /// <summary>
        ///  It will fetch all Secondary Expertise
        /// </summary>
        /// <param name="primaryParentID"></param>
        /// <returns></returns>
        public JsonResult GetSecondaryExpertise(int primaryParentID)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var secondaryExpertiesList = _reviewerIndexDBContext.AreaOfExpertiseMaster.Where(o => o.ParentID == primaryParentID).ToList();
                _logger.Log(userId, "Info : Method Name - GetSecondaryExpertise : SecondaryExpertise count" + secondaryExpertiesList.Count);
                return Json(secondaryExpertiesList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }
        /// <summary>
        ///  It will fetch all Tertiary Expertise
        /// </summary>
        /// <param name="secondaryParentID"></param>
        /// <returns></returns>
        public JsonResult GetTertiaryExpertise(int secondaryParentID)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var tertiaryExpertiesList = _reviewerIndexDBContext.AreaOfExpertiseMaster.Where(o => o.ParentID == secondaryParentID).ToList();
                _logger.Log(userId, "Info : Method Name - GetTertiaryExpertise : tertiaryExperties count" + tertiaryExpertiesList.Count);
                return Json(tertiaryExpertiesList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }

        }

        /// <summary>
        /// It will save all reviewer information on click of save button 
        /// </summary>
        /// <param name="profileDetails"></param>
        /// <returns></returns>
        public JsonResult SaveReviewerProfile(SaveReviewerProfile_Result profileDetails)
        {
            try
            {
                int reviewerId;
                string conString = Convert.ToString(ConfigurationManager.AppSettings["dbTransferDeskService"]);
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                profileDetails.CreatedBy = userId;
                using (ReviewerIndexBL reviewerIndex = new ReviewerIndexBL(conString, _logger))
                {
                    reviewerId = reviewerIndex.SaveReviewerProfile(profileDetails);
                }
                var profile = new JavaScriptSerializer().Serialize(profileDetails);
                _logger.Log(userId, "Info : Method Name - SaveReviewerProfile : reviewerId" + Convert.ToString(reviewerId) + "Profile" + profile.ToString());
                return Json(reviewerId, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }
        /// <summary>
        /// It will edit ManuScript Title and Manuscript by its titleId
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="mScriptID"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public JsonResult EditManuscriptTitle(int titleId, string mScriptID, string name)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                string conString = Convert.ToString(ConfigurationManager.AppSettings["dbTransferDeskService"]);
                using (ReviewerIndexBL reviewerIndex = new ReviewerIndexBL(conString, _logger))
                {
                    if (titleId > 0)
                    {
                        reviewerIndex.EditManuscriptTitle(titleId, mScriptID, name);
                    }
                }
                _logger.Log(userId, "Info : Method Name - EditManuscriptTitle : titleId: " + titleId + "mScriptID: " + mScriptID + "name " + name);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public JsonResult VerifyEmailAddress(string email, int reviewerId)
        {
            try
            {
                bool flag = false;
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                string conString = Convert.ToString(ConfigurationManager.AppSettings["dbTransferDeskService"]);
                var emailAddress = _reviewerIndexDBContext.ReviewerMailLink.Where(o => o.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (emailAddress != null)
                {
                    if (reviewerId != emailAddress.ReviewerMasterID)
                    {
                        flag = true;
                    }                    
                }
                _logger.Log(userId, "Info : Method Name - VerifyEmailAddress : email: " + email );
                return Json(flag, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


        /// <summary>
        /// Loads manuscript/reviewers details on click of Assign Manuscript button, txtManuscriptId textbox and after save/submit.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msid"></param>
        /// <param name="reviewersId"></param>
        /// <returns></returns>

        public JsonResult DisplayManuscriptDetails(string key = null, string msid = null, string reviewersId = null)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var resultList =
                    _reviewerIndexDBRepositoriesReadSite.DisplayManuscriptDetails(key, msid, reviewersId).ToList();

                _logger.Log(userId, "Info : Method Name - DisplayManuscriptDetails , Key : " + key + " , MSID : " + msid + " , reviewersId : " + reviewersId + " loaded successfully.");
                return Json(resultList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                StringBuilder str = new StringBuilder();
                str.Append("Exception occured while loading records on " + key + ". MSID : " + msid + " , reviewersId : " + reviewersId + ".");
                _logger.LogException(ex, str);
                throw;
            }

        }

        /// <summary>
        /// Check selected reviewer is duplicate or not for selected manuscript and journal. 
        /// </summary>
        /// <param name="journalid"></param>
        /// <param name="key"></param>
        /// <param name="msid"></param>
        /// <param name="reviewersId"></param>
        /// <returns></returns>
        public virtual int CheckDuplicates(int journalid, string key = null, string msid = null,
            string reviewersId = null)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var result = _reviewerIndexDBRepositoriesReadSite.CheckDuplicates(journalid, key, msid, reviewersId);

                _logger.Log(userId, "Info : Method Name - CheckDuplicates :Check successful.");
                return result;
            }
            catch (Exception ex)
            {
                StringBuilder str = new StringBuilder();
                str.Append("Exception occured while checking duplicate reviewer for given Manuscript : " + key + ", MSID : " + msid + " , journalid : " + reviewersId + ".");
                _logger.LogException(ex, str);
                throw;
            }
        }
        /// <summary>
        /// It save the below Master detials of manuscript in table [MSReviewersSuggestion].
        /// </summary>
        /// <param name="key"></param>
        /// <param name="msid"></param>
        /// <param name="ddlTask"></param>
        /// <param name="rollId"></param>
        /// <param name="jobType"></param>
        /// <param name="articleTitle"></param>
        /// <param name="ddlJournalId"></param>
        /// <param name="user"></param>
        /// <param name="reviewerMasterId"></param>
        /// <param name="isAssociateFinalSubmit"></param>
        /// <param name="msReviewerSuggestionId"></param>
        /// <returns></returns>
        public virtual int SaveReviewersSuggestion(string key, string msid, int ddlTask, int rollId, char jobType, string articleTitle, int ddlJournalId, string user, int reviewerMasterId, int isAssociateFinalSubmit, int msReviewerSuggestionId)
        {
            try
            {
                var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
                var result = _reviewerIndexDBRepositoriesReadSite.SaveReviewersSuggestion(key, msid, ddlTask, rollId,
                    jobType, articleTitle, ddlJournalId, userId, reviewerMasterId, isAssociateFinalSubmit,
                    msReviewerSuggestionId);

                _logger.Log(userId, "Info : Method Name - CheckDuplicates :Check successful.");
                return result;
            }
            catch (Exception ex)
            {
                StringBuilder str = new StringBuilder();
                str.Append("Exception occured while saving/submitting the reviewer details : reviewer ID - " + reviewerMasterId + ", jobType : " + jobType + " , articleTitle : " + articleTitle + " isAssociateFinalSubmit " + isAssociateFinalSubmit + ".");
                _logger.LogException(ex, str);
                throw;
            }
        }
        /// <summary>
        /// It save the below Master detials of manuscript in table [MSReviewersSuggestionInfo].
        /// </summary>
        /// <param name="key"></param>
        /// <param name="rowNum"></param>
        /// <param name="reviewerMasterId"></param>
        /// <param name="user"></param>
        /// <param name="chk"></param>
        /// <param name="isAssociateFinalSubmit"></param>
        /// <param name="articleTitle"></param>
        /// <param name="msid"></param>
        /// <returns></returns>
        public JsonResult SaveReviewersSuggestionInfo(string key, int rowNum, int reviewerMasterId, string user, bool chk, int isAssociateFinalSubmit, string articleTitle, string msid)
        {
            var userId = @System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
            try
            {
                _reviewerIndexDBRepositoriesReadSite.SaveReviewersSuggestionInfo(key, rowNum, reviewerMasterId, userId,
                    chk, isAssociateFinalSubmit, articleTitle, msid);
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                StringBuilder str = new StringBuilder();
                str.Append("Exception occured while saving/submitting the reviewer details : reviewer ID - " + reviewerMasterId + ", user : " + userId + " , articleTitle : " + articleTitle + " isAssociateFinalSubmit " + isAssociateFinalSubmit + ".");
                _logger.LogException(ex, str);
                throw;
            }
        }

    }

}