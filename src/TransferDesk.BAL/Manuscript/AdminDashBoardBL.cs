using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferDesk.Contracts.Manuscript.DTO;
using TransferDesk.DAL.Manuscript.Repositories;
using TransferDesk.DAL.Manuscript.DataContext;
namespace TransferDesk.BAL.Manuscript
{
    public class AdminDashBoardBL
    {       
        public AdminDashBoardReposistory _adminDashBoardDBReadSide { get; set; }

        public AdminDashBoardBL(string conString)
        {
            _adminDashBoardDBReadSide = new AdminDashBoardReposistory(conString);
        }

        public bool AllocateManuscriptToUser(AdminDashBoardDTO adminDashBoardDTO)
        {
            if (adminDashBoardDTO.JobType.ToLower() == "book")
            {
                return _adminDashBoardDBReadSide.AllocateAssociateToChapter(adminDashBoardDTO);
            }
            else
            {
                return _adminDashBoardDBReadSide.AllocateAssociateToMSID(adminDashBoardDTO);    
            }
            
        }

        public bool updateManuscriptLoginDeatils(AdminDashBoardDTO adminDashBoardDTO)
        {
            if (adminDashBoardDTO.JobType.ToLower() == "book")
            {
                    return _adminDashBoardDBReadSide.UnallocateAssociateUserFromChapter(adminDashBoardDTO) ? true : false;
            }
            else
            {
                return _adminDashBoardDBReadSide.UnallocateAssociateUser(adminDashBoardDTO)?true:false;

            }
        }

        public bool updateManuscriptLoginDeatilsForHold(AdminDashBoardDTO adminDashBoardDTO)
        {
            if (adminDashBoardDTO.JobType.ToLower() == "book")
            {
                return _adminDashBoardDBReadSide.OnHoldBookChapter(adminDashBoardDTO) ? true : false;
            }
            else
            {
                return _adminDashBoardDBReadSide.HoldMSIDForJob(adminDashBoardDTO) ? true : false;
            }
        }
    }
}
