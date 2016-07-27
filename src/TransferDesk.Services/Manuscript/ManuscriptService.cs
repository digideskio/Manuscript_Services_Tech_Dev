using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Entities = TransferDesk.Contracts.Manuscript.Entities;
using TransferDesk.Contracts.Manuscript.DTO;
using TransferDesk.BAL.Manuscript;
using TransferDesk.Services.Manuscript.ViewModel;
using TransferDesk.Contracts.Logging;
using System.Collections;

//todo: a seperate "web adapter" for service that will allow modelstate to cross will be created.

namespace TransferDesk.Services.Manuscript
{
    public class ManuscriptService
    {
        public String _ConStringRead { get; set; }

        public String _ConStringWrite { get; set; }

        public ManuscriptScreeningBL _manuscriptScreeningBL { get; set; }

        private ILogger _logger;

        public ManuscriptService(ILogger Logger)
        {
            //empty constructor  
            _logger = Logger;
        }

        public ManuscriptService(String ConStringRead, String ConStringWrite, ILogger Logger)
        {
            _ConStringRead = ConStringRead;
            _ConStringWrite = ConStringWrite;
            _logger = Logger;
            CreateManuscriptScreeningBL();
        }

        public void CreateManuscriptScreeningBL()
        {
            _manuscriptScreeningBL = new ManuscriptScreeningBL(_ConStringRead, _ConStringWrite);
        }

        public void CreateManuscriptServiceComponents()
        {
            CreateManuscriptScreeningBL();
        }

        public ManuscripScreeningVM GetManuscriptScreeningVM()
        {
            return GetManuscriptScreeningDefaultVM();
        }

        public ManuscripScreeningVM GetManuscriptScreeningVM(int manuscriptID)
        {
            return new ManuscripScreeningVM(_manuscriptScreeningBL.GetManuscriptScreeningDTO(manuscriptID));
        }

        public ManuscripScreeningVM GetManuscriptScreeningDefaultVM()
        {
            ManuscriptScreeningDTO manuscriptScreeningDTO = _manuscriptScreeningBL.GetManuscriptScreeningDefaultDTO();
            manuscriptScreeningDTO.Manuscript.UserID = System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
            return new ManuscripScreeningVM(manuscriptScreeningDTO);
        }

        public ManuscriptBookScreeningVm GetManuscriptBookScreeningVm(int bookId)
        {
            return new ManuscriptBookScreeningVm(_manuscriptScreeningBL.GetManuscriptBoookScreeningDTO(bookId));
        }

        public ManuscriptBookScreeningVm GetManuscriptBookScreeningDefaultVm()
        {
            ManuscriptBookScreeningDTO manuscriptScreeningDTO = _manuscriptScreeningBL.GetManuscriptBookScreeningDefaultDTO();
            return new ManuscriptBookScreeningVm(manuscriptScreeningDTO);
        }

        public void ValidateManuscriptScreening(IDictionary<string, string> dataErrors, ManuscriptScreeningDTO manuscriptScreeningDTO)
        {
            Entities.Manuscript manuscript = manuscriptScreeningDTO.Manuscript;
            if (manuscript.JournalID == null)
                dataErrors.Add("JournalID", "JournalTitle is required.");
            if (manuscript.ArticleTypeID == null)
                dataErrors.Add("ArticleTypeID", "Article Type is required.");
            if (manuscript.ArticleTitle == null)
                dataErrors.Add("ArticleTitle", "Article Title is required.");
            if (manuscript.StartDate == null)
                dataErrors.Add("StartDate", "Start Date is required.");
            if (manuscript.RoleID == null)
                dataErrors.Add("RoleID", "Role is required.");
            if (manuscript.UserID == null)
                dataErrors.Add("UserMasterID", "System UserID is required.");
            if (manuscript.Crosscheck_iThenticateResultID == null)
                dataErrors.Add("Crosscheck_iThenticateResultID", "Crosscheck iThenticateResult is required.");
            if (manuscript.Highest_iThenticateFromSingleSrc == null)
                dataErrors.Add("Highest_iThenticateFromSingleSrc", "Highest iThenticate(From SingleSource) is required.");
            if (manuscript.English_Lang_QualityID == null)
                dataErrors.Add("English_Lang_QualityID", "English Language Quality is required.");
            if (manuscript.Ethics_ComplianceID == null)
                dataErrors.Add("Ethics_ComplianceID", "Ethics Compliance is required.");
            if (manuscript.InitialSubmissionDate == null)
                dataErrors.Add("InitialSubmissionDate", "Initial Submission Date is required.");
            if (manuscript.CorrespondingAuthor == null)
                dataErrors.Add("CorrespondingAuthor", "Corresponding Author is required.");
            if (manuscript.CorrespondingAuthorEmail == null)
                dataErrors.Add("CorrespondingAuthorEmail", "Corresponding Author Email is required.");
            if (manuscript.CorrespondingAuthorAff == null)
                dataErrors.Add("CorrespondingAuthorAff", "Corresponding Author Aff. is required.");
            if (manuscript.OverallAnalysisID == null)
                dataErrors.Add("OverallAnalysis", "Overall Analysis is required.");
        }

        public bool SaveManuscriptScreeningVM(IDictionary<string, string> dataErrors, ManuscripScreeningVM manuscriptVM)
        {
            _logger.Log("trying to save MS Viewmodel, ID is " + manuscriptVM.ID);
            ManuscriptScreeningDTO manuscriptScreeningDTO = manuscriptVM.FetchDTO;
            _logger.Log("DTO fetched with id " + manuscriptScreeningDTO.Manuscript.ID);
            manuscriptScreeningDTO.CurrentUserID = System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
            _logger.Log("Current User ID " + manuscriptScreeningDTO.CurrentUserID);
            ValidateManuscriptScreening(dataErrors, manuscriptScreeningDTO);
            _logger.Log("errors (if any) " + dataErrors.ToString());
            if (dataErrors.Count == 0)
            {
                _manuscriptScreeningBL.SaveManuscriptScreening(manuscriptScreeningDTO, dataErrors);
                _logger.Log("Saved successfully with data errors (if any) " + dataErrors.ToString());
                return true;
            }
            else
            {
                _logger.Log("Not saved due to errors before save - " + dataErrors.ToString());
                return false;
            }
        }

        public bool SaveManuscriptBookScreeningVM(IDictionary<string, string> dataErrors, ManuscriptBookScreeningVm manuscriptVM)
        {
            _logger.Log("trying to save VModel Book Screening ID is " + manuscriptVM.BookScreeningID);
            ManuscriptBookScreeningDTO manuscriptBookScreeningDTO = manuscriptVM.FetchDTO;
            _logger.Log("DTO fetched with Book screening Id - " + manuscriptBookScreeningDTO.ManuscriptBookScreening.ID);
            manuscriptBookScreeningDTO.CurrentUserID = System.Web.HttpContext.Current.User.Identity.Name.Replace("SPRINGER-SBM\\", "");
            _logger.Log("Current User ID " + manuscriptBookScreeningDTO.CurrentUserID);
            manuscriptBookScreeningDTO.ManuscriptBookScreening.BookLoginID = manuscriptVM.BookLoginID;
            _logger.Log("Manuscript screening VM  Book Login Id - " + manuscriptVM.BookLoginID);
            manuscriptBookScreeningDTO.ManuscriptBookScreening.ID = manuscriptVM.BookScreeningID;
            _logger.Log("manuscript VM book screening ID assigned to DTO Manu. Book Screening DTO  as " + manuscriptVM.BookScreeningID);
            ValidateManuscriptBookScreening(dataErrors, manuscriptBookScreeningDTO);
            _logger.Log("Data errors (if any) after validation of Manu. Book Screening DTO");
            if (dataErrors.Count == 0)
            {
                _manuscriptScreeningBL.SaveManuscriptBookScreening(manuscriptBookScreeningDTO, dataErrors);
                _logger.Log("manu. book screening  saved successfully");
                return true;
            }
            else
            {
                _logger.Log("manu. book screening not saved due to errors " + dataErrors.ToString());
                return false;
            }
        }

        private void ValidateManuscriptBookScreening(IDictionary<string, string> dataErrors, ManuscriptBookScreeningDTO manuscriptBookScreeningDTO)
        {
            var bookScreening=new Entities.ManuscriptBookScreening();
            bookScreening = manuscriptBookScreeningDTO.ManuscriptBookScreening;
            if(bookScreening.BookLoginID==0)
                dataErrors.Add("BookLoginID", "Book login details are required.");
            if (bookScreening.RollID == null)
                dataErrors.Add("RollID", "Role is required.");
            if (bookScreening.Crosscheck_iThenticateResultID == null)
                dataErrors.Add("Crosscheck_iThenticateResultID", "Crosscheck iThenticateResult is required.");
            if (bookScreening.Highest_iThenticateFromSingleSrc == null)
                dataErrors.Add("Highest_iThenticateFromSingleSrc", "Highest iThenticate(From SingleSource) is required.");
            if (bookScreening.English_Lang_QualityID == null)
                dataErrors.Add("English_Lang_QualityID", "English Language Quality is required.");
            if (bookScreening.Ethics_ComplianceID == null)
                dataErrors.Add("Ethics_ComplianceID", "Ethics Compliance is required.");
            if (bookScreening.CorrespondingAuthor == null)
                dataErrors.Add("CorrespondingAuthor", "Corresponding Author is required.");
            if (bookScreening.OverallAnalysisID == null)
                dataErrors.Add("OverallAnalysis", "Overall Analysis is required.");
        }

        public void IsBookSaveOrSubmit(ManuscriptBookScreeningVm manuscriptBookScreeningVm, string associateCommand,
            string qualityCommand)
        {
            if (associateCommand != null)
            {
                switch (associateCommand.ToLower())
                {
                    case "save":                        
                        manuscriptBookScreeningVm.IsAssociateFinalSubmit = false;
                        break;
                    case "submit":
                        manuscriptBookScreeningVm.AssociateFinalSubmitDate = DateTime.Now;
                        manuscriptBookScreeningVm.IsAssociateFinalSubmit = true;
                        break;
                }
            }
            if (qualityCommand != null)
            {
                switch (qualityCommand.ToLower())
                {
                    case "save":
                        manuscriptBookScreeningVm.IsQualityFinalSubmit = false;
                        break;
                    case "submit":
                        manuscriptBookScreeningVm.QualityFinalSubmitDate = DateTime.Now;
                        manuscriptBookScreeningVm.IsQualityFinalSubmit = true;
                        break;
                }
            }
        }

    }
}
